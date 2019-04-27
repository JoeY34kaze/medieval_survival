using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using System.Collections.Generic;

public class ItemPickup : Interactable {
    public int item_id; //ujemat se mora z id-jem itema na playerju ce je na playerju al pa nevem
    public int quantity = 1;
    public bool stackable = false;
    //zaenkrat smao pove da je item k se ga lahko pobere



    internal override void interact(uint server_id)
    {

        Debug.Log("Sending from local object to server for aprooval");
        networkObject.SendRpc(RPC_HANDLE_ITEM_PICKUP_SERVER_SIDE, Receivers.Server, this.item_id, this.quantity, server_id);
    }

    public override void DestroyWrapper(RpcArgs args)
    {
        Debug.Log("fucker dead");
        networkObject.Destroy();
    }

    public override void HandleItemPickupServerSide(RpcArgs args)
    {
        int item_id = args.GetNext<int>();
        int quantity = args.GetNext<int>();
        uint player_id = args.GetNext<uint>();

        if (!networkObject.IsServer) return;
        Debug.Log("Server received item pickup request");


        //check players inventory and other shit if he can pick the item up.
        //destroy item if player can carry all or split it if player cant carry all

        NetWorker myNetWorker = GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
        lock (myNetWorker.Players)//send response
        {
            myNetWorker.IteratePlayers((player) =>
            {
                if (player.NetworkId == player_id)
                {
                    Debug.Log("Item pickup aprooved on server! " + player);
                    //List<NetworkObject> nl = myNetWorker.NetworkObjectList;
                    handle_response_from_server(item_id,quantity,player);
                    return;
                }
            });
        }
    }

    private void handle_response_from_server(int item_id, int quantity, NetworkingPlayer player)
    {
        //send response to NetworkPlayerInteraction to handle getting it into inventory
        GameObject player_obj = FindByid(player.NetworkId);
        player_obj.GetComponent<NetworkPlayerInteraction>().call_owner_rpc_item_pickup_response(item_id,quantity);

        //send response to yourself to kill yourself
        Debug.Log("sending kill signal to the fucker");
        networkObject.SendRpc(RPC_DESTROY_WRAPPER, Receivers.Owner);
    }

    public override void ItemPickupResponse(RpcArgs args)
    {
        //empty, NetworkPlayerInteraction script handles this.
    }

}
