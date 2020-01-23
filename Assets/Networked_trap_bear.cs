using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Networked_trap_bear : NetworkTrapBehavior
{
    //damage only and animation. ownership and stuff goes into Interactable_trap_bear.cs

    //damage etwork logic is handled on server locally and only sends update through players rpc.
    //animation is handled through this script and networked here
    public Item item;
    #region Activation


    private bool armed = true;

    [SerializeField]
    public bool Armed
    {
        get
        {
            return armed;
        }
        set
        {
            armed = value;
            OnArmedChanged();
        }
    }

    void OnArmedChanged()
    {
        anim.SetBool("triggered", !armed);
    }

    private Animator anim;

    #endregion


    #region Collision Detection


    void OnTriggerEnter(Collider other)
    {
        if (this.armed) {//if trap is ready
            if (networkObject.IsServer) {
                Debug.Log("Server-side collision detected with trigger object " + other.name);
                if (other.transform.root.name.Equals("NetworkPlayer(Clone)") && !other.transform.name.Equals("NetworkPlayer(Clone)")) {//ce je contact z playerjem in ce ni playerjev movement collider. what about animals??
                                                                                                                                       //handle taking damage on player
                    other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().take_environmental_damage_server_authority(this.item, other.tag);
                    //handle animation here
                    //Debug.LogError("implement animation");
                    networkObject.SendRpc(RPC_SET_ANIMATION_STATE, Receivers.All, 0);
                }
            }
        }
    }

    
    #endregion


    private void Awake()
    {
        anim = GetComponent<Animator>();
    }



    public override void setAnimationState(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;

        int new_state = args.GetNext<int>();
        if (new_state == 0)
        { //snap
                Armed = false;       
        }
        else if (new_state == 1) {
            //arm
            Armed = true;
        }
    }

    #region Startup

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if(!networkObject.IsServer)
            networkObject.SendRpc(RPC_NETWORK_REFRESH_REQUEST, Receivers.Server);

    }

    /// <summary>
    /// poslje client serverju ko se connecta gor. kot response pricakuje rpc armorStandRefresh
    /// </summary>
    /// <param name="args"></param>
    public override void NetworkRefreshRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;
        int r = 0;
        if (this.Armed == true) r = 1;
        networkObject.SendRpc(args.Info.SendingPlayer, RPC_REFRESH, r);
    }

    public override void Refresh(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return; //ni poslov player ampak nas edn hacka
        this.Armed = args.GetNext<int>()==1;
    }
    #endregion
}
