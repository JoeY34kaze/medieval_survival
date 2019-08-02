using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

/// <summary>
/// stvari k so skupne vsem stvarem k jih loh postavs. health, lastnistvo, take fore. boljs specificne stvari nrdit svojo skripto i guess
/// </summary>
public class NetworkPlaceable : NetworkPlaceableBehavior
{
    public Predmet p;
    [SerializeField]
    public Item.SnappableType snappableType;

    [SerializeField]
    private AttachmentPoint[] attachmentPoints; 

    public void init(Predmet p)
    {
        this.attachmentPoints = GetComponentsInChildren<AttachmentPoint>();

        this.p = p;
        //server mu nastavi vse stvari k jih rab nastavt ob instanciaciji objekta.
        if (GetComponent<NetworkChest>() != null) GetComponent<NetworkChest>().init(this.p);
        //
    }

    internal void sendAttachmentUpdate(int sibling, bool boo)
    {
        if (networkObject.IsServer) networkObject.SendRpc(RPC_NETWORK_ATTACHMENT_UPDATE, Receivers.Others, sibling, boo);
    }

    internal void local_placement_of_placeable_request(Quaternion rotation, int v)
    {
        //tukej klicemo rpc za postavlanje objekta k je snappan na nekej.!
        networkObject.SendRpc(RPC_NETWORK_PLACEABLE_ATTACHMENT_REQUEST, Receivers.Server, rotation, v);
    }

    public override void NetworkPlaceableAttachmentRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            GameObject player = FindByid(args.Info.SendingPlayer.NetworkId);
            NetworkPlayerNeutralStateHandler ntrl = FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerNeutralStateHandler>();


            if (networkObject.IsServer)
            {
                Debug.Log("server - placing " + ntrl.current_placeable_item.Display_name);

                Quaternion rot = args.GetNext<Quaternion>();

                //get current placeable predmet!
                Predmet p = ntrl.activePlaceable;
                Transform ch = transform.GetChild(args.GetNext<int>());
                if (ch.GetComponent<AttachmentPoint>().attached_placeable != null) {
                    Debug.LogError("Attachment slot occupied");
                    return;
                }

                if (!ntrl.currentTransformOfPlaceableIsValid(ch.transform.position)) return;
                
                NetworkPlaceable created = ntrl.NetworkPlaceableInstantiationServer(p, ch.position, rot);

                ch.GetComponent<AttachmentPoint>().attach(created.gameObject);
                

                player.GetComponent<NetworkPlayerInventory>().reduceCurrentActivePlaceable(ntrl.selected_index);//sicer vrne bool da nam pove ce smo pobral celotn stack, ampak nima veze ker rabmo poslat update za kvantiteto v vsakem primeru.
                                                                                                                //nastavi selected index na -1 ce smo pobral vse - da gre lepo v rpc             
                ntrl.sendBarUpdate();
            }



        }
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana povsod
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        return null;
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();//server prevzame ownership


            /*
         networking sinhronizacija...... kaj se nrdi ob connectu, kko poupdejtamo med clienti pa take fore    
         
         */
        }
    }

    public List<AttachmentPoint> getAttachmentPointsForType(Item.SnappableType s) {
        List<AttachmentPoint> r = new List<AttachmentPoint>();
        foreach (AttachmentPoint t in this.attachmentPoints) {
            if (t.acceptsAttachmentOfType(s)) r.Add(t);
        }
        return r;
    }

    public override void NetworkAttachmentUpdate(RpcArgs args)
    {
        //sendingPlayer je zmer server so..
        transform.GetChild(args.GetNext<int>()).GetComponent<AttachmentPoint>().setAttachedClient(args.GetNext<bool>());
    }
}
