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

        if (GetComponent<InventorySlotLoadout>().type != Item.Type.backpack)
            if (RectTransformUtility.RectangleContainsScreenPoint(invSlot, Input.mousePosition))//smo dropal nekam na valid inventorij plac
            {
                //Debug.Log(networkPlayerInventory.draggedItemParent.name+"'s child was dropped on " + invSlot.name+" ");

                //koda se bo malo podvajala ker bi sicer biu prevelik clusterfuck od metode

                networkPlayerInventory.handleInventorySlotOnDragDropEvent(invSlot, null, false);
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
            }
        else// backpack. drop that shit
        { networkPlayerInventory.backpackSpot.GetComponentInChildren<NetworkBackpack>().local_player_unequip_request(); }
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
        if (eventData.button == PointerEventData.InputButton.Right && iss.GetItem() != null)
        {
            if(iss.GetItem().type==Item.Type.backpack)
                networkPlayerInventory.backpackSpot.GetComponentInChildren<NetworkBackpack>().local_player_unequip_request();
            else
                networkPlayerInventory.OnRightClick(gameObject);
        }
    }
}
