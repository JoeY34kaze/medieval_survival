using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDropHandler : MonoBehaviour, IDropHandler
{
    private NetworkPlayerInventory networkPlayerInventory;

    private void Start()
    {
        networkPlayerInventory = transform.root.GetComponent<NetworkPlayerInventory>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        RectTransform invSlot = transform as RectTransform;

        if (RectTransformUtility.RectangleContainsScreenPoint(invSlot, Input.mousePosition))
        {
            Debug.Log(networkPlayerInventory.draggedItemParent.name+"'s child was dropped on " + invSlot.name+" ");
            tryToAddOrSwitchItems(networkPlayerInventory.draggedItemParent, invSlot);

        }
        else {

            Debug.Log("Called on " + gameObject.name);
            networkPlayerInventory.RemoveItem(getIndexFromName(invSlot.name));
        }
    }
    /// <summary>
    /// klice se na loadoutPanel in na inventory panel ko iz enga potegnes na druzga item. pohendlat mora da se itema zamenjata ce je na target panelu ze ksn item, sicer se samo doda gor in pobrise v inventoriju.
    /// starting parent je lahko ubistvu inventorij ali loadout, enako target. lahko sta oba loadout (switchanje weapononv) ali oba inventorij ( organizacija inventorija) it will get messy ker nimam inheritanca zrihtanga. tole pisem kot prototip in se bo potem koda pucala ce bo treba
    /// </summary>
    /// <param name="startingParent"></param>
    /// <param name="targetParent"></param>
    private void tryToAddOrSwitchItems(Transform startingParent, RectTransform targetParent)//
    {
        Item i = null;
        if (startingParent.GetComponent<loadoutSlot>() != null)
        {
            i = startingParent.GetComponent<loadoutSlot>().PopItem();//samo pop
        }
        else if (startingParent.GetComponent<InventorySlot>() != null) {
            i = startingParent.GetComponent<InventorySlot>().PopItem();
        }
        if (i == null) { Debug.LogError("Error when retrieving item for switch!"); return; }

        //na tem mestu smo ze odstranili item s pomocjo networkPlayerInventory skripte tko da so tud delegati pohendlan za item i, ki je bil na starting mestu.

        Item old = null;

        if (targetParent.GetComponent<loadoutSlot>() != null)
        {
            if (slotIsEligible())
                old = targetParent.GetComponent<loadoutSlot>().PopItem(i.type);// ce ni taprav slot moramo skenslat transakcijo
            else
                throw new NotImplementedException("Target slot is incompatible. Revert transaction!");
        }
        else if (targetParent.GetComponent<InventorySlot>() != null)
        {
            old = targetParent.GetComponent<InventorySlot>().PopItem();
        }

        //na tem mestu smo pohendlal odstranjevanje obeh itemov.  Oba imamo tukaj. i je ta k ga draggamo, old je ta k je na ciljni lokaciji in ga moramo zamenjat z pozicijo s katere je starting prisel

       
        if (targetParent.GetComponent<loadoutSlot>() != null)
        {
            networkPlayerInventory.try_to_upgrade_loadout(i);
        }
        else if (targetParent.GetComponent<InventorySlot>() != null)
        {
            networkPlayerInventory.Add(i,1);
        }


        //--------------ce je biu switch dejanski.
        if (old != null)
        {
            if (startingParent.GetComponent<loadoutSlot>() != null)
            {
                networkPlayerInventory.try_to_upgrade_loadout(i);

            }
            else if (startingParent.GetComponent<InventorySlot>() != null)
            {
                networkPlayerInventory.Add(old, 1);
            }
        }
        Debug.Log("switch complete..");
    }

    private bool slotIsEligible()
    {
        return true;
    }

    private Item GetSlotItem()//za cel tale class mrde inheritance? ker tole je mal loshe
    {
       Item ret = null;
        InventorySlot sl = GetComponent<InventorySlot>();
        if (sl != null) return sl.GetItem();
        loadoutSlot ls = GetComponent<loadoutSlot>();
        if (ls != null) return ls.GetItem();
        return null;
    }

    private int getIndexFromName(string name)
    {
        string[] a = name.Split('(');
        string[] b = a[a.Length - 1].Split(')');
        return Int32.Parse(b[0]);
    }
}
