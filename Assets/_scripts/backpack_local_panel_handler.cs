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
        Predmet[] predmeti = nci.getAll();
        for (int i = 0; i < this.size; i++) {
            if (predmeti[i] != null)  // If there is an item to add
            {
                slots[i].AddPredmet(predmeti[i]);   // Add it
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
           GameObject g= GameObject.Instantiate(InventorySlotBackpackPrefab, transform);
            g.transform.localScale = new Vector3(1, 1, 1);
            this.slots[i] = g.GetComponent<InventorySlotBackpack>();
            g.name = "InventorySlotBackpack (" + i + ")";
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
