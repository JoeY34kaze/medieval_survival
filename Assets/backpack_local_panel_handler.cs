using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backpack_local_panel_handler : MonoBehaviour
{
    public GameObject InventorySlotBackpackPrefab;
    public int size;
    private NetworkContainer_items nci;
    private InventorySlotBackpack[] slots;
    internal void updateUI()
    {
        Item[] items = nci.getAll();
        for (int i = 0; i < this.size; i++) {
            if (items[i] != null)  // If there is an item to add
            {
                slots[i].AddItem(items[i]);   // Add it
            }
            else
            {
                // Otherwise clear the slot
                slots[i].ClearSlot();
            }
        }


    }

    internal void init(int size,NetworkContainer_items nci)//size dobi z item.size
    {
        this.nci = nci;//nci je najprej prazn. po tej metodi dobi sele updejt o itemih
        this.size = size;
        this.slots = new InventorySlotBackpack[this.size];
        for (int i = 0; i < this.size; i++) {
           GameObject g= GameObject.Instantiate(InventorySlotBackpackPrefab);
            g.transform.SetParent(transform);
            this.slots[i] = g.GetComponent<InventorySlotBackpack>();
        }
       
    }

    internal void clear()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        this.size = 0;
        this.nci = null;
        this.slots = null;
    }
}
