using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UMA.CharacterSystem;

public class NetworkArmorStand : NetworkArmorStandBehavior
{


    private DynamicCharacterAvatar avatar;
    /*
    public Collider collider_head;
    public Collider collider_chest;
    public Collider collider_hands;
    public Collider collider_legs;
    public Collider collider_feet;

    public Collider collider_weapon_0;
    public Collider collider_weapon_1;
    public Collider collider_shield;
    public Collider collider_ranged; //empty ker nimamo ranged weaponov
    */


    public int head=-1;
    public int chest = -1;
    public int hands = -1;
    public int legs = -1;
    public int feet = -1;

    public int weapon_0 = -1;
    public int weapon_1 = -1;
    public int shield = -1;
    public int ranged = -1; //empty ker nimamo ranged weaponov

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        this.avatar = GetComponent<DynamicCharacterAvatar>();
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        networkObject.SendRpc(RPC_NETWORK_REFRESH_REQUEST, Receivers.Server);

    }

    internal void local_interaction_request(int collider_index, uint server_id) {
        //check if both equipped item and armor stand collider items are empty, else send request
        if (Is_sending_request_valid(collider_index, server_id)) {
            networkObject.SendRpc(RPC_ARMOR_STAND_INTERACTION_REQUEST, Receivers.Server, server_id, collider_index);
        }
    }

    private bool Is_sending_request_valid(int collider_index,uint server_id)
    {
        NetworkPlayerInventory npi = FindByid(server_id).GetComponent<NetworkPlayerInventory>();
        switch (collider_index)
        {
            case 0://head
                if (this.head == -1 && npi.getHeadItem() == null) return false;
                break;
            case 1://chest
                if (this.chest == -1 && npi.getChestItem() == null) return false;
                break;
            case 2://hands
                if (this.hands == -1 && npi.getHandsItem() == null) return false;
                break;
            case 3://legs
                if (this.legs == -1 && npi.getLegsItem() == null) return false;
                break;
            case 4://feet
                if (this.feet == -1 && npi.getFeetItem() == null) return false;
                break;
            case 5://wep0
                if (this.weapon_0 == -1 && npi.getWeapon_0Item() == null) return false;
                break;
            case 6://wep1
                if (this.weapon_1 == -1 && npi.getWeapon_1Item() == null) return false;
                break;
            case 7://shield
                if (this.shield == -1 && npi.getShieldItem() == null) return false;
                break;
            case 8://ranged
                if (this.ranged == -1 && npi.getRangedItem() == null) return false;
                break;
            default:
                Debug.LogError("request for armor stand interaction is locally found to be valid");
                return true;
        }
        return true;
    }

    internal Item.Type getItemTypeFromColliderIndex(int i) {
        switch (i)
        {
            case 0:
                return Item.Type.head;
            case 1:
                return Item.Type.chest;
            case 2:
                return Item.Type.hands;
            case 3:
                return Item.Type.legs;
            case 4:
                return Item.Type.feet;
            case 5:
                return Item.Type.weapon;
            case 6:
                return Item.Type.weapon;
            case 7:
                return Item.Type.shield;
            case 8:
                return Item.Type.ranged;
            default:
                Debug.LogError("collider_index doesnt match anything!");
                return Item.Type.resource;
        }
    }


    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log("interactable.findplayerById");
        Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
            if (p.GetComponent<NetworkPlayerStats>().server_id == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }
    //------------------------------------------------------------------------------------------------------RPCS-----------------------------------------------------------------------
    public override void ArmorStandInteractionRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;
        uint server_id = args.GetNext<uint>();
        NetworkPlayerInventory npi = FindByid(server_id).GetComponent<NetworkPlayerInventory>();//bols bi blo zvohat objekt prek args.info.sendingplayer ampak to mi ne ratuje sploh nekej..
        int collider_index = args.GetNext<int>();

        if (!Is_sending_request_valid(collider_index, server_id)) return;
        //tle je najbol pomembna stvar
        switch (collider_index)
        {
            case 0:
                if (this.head != -1)
                {
                    if (npi.getHeadItem() != null)
                    {
                        //perform swap
                        Item loadout_item = npi.PopItemLoadout(Item.Type.head,0);
                        Item onStand = Mapper.instance.getItemById(this.head);
                        this.head = loadout_item.id;
                        npi.SetLoadoutItem(onStand,0);
                    }
                    else {
                        //equip item from stand
                        Item onStand = Mapper.instance.getItemById(this.head);
                        npi.SetLoadoutItem(onStand, 0);
                        this.head = -1;
                    }
                }
                else {
                    if (npi.getHeadItem() != null)
                    {
                        //place equipped item on stand
                        Item loadout_item = npi.PopItemLoadout(Item.Type.head, 0);//pohendla tud removanje itema
                        this.head = loadout_item.id;
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 1:

                if (this.chest != -1)
                {
                    if (npi.getChestItem() != null)
                    {
                        //perform swap
                        Item loadout_item = npi.PopItemLoadout(Item.Type.chest, 0);
                        Item onStand = Mapper.instance.getItemById(this.chest);
                        this.chest = loadout_item.id;
                        npi.SetLoadoutItem(onStand, 0);
                    }
                    else
                    {
                        //equip item from stand
                        Item onStand = Mapper.instance.getItemById(this.chest);
                        npi.SetLoadoutItem(onStand, 0);
                        this.chest = -1;
                    }
                }
                else
                {
                    if (npi.getChestItem() != null)
                    {
                        //place equipped item on stand
                        Item loadout_item = npi.PopItemLoadout(Item.Type.chest, 0);//pohendla tud removanje itema
                        this.chest = loadout_item.id;
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 2:
                if (this.hands != -1)
                {
                    if (npi.getHandsItem() != null)
                    {
                        //perform swap
                        Item loadout_item = npi.PopItemLoadout(Item.Type.hands, 0);
                        Item onStand = Mapper.instance.getItemById(this.hands);
                        this.hands = loadout_item.id;
                        npi.SetLoadoutItem(onStand, 0);
                    }
                    else
                    {
                        //equip item from stand
                        Item onStand = Mapper.instance.getItemById(this.hands);
                        npi.SetLoadoutItem(onStand, 0);
                        this.hands = -1;
                    }
                }
                else
                {
                    if (npi.getHandsItem() != null)
                    {
                        //place equipped item on stand
                        Item loadout_item = npi.PopItemLoadout(Item.Type.hands, 0);//pohendla tud removanje itema
                        this.hands = loadout_item.id;
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 3:
                if (this.legs != -1)
                {
                    if (npi.getLegsItem() != null)
                    {
                        //perform swap
                        Item loadout_item = npi.PopItemLoadout(Item.Type.legs, 0);
                        Item onStand = Mapper.instance.getItemById(this.legs);
                        this.legs = loadout_item.id;
                        npi.SetLoadoutItem(onStand, 0);
                    }
                    else
                    {
                        //equip item from stand
                        Item onStand = Mapper.instance.getItemById(this.legs);
                        npi.SetLoadoutItem(onStand, 0);
                        this.legs = -1;
                    }
                }
                else
                {
                    if (npi.getLegsItem() != null)
                    {
                        //place equipped item on stand
                        Item loadout_item = npi.PopItemLoadout(Item.Type.legs, 0);//pohendla tud removanje itema
                        this.legs = loadout_item.id;
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 4:
                if (this.feet != -1)
                {
                    if (npi.getFeetItem() != null)
                    {
                        //perform swap
                        Item loadout_item = npi.PopItemLoadout(Item.Type.feet, 0);
                        Item onStand = Mapper.instance.getItemById(this.feet);
                        this.feet = loadout_item.id;
                        npi.SetLoadoutItem(onStand, 0);
                    }
                    else
                    {
                        //equip item from stand
                        Item onStand = Mapper.instance.getItemById(this.feet);
                        npi.SetLoadoutItem(onStand, 0);
                        this.feet = -1;
                    }
                }
                else
                {
                    if (npi.getFeetItem() != null)
                    {
                        //place equipped item on stand
                        Item loadout_item = npi.PopItemLoadout(Item.Type.feet, 0);//pohendla tud removanje itema
                        this.feet = loadout_item.id;
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            default:
                Debug.LogError("collider_index doesnt match anything!");
                break;
        }
        npi.sendNetworkUpdate(false, true);
        networkObject.SendRpc(RPC_ARMOR_STAND_REFRESH, Receivers.All, this.head, this.chest, this.hands, this.legs, this.feet, this.weapon_0, this.weapon_1, this.shield, this.ranged);
    }

    public override void ArmorStandRefresh(RpcArgs args)//pri dodajanju dobi tud server
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return; //ni poslov player ampak nas edn hacka

        if (!networkObject.IsServer)
        {
            this.head = args.GetNext<int>();
            this.chest = args.GetNext<int>();
            this.hands = args.GetNext<int>();
            this.legs = args.GetNext<int>();
            this.feet = args.GetNext<int>();
            this.weapon_0 = args.GetNext<int>();
            this.weapon_1 = args.GetNext<int>();
            this.shield = args.GetNext<int>();
            this.ranged = args.GetNext<int>();
        }

        redraw_armor_stand();

    }

    private void redraw_armor_stand()//izrise stvari k so na uma
    {
        Debug.Log("Redrawing");

        //--------------------------------------CLOTHING-----------------------------------------

        avatar.ClearSlots();

        if (this.head != -1)
        {
            avatar.SetSlot("Helmet", Mapper.instance.getItemById(this.head).recipeName);
        }

        if (this.chest != -1)
        {
            avatar.SetSlot("Chest", Mapper.instance.getItemById(this.chest).recipeName);
        }

        if (this.hands != -1)
        {
            avatar.SetSlot("Hands", Mapper.instance.getItemById(this.hands).recipeName);
        }

        if (this.legs != -1)
        {
            avatar.SetSlot("Legs",Mapper.instance.getItemById(this.legs).recipeName);
        }

        if (this.feet != -1)
        {
            avatar.SetSlot("Feet", Mapper.instance.getItemById(this.feet).recipeName);
        }

        avatar.BuildCharacter();

        //--------------------------------------WEAPONS AND SHIELD-----------------------------------------





    }

    /// <summary>
    /// poslje client serverju ko se connecta gor. kot response pricakuje rpc armorStandRefresh
    /// </summary>
    /// <param name="args"></param>
    public override void NetworkRefreshRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;

        networkObject.SendRpc(args.Info.SendingPlayer, RPC_ARMOR_STAND_REFRESH, this.head, this.chest, this.hands, this.legs, this.feet, this.weapon_0, this.weapon_1, this.shield, this.ranged);
    }
}
