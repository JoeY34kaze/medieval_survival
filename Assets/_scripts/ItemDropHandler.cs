using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// skripta lezi na paneli inventory slota. kontrolira kaj se dogaja z itemom, ki ga dropa na to panelo
/// </summary>
public class ItemDropHandler : MonoBehaviour, IDropHandler , IPointerClickHandler
{
    private NetworkPlayerInventory networkPlayerInventory;

    private void Start()
    {
        networkPlayerInventory = transform.root.GetComponent<NetworkPlayerInventory>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        RectTransform invSlot = transform as RectTransform;
        Debug.Log("dropped "+invSlot.name);
        if (GetComponent<InventorySlot>().GetPredmet() != null) {//ce smo potegnil backpack loh sam vrzemo na tla. nemors ga dat u inventorij k ni tak item
            if (GetComponent<InventorySlot>().GetPredmet().item.type == Item.Type.backpack)
            {
                networkPlayerInventory.backpackSpot.GetComponentInChildren<NetworkBackpack>().local_player_backpack_unequip_request();
                return;
            }
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(invSlot, Input.mousePosition))//smo dropal nekam na valid inventorij plac
        {
            //Debug.Log(networkPlayerInventory.draggedItemParent.name+"'s child was dropped on " + invSlot.name+" ");
            InventorySlot to = invSlot.GetComponent<InventorySlot>();
            //koda se bo malo podvajala ker bi sicer biu prevelik clusterfuck od metode
            InventorySlot from = networkPlayerInventory.draggedItemParent.GetComponent<InventorySlot>();
            if ((from is InventorySlotPersonal || from is InventorySlotLoadout) && ((to is InventorySlotLoadout) || (to is InventorySlotPersonal)))//vsa interakcija med loadoutom in personal inventorijem. una velika metoda v npi
                networkPlayerInventory.handleInventorySlotOnDragDropEvent(invSlot, null, false);
            else if (from is InventorySlotLoadout && to is InventorySlotBackpack)
                networkPlayerInventory.handleLoadoutToBackpackDrag(invSlot);
            else if (from is InventorySlotPersonal && to is InventorySlotBackpack)
                networkPlayerInventory.handlePersonalToBackpackDrag(invSlot);
            else if (from is InventorySlotBackpack && to is InventorySlotPersonal)
                networkPlayerInventory.handleBackpackToPersonalDrag(invSlot);
            else if (from is InventorySlotBackpack && to is InventorySlotBackpack)
                networkPlayerInventory.handleBackpackToBackpack(invSlot);
            else if (from is InventorySlotBackpack && to is InventorySlotBar)
                networkPlayerInventory.handleBackpackToBar(invSlot);
            else if (from is InventorySlotBar && to is InventorySlotBackpack)
                networkPlayerInventory.handleBarToBackpack(invSlot);
            else if (from is InventorySlotBackpack)
                networkPlayerInventory.handleBackpackSlotOnDragDropEvent(invSlot, null);
            else if (from is InventorySlotPersonal && to is InventorySlotBar)
                networkPlayerInventory.handlePersonalToBar(invSlot);
            else if (from is InventorySlotBar && to is InventorySlotPersonal)
                networkPlayerInventory.handleBarToPersonal(invSlot);
            else if (from is InventorySlotBar && to is InventorySlotBar)
                networkPlayerInventory.handleBarToBar(invSlot);
        }
        else
        {//smo dropal nekam tko da mora past na tla. gremo prevert s kje smo potegnil
            InventorySlot inventorySlot = networkPlayerInventory.draggedItemParent.GetComponent<InventorySlot>();
            //Debug.Log("Called on " + gameObject.name);
            if (inventorySlot is InventorySlotPersonal) networkPlayerInventory.DropItemFromPersonalInventory(getIndexFromName(invSlot.name));
            else if (inventorySlot is InventorySlotLoadout)
            {
                InventorySlotLoadout ldslt = (InventorySlotLoadout)inventorySlot;
                networkPlayerInventory.DropItemFromLoadout(ldslt.type, ldslt.index);
                //transform.root.GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons(); - to se mora klicat ko server sporoci novo stanje

            }
            else if (inventorySlot is InventorySlotBackpack)
                this.networkPlayerInventory.backpack_inventory.localPlayerdropItemFromBackpackRequest(this.networkPlayerInventory.getIndexFromName(networkPlayerInventory.draggedItemParent.name));
            else if (inventorySlot is InventorySlotBar) this.networkPlayerInventory.localPlayerDropFromBarRequest(this.networkPlayerInventory.getIndexFromName(networkPlayerInventory.draggedItemParent.name));
        }
    }




    private int getIndexFromName(string name)
    {
        string[] a = name.Split('(');
        string[] b = a[a.Length - 1].Split(')');
        return Int32.Parse(b[0]);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventorySlot iss= GetComponent<InventorySlot>();
        if (eventData.button == PointerEventData.InputButton.Right && iss.GetPredmet() != null)
        {
            if(iss.GetPredmet().item.type==Item.Type.backpack)//ce je backpack ga dropa
                networkPlayerInventory.backpackSpot.GetComponentInChildren<NetworkBackpack>().local_player_backpack_unequip_request();
            else if(iss is InventorySlotPersonal || iss is InventorySlotLoadout)//ce je personal inventorij al pa loadout k je blo to spisan ze prej
                networkPlayerInventory.OnRightClick(gameObject);
            else if(iss is InventorySlotBackpack)
                networkPlayerInventory.OnRightClickBackpack(gameObject);
        }
    }
}
