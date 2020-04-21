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
    private NetworkPlayerInventory npi;

    internal void link_local_player() {
        if(UILogic.localPlayerGameObject!=null)
            this.npi = UILogic.localPlayerGameObject.GetComponent<NetworkPlayerInventory>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (this.npi == null) link_local_player();
        RectTransform invSlot = transform as RectTransform;
        Debug.Log("dropped "+invSlot.name);
        if (GetComponent<InventorySlot>().GetPredmet() != null) {//ce smo potegnil backpack loh sam vrzemo na tla. nemors ga dat u inventorij k ni tak item
            if (GetComponent<InventorySlot>().GetPredmet().getItem().type == Item.Type.backpack)
            {
                this.npi.backpackSpot.GetComponentInChildren<NetworkBackpack>().local_player_backpack_unequip_request();
                return;
            }
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(invSlot, Input.mousePosition))//smo dropal nekam na valid inventorij plac
        {
            //Debug.Log(networkPlayerInventory.draggedItemParent.name+"'s child was dropped on " + invSlot.name+" ");
            InventorySlot to = invSlot.GetComponent<InventorySlot>();
            //koda se bo malo podvajala ker bi sicer biu prevelik clusterfuck od metode
            InventorySlot from = this.npi.draggedItemParent.GetComponent<InventorySlot>();
            if ((from is InventorySlotPersonal || from is InventorySlotLoadout) && ((to is InventorySlotLoadout) || (to is InventorySlotPersonal)))//vsa interakcija med loadoutom in personal inventorijem. una velika metoda v npi
                this.npi.handleInventorySlotOnDragDropEvent(invSlot, null, false);
            else if (from is InventorySlotLoadout && to is InventorySlotBackpack)
                this.npi.handleLoadoutToBackpackDrag(invSlot);
            else if (from is InventorySlotPersonal && to is InventorySlotBackpack)
                this.npi.handlePersonalToBackpackDrag(invSlot);
            else if (from is InventorySlotBackpack && to is InventorySlotPersonal)
                this.npi.handleBackpackToPersonalDrag(invSlot);
            else if (from is InventorySlotBackpack && to is InventorySlotBackpack)
                this.npi.handleBackpackToBackpack(invSlot);
            else if (from is InventorySlotBackpack && to is InventorySlotBar)
                this.npi.handleBackpackToBar(invSlot);
            else if (from is InventorySlotBar && to is InventorySlotBackpack)
                this.npi.handleBarToBackpack(invSlot);
            else if (from is InventorySlotBackpack)
                this.npi.handleBackpackSlotOnDragDropEvent(invSlot, null);
            else if (from is InventorySlotPersonal && to is InventorySlotBar)
                this.npi.handlePersonalToBar(invSlot);
            else if (from is InventorySlotBar && to is InventorySlotPersonal)
                this.npi.handleBarToPersonal(invSlot);
            else if (from is InventorySlotBar && to is InventorySlotBar)
                this.npi.handleBarToBar(invSlot);
            else if (from is InventorySlotPersonal && to is InventorySlotContainer)
                this.npi.handlePersonalToContainer(invSlot);
            else if (from is InventorySlotContainer && to is InventorySlotPersonal)
                this.npi.handleContainerToPersonal(invSlot);
            else if (from is InventorySlotBackpack && to is InventorySlotContainer)
                this.npi.handleBackpackToContainer(invSlot);
            else if (from is InventorySlotContainer && to is InventorySlotBackpack)
                this.npi.handleContainerToBackpack(invSlot);
            else if (from is InventorySlotBar && to is InventorySlotContainer)
                this.npi.handleBarToContainer(invSlot);
            else if (from is InventorySlotContainer && to is InventorySlotBar)
                this.npi.handleContainerToBar(invSlot);
            else if (from is InventorySlotLoadout && to is InventorySlotContainer)
                this.npi.handleLoadoutToContainer(invSlot);
            else if (from is InventorySlotContainer && to is InventorySlotLoadout)
                this.npi.handleContainerToLoadout(invSlot);
            else if (from is InventorySlotContainer && to is InventorySlotContainer)
                this.npi.handleContainerToContainer(invSlot);


        }
        else
        {//smo dropal nekam tko da mora past na tla. gremo prevert s kje smo potegnil
            InventorySlot inventorySlot = this.npi.draggedItemParent.GetComponent<InventorySlot>();
            //Debug.Log("Called on " + gameObject.name);
            if (inventorySlot is InventorySlotPersonal) this.npi.DropItemFromPersonalInventory(getIndexFromName(invSlot.name));
            else if (inventorySlot is InventorySlotLoadout)
            {
                InventorySlotLoadout ldslt = (InventorySlotLoadout)inventorySlot;
                this.npi.DropItemFromLoadout(ldslt.type, ldslt.index);
                //transform.root.GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons(); - to se mora klicat ko server sporoci novo stanje

            }
            else if (inventorySlot is InventorySlotBackpack)
                this.npi.backpack_inventory.localPlayerdropItemFromBackpackRequest(this.npi.getIndexFromName(this.npi.draggedItemParent.name));
            else if (inventorySlot is InventorySlotBar) this.npi.localPlayerDropFromBarRequest(this.npi.getIndexFromName(this.npi.draggedItemParent.name));
            else if (inventorySlot is InventorySlotContainer) this.npi.localPlayerDropItemFromContainerRequest(this.npi.getIndexFromName(this.npi.draggedItemParent.name));
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
        if (this.npi == null) link_local_player();
        InventorySlot iss= GetComponent<InventorySlot>();
        if (eventData.button == PointerEventData.InputButton.Right && iss.GetPredmet() != null)
        {
            if (iss.GetPredmet().getItem().type == Item.Type.backpack)//ce je backpack ga dropa
                this.npi.backpackSpot.GetComponentInChildren<NetworkBackpack>().local_player_backpack_unequip_request();
            else if (iss is InventorySlotPersonal)//ce je personal inventorij al pa loadout k je blo to spisan ze prej
                this.npi.OnRightClickPersonalInventory(gameObject);
            else if (iss is InventorySlotBackpack)
                this.npi.OnRightClickBackpack(gameObject);
            else if (iss is InventorySlotBar)
                this.npi.OnRightClickBar(gameObject);
            else if (iss is InventorySlotLoadout)
                this.npi.OnRightClickLoadout(gameObject);
            else if (iss is InventorySlotContainer)
                this.npi.OnRightClickContainer(gameObject);

        }
    }
}
