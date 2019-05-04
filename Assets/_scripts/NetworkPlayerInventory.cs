using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;

public class NetworkPlayerInventory : NetworkPlayerInventoryBehavior
{
    public int space = 20;
    public List<Item> items = new List<Item>();
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public GameObject panel_inventory;
    public GameObject inventorySlotsParent;
    InventorySlot[] slots;  // List of all the slots

    private void Start()
    {
        if (!networkObject.IsOwner) return;
        onItemChangedCallback += UpdateUI;    // Subscribe to the onItemChanged callback
        slots = inventorySlotsParent.GetComponentsInChildren<InventorySlot>();
    }


    void Update()
    {

        // Check to see if we should open/close the inventory
        if (Input.GetButtonDown("Inventory"))
        {
        panel_inventory.SetActive(!panel_inventory.activeSelf);
            if (panel_inventory.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        UpdateUI();


    }



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
        //NETWORKINSTANTIATE THE DROPPED ITEM!
        instantiateDroppedItem(item);
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    // Update the inventory UI by:
    //		- Adding items
    //		- Clearing empty slots
    // This is called using a delegate on the Inventory.
    void UpdateUI()
    {
        Debug.Log("Updating inventory ui");
        if(slots==null) slots = inventorySlotsParent.GetComponentsInChildren<InventorySlot>();
        // Loop through all the slots
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < this.items.Count)  // If there is an item to add
            {
                slots[i].AddItem(this.items[i]);   // Add it
            }
            else
            {
                // Otherwise clear the slot
                slots[i].ClearSlot();
            }
        }
    }


    public void RemoveItem(int inventory_slot)
    {
        Item itemToRemove = slots[inventory_slot].GetItem();
        this.Remove(itemToRemove);
    }


    private void instantiateDroppedItem(Item item)
    {
        NetworkManager.Instance.InstantiateInteractable_object(0, transform.position + transform.forward);
    }

    internal NetworkPlayerInventory link_inventory_to_ui_owner()
    {
        if (networkObject.IsOwner) return this;
        else return null;
    }
}
