using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPoint : MonoBehaviour
{
    public Item.SnappableType[] allowed_attachment_types;
    [SerializeField]
    private float distanceForSnappingHandling = 0.05f;

    public GameObject attached_placeable = null;

    internal bool acceptsAttachmentOfType(Item.SnappableType s)
    {
        foreach (Item.SnappableType t in this.allowed_attachment_types) if (t == s) return true;
        return false;
    }

    internal bool isFree()
    {
        return this.attached_placeable==null;
    }

    internal void local_placement_of_placeable_request(Quaternion rotation)
    {
        GetComponentInParent<NetworkPlaceable>().local_placement_of_placeable_request(rotation, transform.GetSiblingIndex());
    }

    internal void attach(GameObject gameObject)
    {
        this.attached_placeable = gameObject;
        GetComponentInParent<NetworkPlaceable>().sendAttachmentUpdate(transform.GetSiblingIndex(), true);
    }

    internal void detach()
    {
        this.attached_placeable = null;
        GetComponentInParent<NetworkPlaceable>().sendAttachmentUpdate(transform.GetSiblingIndex(), false);
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
        }
        else this.attached_placeable = null;
    }

    internal void attachTryReverse(GameObject gameObject)
    {
        attach(gameObject);

        //poiskat vse valid attachemnt pointe in izbrat najblizjo

        foreach (AttachmentPoint p in gameObject.GetComponentsInChildren<AttachmentPoint>()) {
            if (p.isFree())
                if (p.acceptsAttachmentOfType(GetComponentInParent<NetworkPlaceable>().snappableType))
                    if (Vector3.Distance(p.gameObject.transform.position, this.transform.parent.position) < this.distanceForSnappingHandling)
                    {//naceloma bi mogu bit samo edn so
                        p.attach(this.transform.parent.gameObject);
                    }
        }

    }
}
