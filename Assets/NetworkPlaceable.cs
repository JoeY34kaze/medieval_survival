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
    [SerializeField]
    public Item item;

    [SerializeField]
    private float distanceForSnappingHandling = 0.05f;

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
        if (GetComponent<NetworkCraftingStation>() != null) GetComponent<NetworkCraftingStation>().init(this.p);
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
            NetworkPlayerNeutralStateHandler neutral_state_handler = FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerNeutralStateHandler>();


            if (networkObject.IsServer)
            {
                Debug.Log("server - placing " + neutral_state_handler.current_placeable_item.Display_name);

                Quaternion rot = args.GetNext<Quaternion>();

                //get current placeable predmet!
                Predmet p = neutral_state_handler.activePlaceable;
                Transform ch = transform.GetChild(args.GetNext<int>());
                if (ch.GetComponent<AttachmentPoint>().attached_placeable != null) {
                    Debug.LogError("Attachment slot occupied");
                    return;
                }

                if (!neutral_state_handler.currentTransformOfPlaceableIsValid(ch.transform.position)) return;
                
                NetworkPlaceable created = neutral_state_handler.NetworkPlaceableInstantiationServer(p, ch.position, rot);

                ch.GetComponent<AttachmentPoint>().attachTryReverse(created.gameObject);//proba nrdit obojestransko referenco ce se vse izide, sicer samo v eno smer

                //samo dve povezavi nista dovolj, ko se sklene krog je treba to tud ugotovit, recimo 4 foundationi

                created.refreshAttachmentPointOccupancy();

                
                

                player.GetComponent<NetworkPlayerInventory>().reduceCurrentActivePlaceable(neutral_state_handler.selected_index);//sicer vrne bool da nam pove ce smo pobral celotn stack, ampak nima veze ker rabmo poslat update za kvantiteto v vsakem primeru.
                                                                                                                //nastavi selected index na -1 ce smo pobral vse - da gre lepo v rpc             
                neutral_state_handler.sendBarUpdate();
            }



        }
    }


    /// <summary>
    /// metoda se klice ob networkInstantiation placeable objekta. metoda mora prever kaj se dogaja z attachment pointi tega objekta (če so znotraj druzga objekta) in povezat na soseda, ter v primeru da se dotika tud drugih pointov (recimo če smo postavli 2x2 povezat tud une.
    /// </summary>
    /// <param name="networkPlaceable"></param>
    private void refreshAttachmentPointOccupancy()
    {
        //poisc vse attachment pointe v dolocenem rangeu (1f recimo)

        AttachmentPoint[] all_AttachmentPoints = (AttachmentPoint[])GameObject.FindObjectsOfType<AttachmentPoint>();
        if (all_AttachmentPoints == null) return;
        all_AttachmentPoints = removeTakenAttachmentPoints(all_AttachmentPoints);
        if (all_AttachmentPoints == null) return;
        all_AttachmentPoints = removeNotValidSnappableTypeAttachmentPoints(all_AttachmentPoints, this.item.blocks_placements);

        foreach (AttachmentPoint p in all_AttachmentPoints) {


            if (p.isFree())
                if (p.acceptsAttachmentOfType(this.snappableType))
                    if (Vector3.Distance(p.gameObject.transform.position, this.transform.position) < this.distanceForSnappingHandling)
                    {
                        p.attachTryReverse(this.transform.gameObject);
                    }
        }
    }

    private AttachmentPoint[] GetAttachmentPointsInRangeForSnapping(AttachmentPoint[] all_valid)
    {
        List<AttachmentPoint> l = new List<AttachmentPoint>();
        foreach (AttachmentPoint p in all_valid) {
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if ( dist< this.distanceForSnappingHandling)
                l.Add(p);
            Debug.Log(Vector3.Distance(p.transform.position, transform.position));

        }

        return l.ToArray();
    }

    public AttachmentPoint[] GetAllValidAttachmentPoints(Item.SnappableType current_snappable_type)//kopiran z neutralstatehandlerja
    {
        AttachmentPoint[] all_AttachmentPoints = (AttachmentPoint[])GameObject.FindObjectsOfType<AttachmentPoint>();
        if (all_AttachmentPoints == null) return null;
        all_AttachmentPoints = removeTakenAttachmentPoints(all_AttachmentPoints);
        if (all_AttachmentPoints == null) return null;
        all_AttachmentPoints = removeNotValidSnappableTypeAttachmentPoints(all_AttachmentPoints, current_snappable_type);
        return all_AttachmentPoints;
    }

    private AttachmentPoint[] removeNotValidSnappableTypeAttachmentPoints(AttachmentPoint[] all_AttachmentPoints, Item.SnappableType snappable_type)
    {
        List<AttachmentPoint> l = new List<AttachmentPoint>();
        foreach (AttachmentPoint p in all_AttachmentPoints)
        {
                if (p.acceptsAttachmentOfType(snappable_type)) l.Add(p);
        }
        return l.ToArray();
    }

    private AttachmentPoint[] removeNotValidSnappableTypeAttachmentPoints(AttachmentPoint[] all_AttachmentPoints, Item.SnappableType[] snappable_types)
    {
        List<AttachmentPoint> l = new List<AttachmentPoint>();
        foreach (AttachmentPoint p in all_AttachmentPoints) {
            foreach(Item.SnappableType t in snappable_types)
            if (p.acceptsAttachmentOfType(t)) l.Add(p);
        }
        return l.ToArray();
    }

    private AttachmentPoint[] removeTakenAttachmentPoints(AttachmentPoint[] all_AttachmentPoints)//kopiran z neutralstatehandlerja
    {
        List<AttachmentPoint> l = new List<AttachmentPoint>();
        foreach (AttachmentPoint p in all_AttachmentPoints) if (p.isFree()) l.Add(p);

        return l.ToArray();
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

    internal bool is_placement_possible_for(Item current_placeable_item)
    {
        //gremo cez vse attachment pointe in ce najdemo en point ki je ze zaseden med blockerjim vrzemo false sicer true.
        for (int i = 0; i < this.attachmentPoints.Length; i++) {
            if (attachment_point_can_be_blocked_by(this.attachmentPoints[i], current_placeable_item.blocks_placements) ) {
                if (this.attachmentPoints[i].attached_placeable != null) return false;
            }
        }
        return true;
    }

    private bool attachment_point_can_be_blocked_by(AttachmentPoint attachmentPoint, Item.SnappableType[] blocks_placements)
    {
        for (int i = 0; i < blocks_placements.Length; i++)
            for (int j = 0; j < attachmentPoint.allowed_attachment_types.Length; j++)
                if (attachmentPoint.allowed_attachment_types[j] == blocks_placements[i])
                    return true;
        return false;
    }
}
