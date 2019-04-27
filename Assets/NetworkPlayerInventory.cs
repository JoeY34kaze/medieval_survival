﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

public class NetworkPlayerInventory : NetworkPlayerInventoryBehavior
{
    public int space = 20;
    public List<Item> items = new List<Item>();
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    private InventoryUI inventoryUi;

    //private bool linked_with_InventoryUI = false;
    private void Start()
    {
        if (!networkObject.IsOwner) return;
        inventoryUi =Mapper.instance.GetComponentInChildren<InventoryUI>();
        inventoryUi.linkPersonalInventoryList(this);
    }


   /* public void Add(Item item) {
        if (!networkObject.IsOwner) return;
        items.Add(item);
        if(onItemChangedCallback!=null)
            onItemChangedCallback.Invoke();
    }*/

    public void Add(Item item, int quantity)
    {
        if (!networkObject.IsOwner) return;
        items.Add(item);//nekej bo treba nrdit za hranjenje kolicine. recimo kamen pa take fore
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
    public void Remove(Item item)
    {
        if (!networkObject.IsOwner) return;
        items.Remove(item);
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
}
