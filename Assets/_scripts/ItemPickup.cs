using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using System.Collections.Generic;
using System.Collections;

public class ItemPickup : Interactable {
    public Predmet p; //ujemat se mora z id-jem itema na playerju ce je na playerju al pa nevem

    public bool stackable = false;
    //zaenkrat smao pove da je item k se ga lahko pobere

    private Material glow;
    public Material original_material;
    private MeshRenderer renderer;
    private bool initialized = false;
    private void Start()
    {
        if (this.renderer == null) this.renderer = GetComponent<MeshRenderer>();
        if (this.renderer == null) this.renderer = GetComponentInChildren<MeshRenderer>();

        this.glow = (Material)Resources.Load("Glow_green", typeof(Material));
        this.original_material = this.renderer.material;
        this.local_lock = GetComponent<InteractableLocalLock>();
        this.initialized = true;
    }

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
        if(this.local_lock==null)this.local_lock = GetComponent<InteractableLocalLock>();
        if (local_lock.item_allows_interaction)
        {
            Debug.Log("Sending from local object to server for aprooval");
            networkObject.SendRpc(RPC_HANDLE_ITEM_PICKUP_SERVER_SIDE, Receivers.Server);
            local_lock.setupInteractionLocalLock();
        }
        else {
            Debug.Log("Cannot interact with the object because you interacted recently or server disallowed it.");
        }
    }

    public override void DestroyWrapper(RpcArgs args)
    {
//Debug.Log("Item is scheduled for destruction. disallowing interaction and adding effects signaling that it not interactable any longer.");
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
        if (this.local_lock == null) this.local_lock = GetComponent<InteractableLocalLock>();
        if (!local_lock.item_allows_interaction) {
            Debug.Log("item does not allow interaction at this time.");
        }

        uint player_id = args.Info.SendingPlayer.NetworkId;




        //check players inventory and other shit if he can pick the item up.
        //destroy item if player can carry all or split it if player cant carry all

        //handle_response_from_server(item_id,quantity,args.Info.SendingPlayer);//args.Info is a godsend

        if(FindByid(player_id).GetComponent <NetworkPlayerInventory>().handleItemPickup(this.p))//ce mu uspe pobrat -> unic item
            handle_network_destruction_server();
        return;

        
    }

   

    private void handle_network_destruction_server()
    {
        if (this.local_lock == null) this.local_lock = GetComponent<InteractableLocalLock>();
        local_lock.item_allows_interaction = false;
        local_lock.item_waiting_for_destruction = true;
        //StartCoroutine(DestroyDelayed());

        networkObject.SendRpc(RPC_DESTROY_WRAPPER, Receivers.All);//tole je da disabla interakcijo drugje pa da zacne fadeout al pa nekej. uglavnem nekej casa mora pretect predn se ga unici
        //yield return new WaitForSeconds(0.1f);
        if (!networkObject.IsOwner)
            Debug.Log("NOT OWNER!!");
        networkObject.Destroy();

    }

    IEnumerator DestroyDelayed()//sends vertical and horizontal speed to network
    {

        networkObject.SendRpc(RPC_DESTROY_WRAPPER, Receivers.All);//tole je da disabla interakcijo drugje pa da zacne fadeout al pa nekej. uglavnem nekej casa mora pretect predn se ga unici
        yield return new WaitForSeconds(0.1f);
        if (!networkObject.IsOwner)
            Debug.Log("NOT OWNER!!");
        networkObject.Destroy();

    }

    public override void setForce(Vector3 pos, Vector3 dir) {
        if (!networkObject.IsServer) return;
        networkObject.SendRpc(RPC_APPLY_FORCE_ON_INSTANTIATION, Receivers.Others, pos, dir); //Receivers.All loh rata overkill z stevilom igralcev. optimizacija kasnej

        //za server posebej ker iz meni neznanga razloga ne dela ce dam receivers.all itak na serverju ni vazno v koncni fazi tolk. mogoce za anticheat al pa kej..
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero; //ce ma ksno gravitacijo
            rb.angularVelocity = Vector3.zero; //?

            transform.position = pos;//da se zacne na isti poziciji kot na serverju
            rb.AddForce(dir * 1500);
        }
    }

    public override void ApplyForceOnInstantiation(RpcArgs args)
    {
        Vector3 pos = args.GetNext<Vector3>();
        Vector3 dir = args.GetNext<Vector3>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero; //ce ma ksno gravitacijo
            rb.angularVelocity = Vector3.zero; //?

            transform.position = pos;//da se zacne na isti poziciji kot na serverju
            rb.AddForce(dir * 1500);
        }
    }

    internal override void setStartingInstantiationParameters(Predmet p, Vector3 pos, Vector3 dir)
    {
        if (networkObject.IsServer)
        {
            setForce(pos, dir);
            this.p = p;
            networkObject.SendRpc(RPC_SET_PREDMET_FOR_PICKUP, Receivers.Others, p.toNetworkString());

        }
        
    }

    public override void SetPredmetForPickup(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            Predmet k = new Predmet(null);
            k.setParametersFromNetworkString(args.GetNext<string>());
            this.p = k;
        }
    }

    public override void setMaterialGlow()
    {
        if (this.renderer == null) this.renderer = GetComponent<MeshRenderer>();
        if (this.renderer == null) this.renderer = GetComponentInChildren<MeshRenderer>();

        if(this.initialized)
            this.renderer.material = this.glow;
    }

    public override void resetMaterial()
    {
        if (this.renderer == null) this.renderer = GetComponent<MeshRenderer>();
        if (this.renderer == null) this.renderer = GetComponentInChildren<MeshRenderer>();
        this.renderer.material = this.original_material;
    }
}
