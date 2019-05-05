using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class loadoutSlot : MonoBehaviour
{
    public int index;
    public Item.Type type;
    private Image icon_background;          // Reference to the Icon image to display when empty
    public Image icon;          // Reference to the Icon image
    public Item item = null;  // Current item in the slot

    private void Start()
    {
        icon_background = GetComponentInChildren<Image>();
    }

    // Add item to the slot
    public void AddItem(Item newItem)
    {
            item = newItem;
            icon.sprite = item.icon;
            icon.enabled = true;
    }

    private bool is_upgrade(Item newItem)//tole nj bi vrnil odgovor ce je item upgrade. tko k u apexu k zamenja, tam je precej straightforward
    {
        return true;
    }

    public Item GetItem()
    {
        return this.item;
    }

    public Item PopItem(Item.Type type) {//to je zato da bomo skenslal transakcijo ker item ne pase na taprav slot
        if (this.item == null) return null;
        if (!type.Equals(this.type)) {
            Debug.Log("wrong slot!");return null;

        }
        Item i = this.item;
        transform.root.GetComponent<NetworkPlayerInventory>().RemoveItemLoadout(type,index);
        return i;
    }


    public Item PopItem()
    {
        if (this.item == null) return null;
        Item i = this.item;
        transform.root.GetComponent<NetworkPlayerInventory>().RemoveItemLoadout(this.type,index);
        return i;
    }

    // Clear the slot
    public void ClearSlot()
    {
        item = null;
        icon.sprite = icon_background.sprite;
        icon.enabled = false;
    }
}
