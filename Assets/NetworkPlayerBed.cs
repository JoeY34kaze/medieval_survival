using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerBed : NetworkPlayerBedBehavior
{
    private uint owner_network_id;
    public int seconds=0;
    public string name = "unnamed bed";

    protected override void NetworkStart()
    {
        base.NetworkStart();

    }

    internal bool is_owner(NetworkingPlayer p)
    {
        return this.is_owner(p.NetworkId);
    }

    internal bool is_owner(uint netid) {
        return netid == this.getOwnerOfBed();
    }

    internal uint getOwnerOfBed()//ZAENKRAT!!!! TO SE MORA SPREMENIT DA LAHKO GIFTA FOLK DRUGIM
    {
        return transform.GetComponent<NetworkPlaceable>().get_player_who_placed_this();
    }

    public bool is_valid_timer() {
        return this.seconds <= 0;
    }
    /// <summary>
    /// klice se z bed_btn_handler.OnClick, ko uporabnik klikne na gumb na death screenu, da izbere posteljo kjer se hoce respawnat.
    /// </summary>
    public void localRespawnRequest() {
        if (UILogic.localPlayerGameObject.GetComponent<NetworkPlayerStats>().dead && this.is_valid_timer())
            networkObject.SendRpc(RPC_PLAYER_RESPAWN_REQUEST, Receivers.Server);
    }

    public override void PlayerRespawnRequest(RpcArgs args)
    {
        if (networkObject.Owner.NetworkId == getOwnerOfBed())
        {
            if (FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerStats>().server_side_respawn_request(args, transform.position))
                networkObject.SendRpc(RPC_SET_TIMER, Receivers.All, 0);
            else
                Debug.LogError("Respawn failed!");
        }
    }

    public override void SetTimer(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.seconds = transform.GetComponent<NetworkPlaceable>().p.item.respawn_time;//TOLE TREBA NEKAK PAMETNO NRDIT DA SE ODSTEVA. MOGOCE DA PRIMERJA TIMESTAMPE AL PA DA SE FURA KORUTINA KO JE CD AKTIVEN. ( NE NA VSEH POSTELJAH! ) NAJBRZ TIMESTAMP
        }
    }
    public override void nameRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            string s = args.GetNext<string>();
            if (isLegitName(s))
                networkObject.SendRpc(RPC_NAME_UPDATE, Receivers.All, s);
        }
    }

    /// <summary>
    /// to je zato da preveri al je string neko besedilo al nam koče podret bazo or some shit. zaenkrat ni nč treba primerjat regex z necim pac. mogoce brezveze ker forge pohendla, sam nism zihr
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private bool isLegitName(string s)
    {
        return true;
    }

    public override void nameUpdate(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            string s = args.GetNext<string>();
            this.name = s;
        }
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

    internal void localPlayer_pickup_request()
    {
        if (getOwnerOfBed() == networkObject.MyPlayerId)
        {
           networkObject.SendRpc(RPC_PICKUP_REQUEST, Receivers.Server);
        }
    }

    internal void local_player_playerBed_rename_request(string new_name)
    {
        if(isLegitName(new_name))
            networkObject.SendRpc(RPC_NAME_REQUEST, Receivers.Server, new_name);
        
    }

    public override void PickupRequest(RpcArgs args)
    {
        
        if (getOwnerOfBed() == args.Info.SendingPlayer.NetworkId)
        {

            Debug.Log("Picking up player bed..");
            FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerInventory>().handleItemPickup(GetComponent<NetworkPlaceable>().p);
            networkObject.Destroy();
            
        }
    }
}
