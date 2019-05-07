using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotLoadout : InventorySlot
{
    public Item.Type type; // tip slota na loadoutu



    private bool is_upgrade(Item newItem)//tole nj bi vrnil odgovor ce je item upgrade. tko k u apexu k zamenja, tam je precej straightforward
    {
        return true;
    }



}
