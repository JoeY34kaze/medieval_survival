using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;

public class Network_body : Network_bodyBehavior
{
    private string uma_data;
    private uint id_player_to_copy;

    protected override void NetworkStart()
    {
        base.NetworkStart();


        networkObject.SendRpc(RPC_REQUEST_INIT, Receivers.Server);

        //-povej serverju da si postavlen da ti lahko posle rpc
    }


    public override void setupUma(RpcArgs args)//poslje server vsem--- !!!!!!!! UMA DATA nerabmo posilat z rpc ker mora bit ze posinhroniziran prej, sam vzamemo lokalno od objekta. rabmo samo id - to bo ksnej treba pocistit
    {
        string data = args.GetNext<string>();
        uint id = args.GetNext<uint>();
        //Debug.Log("received UMA data : " + data+"za playerja+"+id);
        //------------- setup appearence
        //------------- setup gear
        //------------- player renderer se disabla drugje. v player stats
    }

    private void LateUpdate()
    {
        if (GetComponent<UMA.Dynamics.UMAPhysicsAvatar>() != null) {
            if(!GetComponent<UMA.Dynamics.UMAPhysicsAvatar>().ragdolled)
                GetComponent<UMA.Dynamics.UMAPhysicsAvatar>().ragdolled = true;
        }
    }

    public GameObject FindByid(uint targetNetworkId)//koda kopirana tud v interactable_player i think
    {
        Debug.Log("interactable.findplayerById");
       // Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
            if (p.GetComponent<NetworkPlayerStats>().server_id == targetNetworkId) return p;
        }
       // Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }

    public override void request_init(RpcArgs args)
    {
       if(networkObject.IsServer)
            networkObject.SendRpc(args.Info.SendingPlayer,RPC_SETUP_UMA, this.uma_data, this.id_player_to_copy);
    }

    internal void set_data_for_init(string data, uint player_id)
    {
        this.uma_data = data;
        this.id_player_to_copy = player_id;
    }
}
