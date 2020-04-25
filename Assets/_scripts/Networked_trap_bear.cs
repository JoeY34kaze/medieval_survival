using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
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
    protected InteractableLocalLock local_lock;


    [SerializeField]
    public bool Armed;

    private Animator anim;

    #endregion


    #region Collision Detection


    void OnTriggerEnter(Collider other)
    {
        if (this.Armed) {//if trap is ready
            if (networkObject.IsServer) {
                Debug.Log("Server-side collision detected with trigger object " + other.name);
                if (other.transform.root.name.Equals("NetworkPlayer(Clone)") && !other.transform.name.Equals("NetworkPlayer(Clone)")) {//ce je contact z playerjem in ne z njegovim movement colliderjem. what about animals??
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
        this.local_lock = GetComponent<InteractableLocalLock>();
    }



    public override void setAnimationState(RpcArgs args)
    {
        if (!args.Info.SendingPlayer.IsHost) return;

        int new_state = args.GetNext<int>();
        if (new_state == 0)
        { //snap
                Armed = false;       
        }
        else if (new_state == 1) {
            //arm
            Armed = true;
        }
        anim.SetBool("triggered", !Armed);//je lih obratno
    }

    #region Startup

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (!networkObject.IsServer) {
            networkObject.SendRpc(RPC_NETWORK_REFRESH_REQUEST, Receivers.Server);
        }
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
        if (!args.Info.SendingPlayer.IsHost) return; //ni poslov player ampak nas edn hacka
        this.Armed = args.GetNext<int>()==1;
        anim.SetBool("triggered", !Armed);//je lih obratno
    }

    #endregion



    internal void local_trap_pickup_request()
    {
        if (local_lock.item_allows_interaction)
        {
            Debug.Log("Sending from local object to server for aprooval");
            networkObject.SendRpc(RPC_PICKUP_REQUEST, Receivers.Server);
            local_lock.setupInteractionLocalLock();
        }
        else
        {
            Debug.Log("Cannot interact with the object because you interacted recently or server disallowed it.");
        }

    }

    internal void local_trap_reload_request()
    {
        networkObject.SendRpc(RPC_RELOAD, Receivers.Server);
    }

    public override void Reload(RpcArgs args)
    {
        if (networkObject.IsServer) {
            networkObject.SendRpc(RPC_SET_ANIMATION_STATE, Receivers.All,1);
        }
    }

    public override void PickupRequest(RpcArgs args)
    {

        if (networkObject == null) { Debug.LogError("networkObject is null."); }
        if (!networkObject.IsServer) return;
        Debug.Log("Server received item pickup request");
        if (this.local_lock == null) this.local_lock = GetComponent<InteractableLocalLock>();
        if (!local_lock.item_allows_interaction)
        {
            Debug.Log("item does not allow interaction at this time.");
        }

        uint player_id = args.Info.SendingPlayer.NetworkId;
        //check players inventory and other shit if he can pick the item up.
        //destroy item if player can carry all or split it if player cant carry all

        //handle_response_from_server(item_id,quantity,args.Info.SendingPlayer);//args.Info is a godsend

        if (FindByid(player_id).GetComponent<NetworkPlayerInventory>().handleItemPickup(new Predmet(this.item)))//ce mu uspe pobrat -> unic item
            handle_network_destruction_server();
        return;


    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs
    {
        Debug.Log("interactable.findplayerById");
        Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }


    private void handle_network_destruction_server()
    {
        if (this.local_lock == null) this.local_lock = GetComponent<InteractableLocalLock>();
        local_lock.item_allows_interaction = false;
        local_lock.item_waiting_for_destruction = true;
        //StartCoroutine(DestroyDelayed());

        networkObject.SendRpc(RPC_DESTROY__WRAPPER, Receivers.All);//tole je da disabla interakcijo drugje pa da zacne fadeout al pa nekej. uglavnem nekej casa mora pretect predn se ga unici
        //yield return new WaitForSeconds(0.1f);
        if (!networkObject.IsOwner)
            Debug.Log("NOT OWNER!!");
        networkObject.Destroy();

    }

    IEnumerator DestroyDelayed()//sends vertical and horizontal speed to network
    {

        networkObject.SendRpc(RPC_DESTROY__WRAPPER, Receivers.All);//tole je da disabla interakcijo drugje pa da zacne fadeout al pa nekej. uglavnem nekej casa mora pretect predn se ga unici
        yield return new WaitForSeconds(0.1f);
        if (!networkObject.IsOwner)
            Debug.Log("NOT OWNER!!");
        networkObject.Destroy();

    }



    private void handle_destroy_animation()//lokalno se zacne predvajat animacija ki pove da se z objektom neda vec interactat in se ga unicuje. fadeout magar al pa nekej
    {
        throw new NotImplementedException();
    }

    public override void Destroy_Wrapper(RpcArgs args)
    {
        //Debug.Log("Item is scheduled for destruction. disallowing interaction and adding effects signaling that it not interactable any longer.");
        local_lock.item_allows_interaction = false;
        local_lock.item_waiting_for_destruction = true;
        //handle_destroy_animation();
    }
}
