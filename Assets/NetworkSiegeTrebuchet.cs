using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSiegeTrebuchet : NetworkedSiegeWeaponBehavior
{
    private int state = 0;
    private Animator anim;
    private float interactable_distance=15f;
    private bool in_animation = false;

    private void Start()
    {
        this.anim = GetComponent<Animator>();
    }

    public override void advance_state_request(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            if (is_player_allowed_to_advance_state(args.Info.SendingPlayer.NetworkId)) {
                try_to_advance_state();
            }
        }
        else return;
    }

    private void try_to_advance_state()
    {
        if (!in_animation) {
            if(this.state==0 && load_next_shot() || ! (this.state==0))
                send_state_update((this.state + 1) % 3);

            //0 - base, ce poberemo vn ko je ze nalovdan gre nazaj tud
            //1 - reloading
            //2 - firing
        }
    }

    private bool load_next_shot()
    {
        //sprozi se samo ko se lovda shot v state 0
        Debug.LogWarning("not loading anythign yet! change this");
        return true;
    }

    private bool is_player_allowed_to_advance_state(uint networkId)
    {
        return Vector3.Distance(transform.position, FindByid(networkId).transform.position) < this.interactable_distance;
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana povsod
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        return null;
    }

    /// <summary>
    /// updates the atributes specific to siege engine. placeable data is handled there
    /// </summary>
    /// <param name="args"></param>
    public override void atribute_update(RpcArgs args)
    {
       
        if (args.Info.SendingPlayer.IsHost) {

            this.state = args.GetNext<int>();
            this.anim.SetInteger("state", this.state);
            this.in_animation = true;
        }
    }

    /// <summary>
    /// sprozi se samo z animatorja po reloadu in po streljanju
    /// </summary>
    public void reset_animation_bool() {
        this.in_animation = false;
    }

    private void send_state_update(int new_state) {
        if (networkObject.IsServer) {
            networkObject.SendRpc(RPC_ATRIBUTE_UPDATE, Receivers.All, new_state);
        }
    }
}
