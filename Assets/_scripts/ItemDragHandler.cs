using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// skripta pase na image componento panele v inventoriju al pa loadoutu. to je item, ki se dragga okol na drugo mesto.
/// </summary>
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{


    public void OnBeginDrag(PointerEventData eventData)
    {
        //transform.root.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
        //if (transform.root.GetComponent<NetworkPlayerInventory>().draggedItemParent == null)
            transform.root.GetComponent<NetworkPlayerInventory>().draggedItemParent = transform.parent;

        //hierarhijo zrihtat ker je unity ui prizadet
        transform.parent.parent.SetAsFirstSibling();//tole menja loadout panel in inventory panel. 

        //pofiksat hierarhijo se za personal inventorij in loadout ker sicer ne detecta ker je unity ui prizadet
        transform.parent.SetAsFirstSibling();

        Debug.Log("start drag " + transform.parent.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end drag "+transform.parent.name);

        transform.localPosition = Vector3.zero;
    }

}
