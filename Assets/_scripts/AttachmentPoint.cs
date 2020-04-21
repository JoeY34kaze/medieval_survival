using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPoint : MonoBehaviour
{
    public Item.SnappableType[] allowed_attachment_types;
    [SerializeField]
    private float distanceForSnappingHandling = 0.05f;

    public  bool blocking = false;

    public GameObject attached_placeable = null;

    internal bool acceptsAttachmentOfType(Item.SnappableType s)
    {
        foreach (Item.SnappableType t in this.allowed_attachment_types) if (t == s) return true;
        return false;
    }

    internal bool isFree()
    {
        return !this.blocking && this.attached_placeable==null;
    }

    internal void local_placement_of_placeable_request(Quaternion rotation)
    {
        GetComponentInParent<NetworkPlaceable>().local_placement_of_placeable_request(rotation, transform.GetSiblingIndex());
    }

    internal void attach(GameObject gameObject)
    {
        this.attached_placeable = gameObject;
        this.blocking = true;
        GetComponentInParent<NetworkPlaceable>().sendAttachmentUpdate(transform.GetSiblingIndex(), true);
    }

    /// <summary>
    /// local_only is set to true ONLY when called from OnDestroyed method because we CANNOT send any rpc's anymore.
    /// </summary>
    /// <param name="ony_server"></param>
    internal NetworkPlaceable detach(bool local_only)
    {
        NetworkPlaceable for_return = this.attached_placeable.GetComponent<NetworkPlaceable>();
        this.attached_placeable = null;
        this.blocking = false;
        if(!local_only) GetComponentInParent<NetworkPlaceable>().sendAttachmentUpdate(transform.GetSiblingIndex(), false);
        return for_return;
    }

    /// <summary>
    /// client nebo vidu istih objektov da je gor attachan k vidi server. client dobi samo true ali false ce je zaseden in kar se tice postavlanja je to dovolj, also brez iudjev objektov je to tud bl tezko nrdit kj bolsga
    /// </summary>
    /// <param name="getNext"></param>
    internal void setAttachedClient(bool stat)
    {
        if (stat)
        {//occupied
            this.attached_placeable = gameObject;//bomo kr nrdil pointer samga nase good enough. za unicevanje pa take fore bomo pa na serverju spawnal in client nerab met. stability bo pa field k se nastav ob dodajanju / odstranjevanju objektov in se zracuna na serverju pa nastavi clientim i guess
            this.blocking = true;
        }
        else { this.attached_placeable = null; this.blocking = false; }
    }

    internal void attachTryReverse(GameObject trenutnoPostavljamo)
    {
        Item i = trenutnoPostavljamo.GetComponent<NetworkPlaceable>().p.getItem();
        attach(trenutnoPostavljamo);

        //poiskat vse valid attachemnt pointe in izbrat najblizjo


        //tole je za poiskat in blokirat attachment pointe, ki so se spawnale sedaj, ki smo nov objekt postavili, in so na lokaciji, kjer objekt ze obstaja. ce tega ni, pride do bugga
        //kjer lahko postavljamo en objekt cez druzga do neskoncnosti. prvi if blokira glede na RAZDALJO med objektoma in TIPOM objekta.
        if (i.PlacementType == Item.SnappableType.foundation || i.PlacementType == Item.SnappableType.wall || i.PlacementType == Item.SnappableType.ceiling || i.PlacementType == Item.SnappableType.door_frame || i.PlacementType == Item.SnappableType.windows_frame)
            foreach (AttachmentPoint p in trenutnoPostavljamo.GetComponentsInChildren<AttachmentPoint>())
            {
                if (p.isFree())
                    if (p.acceptsAttachmentOfType(GetComponentInParent<NetworkPlaceable>().snappableType))
                        if (Vector3.Distance(p.gameObject.transform.position, this.transform.parent.position) < this.distanceForSnappingHandling)
                        {//naceloma bi mogu bit samo edn so
                            p.attach(this.transform.parent.gameObject);
                        }
            }
        else if (i.PlacementType == Item.SnappableType.stairs_narrow || i.PlacementType == Item.SnappableType.stairs_wide ) {
            //na tem foundationu poiskat vse attackment pointe k so valid za stenge in jih disablat
            foreach (AttachmentPoint p in this.gameObject.transform.parent.GetComponentsInChildren<AttachmentPoint>()) {
                if (p.acceptsAttachmentOfType(i.PlacementType))
                    p.block_placements();
            }
        }

    }

    internal void block_placements()
    {
        this.blocking = true;
    }

    internal bool is_sibling_attachment_point_occupied_by(GameObject blocker)
    {
        foreach (AttachmentPoint sibling in transform.parent.GetComponentsInChildren<AttachmentPoint>()) {
            if (!sibling.Equals(this)) {
                if (sibling.attached_placeable.Equals(blocker))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// parent placeable was destroyed. we need to handle behaviour of any attached objects.
    /// </summary>
    /// <param name="networkPlaceable"></param>
    internal void OnPlaceableDestroyed(NetworkPlaceable destroyedPlaceable)
    {
        if (attached_placeable != null) {//ce je nekaj attachan gor (recimo foudnation -> wall)
            if (attached_placeable.GetComponent<NetworkPlaceable>().p.getItem().needsToBeAttached) {//ce je attachan objekt nekaj kar nemore obstajat sam po seb ( foundation lahko | wall nemore )
                if (!attached_placeable.GetComponent<NetworkPlaceable>().isAttachedToAlternativeAttachmentPoint(this)) { //pogleda na ta placeable (wall ki je zgubil attachment) če se lahko attacha nekam drugam na valid
                    NetworkPlaceable previous = this.detach(true);
                    previous.handle_object_destruction(); //ce je tukaj pomeni da se ni mogel attachat nikamor. treba ga je unicit in pohendlat unicenje naprej.
                }
            }
        }
    }

    internal bool has_attached_placeable_that_block_placement_of(Item.SnappableType placementType)
    {
        if (this.attached_placeable != null)
            foreach (Item.SnappableType s in this.attached_placeable.GetComponent<NetworkPlaceable>().p.getItem().blocks_placements)
                if (s == placementType)
                    return true;
        return false;
    }
}
