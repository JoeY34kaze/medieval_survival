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
        if (this.npi == null) link_local_player();

        npi.dragged_gameobjectSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsFirstSibling();


        npi.draggedItemParent = transform.parent;

        bool endReached = false;

        if (transform.parent != null)
            if (transform.parent.GetComponent<UILogic>() == null) {//ni se canvas. pr canvasu se bomo ustavli

                npi.draggedGameobjectParentSiblingIndex = transform.parent.GetSiblingIndex();
                transform.parent.SetAsFirstSibling();
            } else
                endReached = true;
        else endReached = true;


        if (transform.parent.parent != null && !endReached)
            if (transform.parent.parent.GetComponent<UILogic>() == null)
            {//ni se canvas. pr canvasu se bomo ustavli
                npi.draggedGameobjectParent_parentSiblingIndex = transform.parent.parent.GetSiblingIndex();
                transform.parent.parent.SetAsFirstSibling();
            }
            else endReached = true;
        else endReached = true;

        if (transform.parent.parent.parent != null && !endReached)
            if (transform.parent.parent.parent.GetComponent<UILogic>() == null)
            {//ni se canvas. pr canvasu se bomo ustavli
                npi.draggedGameobjectParent_parent_parentSiblingIndex = transform.parent.parent.parent.GetSiblingIndex();
                transform.parent.parent.parent.SetAsFirstSibling();
            }
            else endReached = true;
        else endReached = true;

        
        if (transform.parent.parent.parent.parent != null && !endReached)
            if (transform.parent.parent.parent.parent.GetComponent<UILogic>() == null)
            {//ni se canvas. pr canvasu se bomo ustavli
                npi.draggedGameobjectParent_parent_parent_parentSiblingIndex = transform.parent.parent.parent.parent.GetSiblingIndex();
                transform.parent.parent.parent.parent.SetAsFirstSibling();
            }
            else endReached = true;
        else endReached = true;

        //pofiksat hierarhijo se za personal inventorij in loadout ker sicer ne detecta ker je unity ui prizadet

        Debug.Log("start drag " + transform.parent.name + " | " + npi.draggedItemParent.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
       
        transform.position = Input.mousePosition;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(this.npi==null) link_local_player();
        Debug.Log("end drag -"+transform.parent.name);
        transform.SetSiblingIndex(npi.dragged_gameobjectSiblingIndex);

        bool endReached = false;

        if (transform.parent != null)
            if (transform.parent.GetComponent<UILogic>() == null)
            {//ni se canvas. pr canvasu se bomo ustavli
                transform.parent.SetSiblingIndex(npi.draggedGameobjectParentSiblingIndex);
            }
            else
                endReached = true;
        else endReached = true;


        if (transform.parent.parent != null && !endReached)
            if (transform.parent.parent.GetComponent<UILogic>() == null)
            {//ni se canvas. pr canvasu se bomo ustavli
                transform.parent.parent.SetSiblingIndex(npi.draggedGameobjectParent_parentSiblingIndex);
            }
            else endReached = true;
        else endReached = true;

        if (transform.parent.parent.parent != null && !endReached)
            if (transform.parent.parent.parent.GetComponent<UILogic>() == null)
            {//ni se canvas. pr canvasu se bomo ustavli
                transform.parent.parent.parent.SetSiblingIndex(npi.draggedGameobjectParent_parent_parentSiblingIndex);
            }
            else endReached = true;
        else endReached = true;


        if (transform.parent.parent.parent.parent != null && !endReached)
            if (transform.parent.parent.parent.parent.GetComponent<UILogic>() == null)
            {//ni se canvas. pr canvasu se bomo ustavli
                transform.parent.parent.parent.parent.SetSiblingIndex(npi.draggedGameobjectParent_parent_parent_parentSiblingIndex);
            }
            else endReached = true;
        else endReached = true;

        
        transform.localPosition = Vector3.zero;
        npi.dragged_gameobjectSiblingIndex = -1;
        npi.draggedItemParent = null;
        npi.draggedGameobjectParentSiblingIndex = -1;
        npi.draggedGameobjectParent_parentSiblingIndex = -1;
        npi.draggedGameobjectParent_parent_parentSiblingIndex = -1;
        npi.draggedGameobjectParent_parent_parent_parentSiblingIndex = -1;
    }



    internal void link_local_player()
    {
        if (UILogic.localPlayerGameObject != null)
            this.npi = UILogic.localPlayerGameObject.GetComponent<NetworkPlayerInventory>();
    }
}
