using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

/* This object updates the inventory UI. */

public class InventoryUI : MonoBehaviour
{

    public Transform itemsParent;   // The parent object of all the items
    public GameObject inventoryUI;  // The entire UI

    public NetworkPlayerInventory npInventory;
    private List<Item> inventory_list;    // Our current inventory

    InventorySlot[] slots;  // List of all the slots

    void Start()
    {

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
    }

    public void linkPersonalInventoryList(NetworkPlayerInventory npi) {

        npInventory = npi;
        inventory_list = npi.items;
        npInventory.onItemChangedCallback += UpdateUI;    // Subscribe to the onItemChanged callback
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
    }

    void Update()
    {

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
        else { //Debug.Log("This should never be showing. what the fuck...");
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
}