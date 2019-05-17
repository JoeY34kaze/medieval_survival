﻿using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using System.Collections.Generic;
using System.Collections;

public class ItemPickup : Interactable {
    public int item_id; //ujemat se mora z id-jem itema na playerju ce je na playerju al pa nevem
    public int quantity = 1;
    public bool stackable = false;
    //zaenkrat smao pove da je item k se ga lahko pobere


    IEnumerator changeOwner()//hacky, but we save 1 rpc call because of it
    {
        yield return new WaitForSeconds(1);
        if (networkObject == null) { Debug.LogError("networkObject is null."); }
        if (networkObject.IsServer) networkObject.TakeOwnership();
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        // TODO:  Your initialization code that relies on network setup for this object goes here
        StartCoroutine(changeOwner());

    }

    internal override void interact(uint server_id)//sprozi na playerju
    {
        if (networkObject == null) { Debug.LogError("networkObject is null."); }

        if (local_lock.item_allows_interaction)
        {
            Debug.Log("Sending from local object to server for aprooval");
            networkObject.SendRpc(RPC_HANDLE_ITEM_PICKUP_SERVER_SIDE, Receivers.Server, this.item_id, this.quantity, server_id);
            local_lock.setupInteractionLocalLock();
        }
        else {
            Debug.Log("Cannot interact with the object because you interacted recently or server disallowed it.");
        }
    }

    public override void DestroyWrapper(RpcArgs args)
    {
        Debug.Log("Item is scheduled for destruction. disallowing interaction and adding effects signaling that it not interactable any longer.");
        local_lock.item_allows_interaction = false;
        local_lock.item_waiting_for_destruction = true;
       //handle_destroy_animation();
    }

    private void handle_destroy_animation()//lokalno se zacne predvajat animacija ki pove da se z objektom neda vec interactat in se ga unicuje. fadeout magar al pa nekej
    {
        throw new NotImplementedException();
    }

    public override void HandleItemPickupServerSide(RpcArgs args)
    {
        if (networkObject == null) { Debug.LogError("networkObject is null."); }
        if (!networkObject.IsServer) return;
        Debug.Log("Server received item pickup request");
        if (!local_lock.item_allows_interaction) {
            Debug.Log("item does not allow interaction at this time.");
        }

        int item_id = args.GetNext<int>();
        int quantity = args.GetNext<int>();
        uint player_id = args.GetNext<uint>();




        //check players inventory and other shit if he can pick the item up.
        //destroy item if player can carry all or split it if player cant carry all

        handle_response_from_server(item_id,quantity,args.Info.SendingPlayer);//args.Info is a godsend
        return;

        
    }

    private void handle_response_from_server(int item_id, int quantity, NetworkingPlayer player)
    {
        
        //send response to NetworkPlayerInteraction to handle getting it into inventory
        GameObject player_obj = FindByid(player.NetworkId);
        Debug.Log("sending pickup response signal to player");
        player_obj.GetComponent<NetworkPlayerInteraction>().call_owner_rpc_item_pickup_response(item_id,quantity);
        //send response to yourself to kill itself
        Debug.Log("sending kill signal the object");
        

        handle_network_destruction_server();
    }

    private void handle_network_destruction_server()
    {
        local_lock.item_allows_interaction = false;
        local_lock.item_waiting_for_destruction = true;
        StartCoroutine(DestroyDelayed());

    }

    IEnumerator DestroyDelayed()//sends vertical and horizontal speed to network
    {

        networkObject.SendRpc(RPC_DESTROY_WRAPPER, Receivers.AllProximity);//tole je da disabla interakcijo drugje pa da zacne fadeout al pa nekej. uglavnem nekej casa mora pretect predn se ga unici
        yield return new WaitForSeconds(0.5f);
        if (!networkObject.IsOwner)
            Debug.Log("NOT OWNER!!");
        networkObject.Destroy();

    }
}
