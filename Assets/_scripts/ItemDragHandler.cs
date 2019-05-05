using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{


    public void OnBeginDrag(PointerEventData eventData)
    {
        //transform.root.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
        if (transform.root.GetComponent<NetworkPlayerInventory>().draggedItemParent == null)
            transform.root.GetComponent<NetworkPlayerInventory>().draggedItemParent = transform.parent;
        Debug.Log("start drag " + transform.parent.name);
        //transform.parent = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end drag "+transform.parent.name);

        transform.localPosition = Vector3.zero;
        //transform.root.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
    }

}
