using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// vse kar ma veze z chestom in ne pase u superclass placeabla
/// </summary>
/// 

    //---------------------------------------------------------networkObject.owner bi mogu bit server!----------
public class NetworkChest : NetworkChestBehavior
{
    private Predmet p;

    private NetworkContainer_items nci;

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();//server prevzame ownership
            this.nci = GetComponent<NetworkContainer_items>();
            init();
        }
    }


        public void interact() {
        Debug.Log("Interacting. this is it");
    }

    //nekak se mora klicat da se nastimajo parametri ob postavitvi
    public void init()
    {
        if (this.p != null)
            this.nci.init(p.item.backpack_capacity);
        else this.nci.init(30);
    }

    /// <summary>
    /// player (se da nekak zvodat id z networkinga ampak nerabmo. server vid takoj id) proba pobrat ta chest
    /// </summary>
    internal void local_chest_pickup_request()
    {

        networkObject.SendRpc(RPC_PICKUP_REQUEST, Receivers.Server);
    }

    internal void local_chest_open_request()
    {
        throw new NotImplementedException();
    }

    public override void pickupRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            NetworkingPlayer requester_networkPlayer = args.Info.SendingPlayer;
            GameObject requester_gameObject= FindByid(requester_networkPlayer.NetworkId);

            //----------checks for security and valididty
            if (!this.nci.isEmpty()) return;

            //prevert se ce je owner, ali ma privilegij za pobiranje pa take fore odvisn od guilda. sam zaenkrat guildi se nimajo influenca
            //------------------------
            requester_gameObject.GetComponent<NetworkPlayerInventory>().handleItemPickup(p);
            Destroy(this.gameObject);

        }
    }

    public override void openRequest(RpcArgs args)
    {
        throw new NotImplementedException();
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

    public override void openResponse(RpcArgs args)
    {
        throw new NotImplementedException();
    }
}
