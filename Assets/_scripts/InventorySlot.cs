using System;
using UnityEngine;
using UnityEngine.UI;

/* Sits on all InventorySlots. */

public class InventorySlot : MonoBehaviour
{

    public Image icon;          // Reference to the Icon image
    Item item;  // Current item in the slot

    // Add item to the slot
    public void AddItem(Item newItem)
    {
        item = newItem;

        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public Item GetItem() {
        return this.item;
    }
    public Item PopItem() {
        if (this.item == null) return null;
        Item i = this.item;
        transform.root.GetComponent<NetworkPlayerInventory>().RemoveItem(getIndexFromName(transform.name));
        return i;
    }

    // Clear the slot
    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;
    }

    private int getIndexFromName(string name)
    {
        string[] a = name.Split('(');
        string[] b = a[a.Length - 1].Split(')');
        return Int32.Parse(b[0]);
    }

}