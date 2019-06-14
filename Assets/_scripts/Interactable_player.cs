using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;

public class Interactable_player : Interactable
{
    internal override bool isPlayerDowned()
    {
        return GetComponent<NetworkPlayerStats>().downed;
    }

    internal override void local_player_pickup_request(uint healthy_player_server_id) {

        //tle lahko ze mal antihacka nrdimo drgac ceprov je client side ?


        networkObject.SendRpc(RPC_REVIVE_DOWNED_PLAYER_REQUEST, Receivers.Server, healthy_player_server_id);
        
    }

    public override void ReviveDownedPlayerRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;

        uint reviver = args.GetNext<uint>();
        uint downed_server_id = GetComponent<NetworkPlayerStats>().server_id;
        Debug.Log("Server: ReviveDownedPlayerRequest :" + reviver + " -> (" + downed_server_id);

        //-----------------antihack checks  (distance, raycast za fov, take stvari)
        float max_distance = 6f;
        Transform a = FindByid(reviver).transform;
        Transform d = GetComponent<Transform>();
        if (Vector3.Distance(a.position, d.position) > max_distance)
        {
            Debug.LogError("ANTIHACK! : Reviving. Players:" + reviver + " -> (" + downed_server_id + " | " + GetComponent<NetworkPlayerStats>().server_id + ")");
            return;
        }
        //------------------------

        networkObject.SendRpc(RPC_REVIVE_DOWNED_PLAYER_RESPONSE, Receivers.All);
        //set health and other shit
        FindByid(downed_server_id).GetComponent<NetworkPlayerStats>().set_player_health(25,downed_server_id);
    }

    public override void ReviveDownedPlayerResponse(RpcArgs args)
    {
        Debug.Log(" ReviveDownedPlayersponse : (" + args.Info.SendingPlayer.NetworkId + " | " + GetComponent<NetworkPlayerStats>().server_id + ")");
        GetComponent<NetworkPlayerStats>().handle_player_pickup();
    }

    internal void local_player_team_invite_request(uint server_id)
    {
        throw new NotImplementedException();
    }

    internal void local_player_guild_invite_request(uint server_id)
    {
        throw new NotImplementedException();
    }

    internal void local_player_steal_request(uint server_id)
    {
        throw new NotImplementedException();
    }

    internal void local_player_tieup_request(uint server_id)
    {
        throw new NotImplementedException();
    }

    internal void local_player_execution_request(uint server_id)
    {
        throw new NotImplementedException();
    }
}
