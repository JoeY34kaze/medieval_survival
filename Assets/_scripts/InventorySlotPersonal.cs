using System;
using UnityEngine;
using UnityEngine.UI;

/* Sits on all InventorySlots. */

public class InventorySlotPersonal : InventorySlot
{


    public override Item PopItem() {
        if (this.item == null) return null;
        Item i = this.item;
        transform.root.GetComponent<NetworkPlayerInventory>().RemoveItem(getIndexFromName(transform.name));
        return i;
    }


    private int getIndexFromName(string name)
    {
        string[] a = name.Split('(');
        string[] b = a[a.Length - 1].Split(')');
        return Int32.Parse(b[0]);
    }

}