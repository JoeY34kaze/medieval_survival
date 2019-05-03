using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;

/* This object updates the inventory UI. */

public class InventoryUI : MonoBehaviour
{

    public Transform itemsParent;   // The parent object of all the items
    public GameObject inventoryUI;  // The entire UI

    private NetworkPlayerInventory npInventory;
    private List<Item> inventory_list;    // Our current inventory

    InventorySlot[] slots;  // List of all the slots

    void Start()
    {

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
    }

    public void linkPersonalInventoryList(NetworkPlayerInventory npi) {
        Debug.Log("LINKING");
        npInventory = npi;
        inventory_list = npi.items;
        npInventory.onItemChangedCallback += UpdateUI;    // Subscribe to the onItemChanged callback
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
    }

    public bool isSetup() {
        if (npInventory == null || inventory_list == null) return false;
        else return true;
    }

    void Update()
    {
        if (npInventory == null) request_link_with_owner_player();
        if (npInventory != null)
        {
            //Debug.Log("networkPlayerInventory not null");
            // Check to see if we should open/close the inventory
            if (Input.GetButtonDown("Inventory"))
            {
                inventoryUI.SetActive(!inventoryUI.activeSelf);
                if (inventoryUI.activeSelf)
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
        }
        else { Debug.Log("This should never be showing. what the fuck...");
        }
    }

    private void request_link_with_owner_player()
    {
        NetworkPlayerInventory npi_fix = null;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            npi_fix=player.GetComponent<NetworkPlayerInventory>().link_inventory_to_ui_owner();
            if (npi_fix != null) {
                this.npInventory = npi_fix;
                this.inventory_list = npi_fix.items;
                return;
            }
        }
    }

    // Update the inventory UI by:
    //		- Adding items
    //		- Clearing empty slots
    // This is called using a delegate on the Inventory.
    void UpdateUI()
    {

        // Loop through all the slots
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory_list.Count)  // If there is an item to add
            {
                slots[i].AddItem(inventory_list[i]);   // Add it
            }
            else
            {
                // Otherwise clear the slot
                slots[i].ClearSlot();
            }
        }
    }

    public void RemoveItem(int inventory_slot) {
        Item removedItem = slots[inventory_slot].GetItem();
        npInventory.Remove(removedItem);
    }
}