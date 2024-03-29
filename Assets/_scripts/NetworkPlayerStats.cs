﻿using UnityEngine;
using UnityEngine.UI;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;


public class NetworkPlayerStats : NetworkPlayerStatsBehavior
{
    private float max_health = 255;
    public float health = 255;//for debug purposes, its not being called from any other script that i made, its public just so that i can see it easier in inspector
    public Image healthBar;

    public uint server_id = 5;
    public NetWorker myNetWorker;
    public Text player_name;

    public float head_damage_multiplier = 1.5f;
    public float torso_damage_multiplier = 1.0f;
    public float limb_damage_multiplier = 0.75f;

    public float block_damage_reduction = 0.025f;

    public float fire1_cooldown = 1.5f;

    public float player_weapon_instantiation_cooldown = 2.0f;




    /*
     HOW DAMAGE WORKS RIGHT NOW:
     na serverju se detektira hit. trenutno edina skripta ki to dela je Weapon_Collider_handler, ki poklice tole metodo. ta metoda izracuna nov health od tega k je bil napaden. to vrednost poslje
     napadenmu playerju da si poupdejta health. ta player pol ko si je updejtov health poslje nov rpc vsem drugim da nj si nastavijo njegov health na njegov health. tud server(i can see how this is bad ampak za prototip me ne skrbi.)
         */
    public void take_weapon_damage_server_authority(float dmg, string tag_passive, string tag_agressor ,uint passive_player_server_network_id, uint agressor_server_network_id)
    {
        //tag je za tag colliderja. coll_0 = headshot, coll_1 = body/torso, coll2=arms/legs
        networkObject.SendRpc(RPC_UPDATE_ALL_PLAYER_ID, Receivers.Server);
        if (networkObject.IsServer)
        {
            //-----------------------------------------DAMAGE MODIFIERS----------------------------------------------------
            float current_block_damage_reduction = 1.0f;
            if (tag_passive.Equals("block_player")) current_block_damage_reduction = block_damage_reduction;


            float locational_damage_reduction = torso_damage_multiplier;
            if (tag_passive.Equals("coll_0")) locational_damage_reduction = head_damage_multiplier;
            else if(tag_passive.Equals("coll_2")) locational_damage_reduction = limb_damage_multiplier;

            float all_modifiers = locational_damage_reduction * current_block_damage_reduction;
            //-------------------------------------------------------------------------------------------------------------
            float final_damage_taken = dmg * all_modifiers;
            this.health -= final_damage_taken;
            healthBar.fillAmount = (float)this.health / (float)this.max_health;

            lock (myNetWorker.Players)
            {
                int count = 0;//v koliziji sta udelezena dva igralca, poiskat moramo oba. tukej je lahko problem ce klicemo to metodo pri koliziji z ne-igralcem, za agresorja bo slo vedno cez vse igralce.
                myNetWorker.IteratePlayers((player) =>
                {
                    if (player.NetworkId == passive_player_server_network_id) //passive target
                    {
                        //Debug.Log("Victim found! "+ passive_player_server_network_id);
                        networkObject.SendRpc(player, RPC_SET_HEALTH_PASSIVE_TARGET, this.health);
                        count++;
                    }

                    //agressor za izrisanje damage-a
                    if (player.NetworkId == agressor_server_network_id)
                    {
                        //Debug.Log("Agressor player found! " + agressor_server_network_id);
                        networkObject.SendRpc(player, RPC_RECEIVE_NOTIFICATION_FOR_DAMAGE_DEALT, final_damage_taken, tag_passive);
                        count++;
                    }
                    if (count == 2) return;

                });

            }
        }
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        // TODO:  Your initialization code that relies on network setup for this object goes here
        myNetWorker = GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
        networkObject.SendRpc(RPC_UPDATE_ALL_PLAYER_ID, Receivers.Server);
        if (networkObject.IsOwner)
        {

            healthBar = GameObject.Find("health_fg").GetComponent<Image>();
            transform.Find("canvas_player_overhead").gameObject.SetActive(false);
            FloatingTextController.Initialize();
            reticle_hit_controller.Initialize();
        }

    }


    public void Update()
    {
        if (networkObject == null) return; //tole lahko verjetn vse damo v start
        if (myNetWorker == null)
        {
            if (GameObject.Find("NetworkManager(Clone)") != null)
            {
                myNetWorker = GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
            }
        }

        if (!networkObject.IsServer)
        {
            return;
        }
        
    }



    //--------------------RPC

    public override void updateAllPlayerId(RpcArgs args)//server updejta id-je vseh playerjev. to se zgodi vedno kose sconnecta nov player gor, mogl bi se tud ko se kdo disconnecta ampak ni se to
    {
        if (networkObject.IsServer)
        {
            //Debug.Log("Updating player id's");
            lock (myNetWorker.Players)
            {
                NetworkingPlayer[] players = myNetWorker.Players.ToArray();
                for (int i = 0; i < players.Length; i++)
                {
                    //Debug.Log("id = "+ players[i].NetworkId);
                    networkObject.SendRpc(players[i], RPC_UPDATE_PLAYER_ID, players[i].NetworkId);//networkId je unikaten na serverju, ni pa unikaten za cliente ker vsakmu kaze da je 0. neda se spreminjat tko da sm naredu nov field
                }
            }
        }
    }

    public override void updatePlayerId(RpcArgs args)
    {
        if (!networkObject.IsOwner) { return; }
        this.server_id = args.GetNext<uint>();
        this.player_name.text = this.server_id + "";
        //Debug.Log("changing id to" + this.server_id);
        networkObject.SendRpc(RPC_UPDATE_OTHER_CLIENT_ID, Receivers.Others, this.server_id);
    }

    public override void updateOtherClientId(RpcArgs args)
    {
        this.server_id = args.GetNext<uint>();
        this.player_name.text = this.server_id + "";
        //Debug.Log("changing id to" + this.server_id);
    }

    public override void setHealthPassiveTarget(RpcArgs args)
    {
        if (!networkObject.IsOwner) {
            Debug.Log("server ga retardira");
            return;
        }
       // if (networkObject.IsOwner)
        //{            //if its the owner change the value on other clients
        //Debug.Log("Changing Health from server's RPC");
        this.health = args.GetNext<float>();
        this.healthBar.fillAmount = this.health / (this.max_health);
            networkObject.SendRpc(RPC_SET_HEALTH_ON_OTHERS,Receivers.Others, this.health);
        //}
    }

    public override void setHealthOnOthers(RpcArgs args)
    {
        //Debug.Log("Changing Health from victim's RPC");
        this.health = args.GetNext<float>();
        this.healthBar.fillAmount =this.health / (this.max_health);
    }

    public override void ReceiveNotificationForDamageDealt(RpcArgs args)//tole funkcijo dobi owner agresor objekta in izrise na ekran da je naredu damage, rpc poslje server v metodi take_damage_server_authority
    {

       // if (!networkObject.IsOwner) Debug.Log("I dont know why this prints out. The server is the owner of one of the objects. what the hell?");

        float dmg = args.GetNext<float>();
        string tag = args.GetNext<string>();
       // Debug.Log("I Did Damage! "+tag);
        FloatingTextController.CreateFloatingText(dmg+"", Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane)),tag);
        reticle_hit_controller.CreateReticleHit(tag); //cod2 reticle hit style like
    }




    //treba je nekak nrdit da bo server povedov agresorju da nj izrise damage text. zaenkrat to vseskup server rihta in lokalni player sploh neve ce je naredu damage. to bo mal tezje kot sm mislu. mrde podat argument zravn rpcja za nastavlat dmg al pa mrde nov locen rpc nrdit. hmm
}
