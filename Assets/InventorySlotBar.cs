using System;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotBar : InventorySlot
{
    private int getIndexFromName(string name)
    {
        string[] a = name.Split('(');
        string[] b = a[a.Length - 1].Split(')');
        return Int32.Parse(b[0]);
    }
}
