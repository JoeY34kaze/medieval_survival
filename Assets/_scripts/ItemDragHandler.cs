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
    private NetworkPlayerInventory npi;

    public void OnBeginDrag(PointerEventData eventData)
    {
        //transform.root.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
        //if (transform.root.GetComponent<NetworkPlayerInventory>().draggedItemParent == null)
        npi.draggedItemParent = transform.parent;
        npi.draggedParent_sibling_index = transform.GetSiblingIndex();
        if (transform.GetComponent<InventorySlot>() is InventorySlotPersonal) {
            transform.SetAsFirstSibling();
        }
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
        if (transform.GetComponent<InventorySlot>() is InventorySlotPersonal)
        {
            transform.SetSiblingIndex(npi.draggedParent_sibling_index);
        }
        transform.localPosition = Vector3.zero;
        npi.draggedParent_sibling_index = -1;
        npi.draggedItemParent = null;
    }

    public void Start()
    {
        this.npi = transform.root.GetComponent<NetworkPlayerInventory>();
    }

}
