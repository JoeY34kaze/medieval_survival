using System;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections.Generic;

public class NetworkPlayerInteraction : NetworkPlayerInteractionBehavior
{
    private Transform player_cam;
    private NetworkPlayerStats stats;
    public float radius = 4f; // ce je distance od camere manjsi kot to lahko interactamo
    private NetworkPlayerInventory networkPlayerInventory;

    private NetWorker myNetWorker;

    public Interactable_radial_menu menu;
    private bool interacting = false;

    public GameObject canvas;
    private List<GameObject> alerts;
    private double time_pressed_interaction = 0;
    private double time_pressed_alert = 0;
    private DateTime baseDate;
    private Interactable recent_interactable;

    public GameObject[] alert_world_prefab;

    public Greyman.OffScreenIndicator offscreen_indicator;//kupljen asset
    private void Start()
    {
        stats = GetComponent<NetworkPlayerStats>();
        networkPlayerInventory = GetComponent<NetworkPlayerInventory>();
        this.alerts = new List<GameObject>();
        this.baseDate = new DateTime(1970, 1, 1);
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        // TODO:  Your initialization code that relies on network setup for this object goes here
        myNetWorker = GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
        if (!networkObject.IsOwner)
            Destroy(canvas);
    }
    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log("interactable.findplayerById");
        //Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
         //    Debug.Log(p.GetComponent<NetworkPlayerStats>().server_id);
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }
    // Update is called once per frame
    void Update()
    {
        if (networkObject == null) { Debug.LogError("networkObject is null."); return; }
        if (!networkObject.IsOwner) return;
        if (stats.downed || stats.dead) return;
        if (stats == null) stats = GetComponent<NetworkPlayerStats>();
        if (networkPlayerInventory == null) networkPlayerInventory = GetComponent<NetworkPlayerInventory>();
        //if (Input.GetButton("Interact")) return;
        //if(mapper==null)mapper = GameObject.Find("Mapper").GetComponent<Mapper>();

        if (player_cam == null)
        {
            setup_player_cam();
        }
        else
        {

            //check what we are looking at with camera.
            Ray ray = new Ray(player_cam.position, player_cam.forward);


            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50))
            {

                Debug.DrawRay(player_cam.position, player_cam.forward * 10, Color.blue);

                //Debug.Log("raycast : "+hit.collider.name);

                Interactable interactable = hit.collider.GetComponent<Interactable>();
                if (interactable == null) interactable = hit.collider.GetComponentInParent<Interactable>();//je collider popravlen zarad neujemanja pivota ker je blender ziva nocna mora


                if (interactable != null)
                {
                    //izriši eno obrobo al pa nekej samo tolk da player vidi da lahko z stvarjo eventualno interacta?
                    /*
                   
                 
                 */


                    if (hit.distance <= radius)
                    {
                        /*
                         Izsis se kaj dodatnega da bo vedu da lohko direkt pobere - glow?

                         */

                        if ((interactable is ItemPickup || interactable is Interactable_door) && interactable != this.recent_interactable)
                        {
                            interactable.setMaterialGlow();
                            if (this.recent_interactable != null)
                                this.recent_interactable.resetMaterial();
                            this.recent_interactable = interactable;
                        }


                        if (Input.GetButtonDown("Interact") && this.time_pressed_interaction == 0f && !(Input.GetButton("Alert") || this.time_pressed_alert > 0))
                        {

                            TimeSpan diff = DateTime.Now - baseDate;
                            this.time_pressed_interaction = diff.TotalMilliseconds;
                        }
                        else if (Input.GetButtonUp("Interact") && this.time_pressed_interaction > 0 && !(Input.GetButton("Alert") || this.time_pressed_alert > 0))
                        {

                            Debug.Log("quick press");
                            this.time_pressed_interaction = 0;
                            //Debug.Log("quick press" + (time_released - this.time_pressed));
                            if (interactable is ItemPickup)//pobere item
                                interactable.interact(stats.Get_server_id());

                            if (interactable is Interactable_Backpack)//pobere backpack
                                                                      //this.menu.show_backpack_interaction_menu(interactable.gameObject);
                                interactable.GetComponent<NetworkBackpack>().local_player_equip_request();

                            if (interactable is Interactible_ArmorStand)
                            {
                                ((Interactible_ArmorStand)interactable).local_player_interaction_swap_request(stats.Get_server_id());
                            }

                            if (interactable is Interactable_door)
                                interactable.localPlayer_interaction_request(0);

                        }
                        else if (Input.GetButton("Interact") && this.time_pressed_interaction > 0 && time_passed_interaction(150f) && !(Input.GetButton("Alert") || this.time_pressed_alert > 0))
                        {
                            this.time_pressed_interaction = 0;
                            //long hold - odpri radial menu
                            Debug.Log("long hold");
                            Debug.Log("Interacting with " + hit.collider.name + " with distance of " + hit.distance);
                            if (!stats.downed && !stats.dead)//ce je prayer ziv
                            {
                                // -----------------------------------------    Inventory item / weapon /gear ---------------------------------------------------
                                if (interactable is ItemPickup)
                                    interactable.interact(stats.Get_server_id());//full inventory se mora handlat drugje
                                                                           //-------------------------------------------  player ---------------------------------------------------------------
                                if (interactable is Interactable_player)
                                {
                                    if (!interactable.transform.root.Equals(transform))//ce nismo raycastal samo nase ( recimo ce smo gledal dol na lastno nogo/roko al pa kej
                                        this.menu.show_player_interaction_menu(interactable.gameObject);
                                }
                                //-----------------------------------------------------ARMOR STAND-------------------------------
                                if (interactable is Interactible_ArmorStand)
                                {
                                    this.menu.show_ArmorStand_interaction_menu(interactable.gameObject);
                                }
                                if (interactable is Interactable_Backpack)
                                    this.menu.show_backpack_interaction_menu(interactable.gameObject);
                            }
                        }
                    }
                }
                else
                {
                    if (this.recent_interactable != null)
                        this.recent_interactable.resetMaterial();
                    this.recent_interactable = null;
                }

                // ------------------- za alerte 
                if (Input.GetButtonDown("Alert") && this.time_pressed_alert == 0f && !(Input.GetButton("Interact") || this.time_pressed_interaction > 0))
                {
                    TimeSpan diff = DateTime.Now - baseDate;
                    this.time_pressed_alert = diff.TotalMilliseconds;
                    Debug.Log("ALERT start");
                }
                else if (Input.GetButtonUp("Alert") && this.time_pressed_alert > 0 && !(Input.GetButton("Interact") || this.time_pressed_interaction > 0))
                {
                    this.time_pressed_alert = 0;
                    Debug.Log("ALERT quick");
                    local_send_alert(hit.point, 1);
                }
                else if (Input.GetButton("Alert") && this.time_pressed_alert > 0 && time_passed_alert(150f) && !(Input.GetButton("Interact") || this.time_pressed_interaction > 0))
                {
                    this.time_pressed_alert = 0;
                    Debug.Log("ALERT long");
                    this.menu.show_alert_menu(hit.point);
                }
                //----------------- konc alertov
            }
            else
            {
                // Debug.Log("Looking at not interactable " + hit.collider.name + " with distance of " + hit.distance);
            }
        }
        if (Input.GetButtonUp("Interact") || Input.GetButtonUp("Alert"))
        {
            this.menu.hide_radial_menu();
        }
    }





    private bool time_passed_interaction(float limit)
    {
        double time_released = (DateTime.Now - this.baseDate).TotalMilliseconds;

        if (time_released - this.time_pressed_interaction >= limit)
        {
            return true;
        }
        return false;
    }

    private bool time_passed_alert(float limit)
    {
        double time_released = (DateTime.Now - this.baseDate).TotalMilliseconds;

        if (time_released - this.time_pressed_alert >= limit)
        {
            return true;
        }
        return false;
    }

    private void setup_player_cam()
    {
        this.player_cam = Camera.main.transform;
    }
    //                                                                                       -----------------Player interactions--------------------
    internal void local_player_interaction_execution_request(GameObject target)
    {
        target.GetComponent<Interactable_player>().local_player_execution_request(stats.Get_server_id());
        GetComponent<NetworkPlayerAnimationLogic>().BeginExecution();//ubistvu ze zacne animacijo
    }

    internal void local_player_interaction_tieup_request(GameObject target)
    {
        target.GetComponent<Interactable_player>().local_player_tieup_request(stats.Get_server_id());
    }

    internal void local_player_interaction_steal_request(GameObject target)
    {
        target.GetComponent<Interactable_player>().local_player_steal_request(stats.Get_server_id());
    }

    internal void local_player_interaction_pickup_request(GameObject target)
    {
        //klice downan player, poda id of playerja kter ga pobira
        target.GetComponent<Interactable_player>().local_player_pickup_request(stats.Get_server_id());
    }

    //------------HEALTHY PLAYER-------------

    internal void local_player_interaction_guild_invite_request(GameObject target)
    {
        //target.GetComponent<Interactable_player>().local_player_guild_invite_request(stats.Get_server_id());
        GameObject.FindGameObjectWithTag("GuildManager").GetComponent<NetworkGuildManager>().localSendGuildInvite(target.GetComponent<NetworkPlayerStats>().Get_server_id());//z networkManagerja klicemo rpc na server da pohendla pa posle response
    }

    internal void local_player_interaction_team_invite_request(GameObject target)
    {
        // target.GetComponent<Interactable_player>().local_player_team_invite_request(stats.server_id);tole zbrisat ker ta shit negre tukej
        stats.localTeamInviteRequest(target.GetComponent<NetworkPlayerStats>().Get_server_id());

    }

    //                                                                                       -----------------ARMOR STAND---------------------

    internal void local_armor_stand_interaction_ranged_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_ranged_request(stats.Get_server_id());
    }

    internal void local_armor_stand_interaction_shield_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_shield_request(stats.Get_server_id());
    }

    internal void local_armor_stand_interaction_weapon0_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_weapon0_request(stats.Get_server_id());
    }

    internal void local_armor_stand_interaction_feet_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_feet_request(stats.Get_server_id());
    }

    internal void local_armor_stand_interaction_legs_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_legs_request(stats.Get_server_id());
    }

    internal void local_armor_stand_interaction_hands_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_hands_request(stats.Get_server_id());
    }

    internal void local_armor_stand_interaction_chest_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_chest_request(stats.Get_server_id());
    }

    internal void local_armor_stand_interaction_helmet_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_helmet_request(stats.Get_server_id());
    }

    internal void local_armor_stand_interaction_give_all_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_give_all_request(stats.Get_server_id(), networkPlayerInventory);
    }

    internal void local_armor_stand_interaction_take_all_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_get_all_request(stats.Get_server_id(), networkPlayerInventory);
    }

    internal void local_armor_stand_interaction_swap_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_swap_request(stats.Get_server_id());
    }

    internal void local_backpack_interaction_equip_request(GameObject target)
    {
        target.GetComponent<NetworkBackpack>().local_player_equip_request();
    }

    internal void local_backpack_interaction_look_request(GameObject target)
    {
        target.GetComponent<NetworkBackpack>().local_player_look_request();
    }

    internal void local_alert_danger_request(Vector3 point)
    {
        local_send_alert(point, 0);
    }

    internal void local_alert_ground_request(Vector3 point)
    {
        local_send_alert(point, 1);
    }

    public override void AlertRequest(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            //poisc njegov team
            uint[] team = FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerStats>().getTeam();
            if (team == null)
            {

            }
            else
            {
                byte b = args.GetNext<byte>();
                Vector3 p = args.GetNext<Vector3>();
                lock (myNetWorker.Players)
                {

                    myNetWorker.IteratePlayers((player) =>
                    {
                        if (contains(team, player.NetworkId)) //passive target
                        {

                            FindByid(player.NetworkId).GetComponent<NetworkPlayerInteraction>().send_alert_server_side(b, p);
                            Debug.Log("sending alerto from server to " + player.NetworkId);
                        }
                    });

                }
            }
        }
    }

    private void send_alert_server_side(byte b, Vector3 p)
    {
        networkObject.SendRpc(RPC_ALERT, Receivers.Owner, b, p);
    }


    private void local_send_alert(Vector3 point, int v)
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_ALERT_REQUEST, Receivers.Server, (byte)v, point);
    }

    private bool contains(uint[] team, uint networkId)
    {
        foreach (uint i in team)
            if (i == networkId)
                return true;
        return false;
    }
    /// <summary>
    /// kdorkoli lahko poslje. rpc se poslje vsem k so u njegovmu teamu. ce nima teama se ne pšoslje nikamor
    /// </summary>
    /// <param name="args"></param>
    public override void Alert(RpcArgs args)
    {
        Debug.Log("sending player id: " + args.Info.SendingPlayer.NetworkId + " owner: " + networkObject.Owner.NetworkId);
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            foreach (GameObject gi in this.alerts)
            {
                removeIndicator(gi.transform);
                Destroy(gi);
            }

            this.alerts.Clear();

            int tip = (int)args.GetNext<byte>();
            GameObject g = GameObject.Instantiate<GameObject>(this.alert_world_prefab[tip], args.GetNext<Vector3>(), Quaternion.identity);
            alerts.Add(g);
            g.GetComponent<alert_world_object>().linked_player_interaction = this;
            offscreen_indicator.AddIndicator(g.transform, tip);
        }
    }

    public void kill_alert_from_alert(Transform t)
    {
        removeIndicator(t);
        this.alerts.Clear();//ker se je stvar rabila saba ubit moramo pohendlat ta shit ker gani nbena stvar prepisala.
    }

    private void removeIndicator(Transform t)
    {
        offscreen_indicator.RemoveIndicator(t);
    }
}
