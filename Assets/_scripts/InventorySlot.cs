using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InventorySlot : MonoBehaviour
{
    public Image icon;          // Reference to the Icon image
    protected Item item;  // Current item in the slot
    protected Image icon_background;          // Reference to the Icon image to display when empty
    public int index;

    private void Start()
    {
        icon_background = GetComponentInChildren<Image>();
    }

    public void AddItem(Item newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public Item GetItem()
    {
        return this.item;
    }

    // Clear the slot
    public void ClearSlot()
    {
        item = null;
        icon.sprite = icon_background.sprite;
        icon.enabled = true;
    }

    public abstract Item PopItem();
}
