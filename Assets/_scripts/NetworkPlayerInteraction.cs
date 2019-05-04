﻿using System;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

public class NetworkPlayerInteraction : NetworkPlayerInteractionBehavior
{
    private Transform player_cam;
    private NetworkPlayerStats stats;
    public float radius = 4f; // ce je distance od camere manjsi kot to lahko interactamo
    private NetworkPlayerInventory player_inventory;
    //private Mapper mapper; sm dau u instance oziroma singleton i think

    private NetWorker myNetWorker;
    protected override void NetworkStart()
    {
        base.NetworkStart();
        // TODO:  Your initialization code that relies on network setup for this object goes here
        myNetWorker = GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
    }

    // Update is called once per frame
    void Update()
    {
        if (!networkObject.IsOwner) return;
        if (stats == null) stats = GetComponent<NetworkPlayerStats>();
        if (player_inventory == null) player_inventory = GetComponent<NetworkPlayerInventory>();
        //if(mapper==null)mapper = GameObject.Find("Mapper").GetComponent<Mapper>();
        if (player_cam == null)
        {
            setup_player_cam();
        }
        else {
            //check what we are looking at with camera.
            Ray ray = new Ray(player_cam.position, player_cam.forward);


            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50)) {

                Debug.DrawRay(player_cam.position, player_cam.forward,Color.blue);

                Interactable interactable = hit.collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    //izriši eno obrobo al pa nekej samo tolk da player vidi je stvari lezijo na tleh
                    /*
                     
                 
                 
                 
                 
                 
                 
                 */

                    if (hit.distance <= radius) { 
                        /*
                         Izsis se kaj dodatnega da bo vedu da lohko direkt pobere



                         */
                        

                        if (Input.GetButtonDown("Interact"))
                        {
                            Debug.Log("Interacting with " + hit.collider.name + " with distance of " + hit.distance);
                            interactable.interact(stats.server_id);
                        }
                    }
                }
                else {
                   // Debug.Log("Looking at not interactable " + hit.collider.name + " with distance of " + hit.distance);
                }
            }
        }
    }

    private void setup_player_cam()
    {
        this.player_cam = GetComponent<player_camera_handler>().player_cam.transform;
    }

         
    public override void ItemPickupResponse(RpcArgs args)
    {
        if (!networkObject.IsOwner) return;
        int item_id = args.GetNext<int>();
        int quantity = args.GetNext<int>();

        //add into inventory since all was aprooved
        //za nahrbtnike bi mrde pustu ks u inventory skripti da se ukvarja z tem najbrz
        Debug.Log("adding into npInventory");
        player_inventory.Add(Mapper.instance.getItemById(item_id), quantity);
        Debug.Log("Inventory aprooval received on client.");
    }
    

    public void call_owner_rpc_item_pickup_response(int item_id, int quantity) {
        Debug.Log("sending response to owner of player");

        networkObject.SendRpc(RPC_ITEM_PICKUP_RESPONSE, Receivers.Owner, item_id, quantity);
    }

    public override void ItemPickupRequest(RpcArgs args)
    {
        throw new NotImplementedException();
    }
}