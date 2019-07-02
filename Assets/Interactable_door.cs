using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;

public class Interactable_door : Interactable
{
    public bool closed = true;
    public bool autoClose = true;

    public bool public_door = true;
    //public uint owner_guild_id = 0;//0 pomen da so od serverja in loh vsak odpre. mesta pa tko

    private Animator anim;
    private void Start()
    {
        this.anim = GetComponent<Animator>();
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer)
            networkObject.TakeOwnership();
    }
    /*
    private void Update()
    {
        if (this.closed && closed != prev_closed)
            CloseDoor();
        else if (!this.closed && closed != prev_closed)
            OpenDoor();

        prev_closed = closed;
    }*/


    private void OpenDoor() {
        //odpri vrata
        anim.SetBool("Closed", false);
        //
        if (networkObject.IsServer)
        {
            if(autoClose)
                StartCoroutine(CloseDoorAfterTime(10f));
            this.closed = false;
        }
    }

    IEnumerator CloseDoorAfterTime(float time)
    {

        yield return new WaitForSecondsRealtime(time);

        if (networkObject.IsServer)
            networkObject.SendRpc(RPC_DOOR_STATE_UPDATE, Receivers.All, true);
    }

    private void CloseDoor() {
        
        anim.SetBool("Closed", true);

        if (networkObject.IsServer) {
            this.closed = true;
        }
    }

    internal override void localPlayer_interaction_request(int v)
    {
        networkObject.SendRpc(RPC_DOOR_INTERACTION_REQUEST, Receivers.Server, v);
    }

    public override void DoorInteractionRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            int interaction_type = args.GetNext<int>();

            if (interaction_type == 0) {//toggle doors
                if(Open_close_door_allowed(args.Info.SendingPlayer.NetworkId))
                    networkObject.SendRpc(RPC_DOOR_STATE_UPDATE, Receivers.All, !this.closed);
            }
        }
    }

    private bool Open_close_door_allowed(uint networkId)
    {
        if (this.public_door) return true;
        //else if player and door in same guild
        return false;
    }

    public override void DoorStateUpdate(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            bool _closed = args.GetNext<bool>();

            if (_closed)
            {
                CloseDoor();
            }
            else {
                OpenDoor();
            }
        }
    }
}
