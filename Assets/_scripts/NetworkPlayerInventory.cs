using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;

public class NetworkPlayerInventory : NetworkPlayerInventoryBehavior
{
    public int space = 20; // kao space inventorija
    public List<Item> items = new List<Item>(); // seznam itemov, ubistvu inventorij
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public GameObject panel_inventory; //celotna panela za inventorij, to se izrise ko prtisnes "i"
    public GameObject inventorySlotsParent; // parent object od slotov da jih komot dobis v array
    InventorySlot[] slots;  // predstavlajo slote v inventoriju, vsak drzi en item. 

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

        UpdateUI(); // za test sm dau


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


    public void RemoveItem(int inventory_slot) // to se klice z slota na OnDrop eventu
    {
        Item itemToRemove = slots[inventory_slot].GetItem();
        this.Remove(itemToRemove);
    }


    private void instantiateDroppedItem(Item item) // instantiate it when dropped
    {
        NetworkManager.Instance.InstantiateInteractable_object(0, transform.position + transform.forward);
    }
}
