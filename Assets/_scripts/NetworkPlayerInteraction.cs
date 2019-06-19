using System;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

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
    private void Start()
    {
        stats = GetComponent<NetworkPlayerStats>();
        networkPlayerInventory = GetComponent<NetworkPlayerInventory>();

        
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        // TODO:  Your initialization code that relies on network setup for this object goes here
        myNetWorker = GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
        if(!networkObject.IsOwner)
            Destroy(canvas);
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
        else{
            
            //check what we are looking at with camera.
            Ray ray = new Ray(player_cam.position, player_cam.forward);


            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50)) {

                Debug.DrawRay(player_cam.position, player_cam.forward*10,Color.blue);

                //Debug.Log("raycast : "+hit.collider.name);

                Interactable interactable = hit.collider.GetComponent<Interactable>();
                if(interactable==null) interactable = hit.collider.GetComponentInParent<Interactable>();//je collider popravlen zarad neujemanja pivota ker je blender ziva nocna mora


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
                        if (Input.GetButtonDown("Interact"))
                        {
                            Debug.Log("Interacting with " + hit.collider.name + " with distance of " + hit.distance);
                            if (!stats.downed && !stats.dead)//ce je prayer ziv
                            {
                                // -----------------------------------------    Inventory item / weapon /gear ---------------------------------------------------
                                if (interactable is ItemPickup)
                                    interactable.interact(stats.server_id);//full inventory se mora handlat drugje


                                //-------------------------------------------  player ---------------------------------------------------------------
                                if (interactable is Interactable_player)
                                {
                                    this.menu.show_player_interaction_menu(interactable.gameObject);
                                }
                                //-----------------------------------------------------ARMOR STAND-------------------------------
                                if (interactable is Interactible_ArmorStand)
                                {
                                    this.menu.show_ArmorStand_interaction_menu(interactable.gameObject);
                                }

                                if(interactable is Interactable_Backpack)
                                    this.menu.show_backpack_interaction_menu(interactable.gameObject);

                            }
                        }
                    }
                }
                else {
                   // Debug.Log("Looking at not interactable " + hit.collider.name + " with distance of " + hit.distance);
                }
            }
        }
        if (Input.GetButtonUp("Interact"))
        {
            this.menu.hide_radial_menu();
        }
    }


    private void setup_player_cam()
    {
        this.player_cam = GetComponent<player_camera_handler>().player_cam.transform;
    }
    //                                                                                       -----------------Player interactions--------------------
    internal void local_player_interaction_execution_request(GameObject target)
    {
        target.GetComponent<Interactable_player>().local_player_execution_request(stats.server_id);
    }

    internal void local_player_interaction_tieup_request(GameObject target)
    {
        target.GetComponent<Interactable_player>().local_player_tieup_request(stats.server_id);
    }

    internal void local_player_interaction_steal_request(GameObject target)
    {
        target.GetComponent<Interactable_player>().local_player_steal_request(stats.server_id);
    }

    internal void local_player_interaction_pickup_request(GameObject target)
    {
        //klice downan player, poda id of playerja kter ga pobira
        target.GetComponent<Interactable_player>().local_player_pickup_request(stats.server_id);
    }

    //------------HEALTHY PLAYER-------------

    internal void local_player_interaction_guild_invite_request(GameObject target)
    { 
        target.GetComponent<Interactable_player>().local_player_guild_invite_request(stats.server_id);
    }

    internal void local_player_interaction_team_invite_request(GameObject target)
    {
       // target.GetComponent<Interactable_player>().local_player_team_invite_request(stats.server_id);tole zbrisat ker ta shit negre tukej
        stats.localTeamInviteRequest(target.GetComponent<NetworkPlayerStats>().server_id);

    }

    //                                                                                       -----------------ARMOR STAND---------------------

    internal void local_armor_stand_interaction_ranged_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_ranged_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_shield_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_shield_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_weapon1_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_weapon1_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_weapon0_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_weapon0_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_feet_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_feet_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_legs_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_legs_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_hands_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_hands_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_chest_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_chest_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_helmet_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_helmet_request(stats.server_id);
    }

    internal void local_armor_stand_interaction_give_all_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_give_all_request(stats.server_id, networkPlayerInventory);
    }

    internal void local_armor_stand_interaction_take_all_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_get_all_request(stats.server_id, networkPlayerInventory);
    }

    internal void local_armor_stand_interaction_swap_request(GameObject target)
    {
        target.GetComponent<Interactible_ArmorStand>().local_player_interaction_swap_request(stats.server_id);
    }

    internal void local_backpack_interaction_equip_request(GameObject target)
    {
        target.GetComponent<NetworkBackpack>().local_player_equip_request(stats.server_id);
    }

    internal void local_backpack_interaction_look_request(GameObject target)
    {
        target.GetComponent<NetworkBackpack>().local_player_look_request(stats.server_id);
    }


    public override void ItemPickupRequest(RpcArgs args)//ne nrdi nc
    {
        throw new NotImplementedException();
    }
}
