using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        RectTransform invSlot = transform as RectTransform;

        if (RectTransformUtility.RectangleContainsScreenPoint(invSlot, Input.mousePosition))
        {
            Debug.Log("Place item in inventory on index " + getIndexFromName(invSlot.name));

        }
        else {

            Debug.Log("Remove item " + invSlot.name);
            transform.parent.GetComponent<InventoryUI>().RemoveItem(getIndexFromName(invSlot.name));
        }
    }

    private int getIndexFromName(string name)
    {
        string[] a = name.Split('(');
        string[] b = a[a.Length - 1].Split(')');
        return Int32.Parse(b[0]);
    }
}
