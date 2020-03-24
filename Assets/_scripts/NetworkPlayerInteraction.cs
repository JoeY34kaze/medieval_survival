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
    private bool interacting = false;

    public GameObject canvas;
    private List<GameObject> alerts;
    private double time_pressed_interaction = 0;
    private double time_pressed_alert = 0;
    private DateTime baseDate;
    private Interactable recent_interactable;

    public GameObject[] alert_world_prefab;

    public NetworkPlaceable current_placeable_for_durability_lookup = null;

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

                if (is_holding_a_repair_hammer())
                {//repair hammer -> disable interaction
                    if (hit.collider.gameObject.GetComponent<NetworkPlaceable>() != null && Vector3.Distance(transform.position, hit.collider.transform.position)<NetworkPlaceable.max_distance_for_durability_check)
                        setup_placeable_for_durability_lookup(hit.collider.gameObject.GetComponent<NetworkPlaceable>());
                    else
                        clear_placeable_for_durability_lookup();
                }
                else {//navadna interakcija
                    clear_placeable_for_durability_lookup();

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
                            if (interactable is Interactable_parenting_fix) { interactable = ((Interactable_parenting_fix)(interactable)).parent_interactable; }//tole je zato da se pohendla ce colliderja nimamo na prvem objektu ampak je nizje u hierarhiji. recimo za vrata


                            if ((interactable is Interactable_chest || interactable is ItemPickup || interactable is Interactable_door || interactable is Interactable_trap || interactable is Interactable_crafting_station || interactable is Interactable_guild_flag) && interactable != this.recent_interactable)
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
                                    interactable.Interact();

                                if (interactable is Interactable_Backpack)//pobere backpack
                                                                          //this.menu.show_backpack_interaction_menu(interactable.gameObject);
                                    interactable.GetComponent<NetworkBackpack>().local_player_equip_request();

                                if (interactable is Interactible_ArmorStand)
                                {
                                    ((Interactible_ArmorStand)interactable).local_player_interaction_swap_request(stats.Get_server_id());
                                }

                                if (interactable is Interactable_door)
                                    interactable.localPlayer_interaction_request(0);

                                if (interactable is Interactable_chest)
                                    local_chest_open_request(interactable.gameObject);

                                if (interactable is Interactable_crafting_station)
                                    local_crafting_station_open_inventory_request(interactable.gameObject);

                                if (interactable is Interactable_guild_flag)
                                    if (interactable.GetComponent<NetworkGuildFlag>().is_player_authorized(networkObject.Owner.NetworkId))
                                        local_guild_flag_open_request(interactable.gameObject);
                                    else
                                        local_guild_flag_toggle_authorized_request(interactable.gameObject);

                                if (interactable is interactable_trebuchet) {
                                    local_player_siege_weapon_advance_fire_state_request(interactable.gameObject);
                                }

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
                                        interactable.Interact();//full inventory se mora handlat drugje
                                                                //-------------------------------------------  player ---------------------------------------------------------------
                                    if (interactable is Interactable_player)
                                    {
                                        if (!interactable.transform.root.Equals(transform))//ce nismo raycastal samo nase ( recimo ce smo gledal dol na lastno nogo/roko al pa kej
                                            UILogic.Instance.interactable_radial_menu.show_player_interaction_menu(interactable.gameObject);
                                    }
                                    //-----------------------------------------------------ARMOR STAND-------------------------------
                                    if (interactable is Interactible_ArmorStand)
                                    {
                                        UILogic.Instance.interactable_radial_menu.show_ArmorStand_interaction_menu(interactable.gameObject);
                                    }
                                    if (interactable is Interactable_Backpack)
                                        UILogic.Instance.interactable_radial_menu.show_backpack_interaction_menu(interactable.gameObject);

                                    if (interactable is Interactable_chest)
                                        UILogic.Instance.interactable_radial_menu.show_chest_interaction_menu(interactable.gameObject);

                                    if (interactable is Interactable_trap)
                                        UILogic.Instance.interactable_radial_menu.show_trap_interaction_menu(interactable.gameObject);

                                    if (interactable is Interactable_crafting_station)
                                        UILogic.Instance.interactable_radial_menu.show_craftingStation_menu(interactable.gameObject);

                                    if (interactable is Interactable_guild_flag)
                                        UILogic.Instance.interactable_radial_menu.show_flag_menu(interactable.gameObject, interactable.GetComponent<NetworkGuildFlag>().is_player_authorized(networkObject.Owner.NetworkId));

                                    if (interactable is interactable_trebuchet) {
                                        UILogic.Instance.interactable_radial_menu.show_trebuchet_menu(interactable.gameObject, interactable.GetComponent<NetworkPlaceable>().is_player_owner(networkObject.MyPlayerId));
                                    }
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
                        UILogic.Instance.interactable_radial_menu.show_alert_menu(hit.point);
                    }
                    //----------------- konc alertov

                }
            }
            else
            {
                // Debug.Log("Looking at not interactable " + hit.collider.name + " with distance of " + hit.distance);
            }
        }
        if (Input.GetButtonUp("Interact") || Input.GetButtonUp("Alert"))
        {
            UILogic.Instance.hide_radial_menu();
        }
    }



    private void clear_placeable_for_durability_lookup()
    {
        if (this.current_placeable_for_durability_lookup != null)
        {
            this.current_placeable_for_durability_lookup = null;
            UILogic.Instance.clear_placeable_durability_lookup();
        }
    }

    private void setup_placeable_for_durability_lookup(NetworkPlaceable p)
    {
        if (this.current_placeable_for_durability_lookup == p || p==null) {
                return;
        }

        this.current_placeable_for_durability_lookup = p;
        UILogic.Instance.setup_placeable_durability_lookup(p);
        p.local_player_predmet_update_request();
    }

    private bool is_holding_a_repair_hammer()
    {
        return GetComponentInChildren(typeof(repair_hammer_collider_handler),true).gameObject.activeInHierarchy;
    }


    private void local_guild_flag_toggle_authorized_request(GameObject flag)
    {
        flag.GetComponent<NetworkGuildFlag>().local_flag_toggle_authorized_request();
    }

    private void local_guild_flag_open_request(GameObject flag)
    {
        flag.GetComponent<NetworkContainer>().local_open_container_request();
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

    internal void local_crafting_station_open_inventory_request(GameObject target )
    {
        //poslat mormo rpc na target
        target.GetComponent<Interactable_crafting_station>().local_inventory_request();
    }

    internal void local_crafting_station_pickup_request(GameObject target) {
        target.GetComponent<Interactable_crafting_station>().local_pickup_request();
    }

    internal void local_crafting_station_toggle_request(GameObject target) {
        target.GetComponent<Interactable_crafting_station>().local_toggle_request();
    }

    //        --------------------------------------------------------- -----------------Player interactions--------------------
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

    internal void local_chest_pickup_request(GameObject target)
    {
        Debug.Log("request to pickup chest");
        target.GetComponent<NetworkContainer>().local_container_pickup_request();
    }

    internal void local_chest_open_request(GameObject target)
    {
        Debug.Log("request to open chest");
        target.GetComponent<NetworkContainer>().local_open_container_request();
    }

    internal void local_trap_reload_request(GameObject target)
    {
        Debug.Log("request to reload trap");
        target.GetComponent<Interactable_trap>().local_trap_reload_request();
    }

    internal void local_trap_pickup_request(GameObject target)
    {
        Debug.Log("request to pickup trap");
        target.GetComponent<Interactable_trap>().local_trap_pickup_request();
    }

    internal void local_flag_open_request(GameObject target)
    {
        target.GetComponent<NetworkContainer>().local_open_container_request();
    }

    internal void local_flag_clear_all_request(GameObject target)
    {
        target.GetComponent<NetworkGuildFlag>().local_clear_all_request();
    }

    internal void local_flag_upload_image_request(GameObject target)
    {
        target.GetComponent<NetworkGuildFlag>().local_flag_upload_image_request();
    }

    internal void local_flag_pickup_request(GameObject target)
    {
        target.GetComponent<NetworkContainer>().local_container_pickup_request();
    }

    internal void local_flag_toggle_authorized_request(GameObject target)
    {
        target.GetComponent<NetworkGuildFlag>().local_flag_toggle_authorized_request();
    }

    internal void local_player_siege_weapon_rotate_request(GameObject t)
    {
        UILogic.Instance.show_trebuchet_rotation_panel(t.GetComponent<NetworkSiegeTrebuchet>());
    }

    internal void local_player_siege_weapon_trajectory_change_request(GameObject t)
    {
        t.GetComponent<NetworkSiegeTrebuchet>().local_player_siege_weapon_change_trajectory_request();
    }

    internal void local_player_siege_weapon_open_container_request(GameObject t)
    {
        t.GetComponent<NetworkSiegeTrebuchet>().local_player_siege_weapon_open_container_request();
    }

    internal void local_player_siege_weapon_pickup_request(GameObject t)
    {
        throw new NotImplementedException();
    }

    internal void local_player_siege_weapon_advance_fire_state_request(GameObject t)
    {
        t.GetComponent<NetworkSiegeTrebuchet>().local_player_siege_weapon_advance_fire_state_request();
    }
}
