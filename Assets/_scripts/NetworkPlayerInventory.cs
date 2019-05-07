using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;

public class NetworkPlayerInventory : NetworkPlayerInventoryBehavior
{
    public Transform draggedItemParent = null;//mrde bolsa resitev obstaja ker nemaram statikov uporablat ampak lej. dela

    public int space = 20; // kao space inventorija
    public List<Item> items = new List<Item>(); // seznam itemov, ubistvu inventorij
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public GameObject panel_inventory; //celotna panela za inventorij, to se izrise ko prtisnes "i"
    public GameObject inventorySlotsParent; // parent object od slotov da jih komot dobis v array
    InventorySlotPersonal[] slots;  // predstavlajo slote v inventoriju, vsak drzi en item. 




    //-------------------------------LOADOUT SLOTS-----------------------------
    public InventorySlotLoadout loadout_head;
    public InventorySlotLoadout loadout_chest;
    public InventorySlotLoadout loadout_hands;
    public InventorySlotLoadout loadout_legs;
    public InventorySlotLoadout loadout_feet;
    public InventorySlotLoadout loadout_ranged;
    public InventorySlotLoadout loadout_weapon_0;
    public InventorySlotLoadout loadout_weapon_1;
    public InventorySlotLoadout loadout_shield;

    private Item head;
    private Item chest;
    private Item hands;
    private Item legs;
    private Item feet;
    private Item ranged;
    private Item weapon_0;
    private Item weapon_1;
    private Item shield;


    /// <summary>
    /// proba upgrejdat loadout z itemom i. vrne item i ce ni upgrade. vrne item s katermu ga je zamenov ce je upgrade bil, vrne null ce je biu prazn slot prej
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public Item try_to_upgrade_loadout(Item i) {//vrne item s katermu smo ga zamenjal al pa null ce je biu prej prazn slot
        Item r = i;
        switch (i.type) {
            case Item.Type.head:
                if (compareGear(head, i)) {
                    r = head;
                    head = i;
                }
                break;
            case Item.Type.chest:
                if (compareGear(chest, i))
                {
                    r = chest;
                    chest = i;
                }
                break;
            case Item.Type.hands:
                if (compareGear(hands, i))
                {
                    r = hands;
                    hands = i;
                }
                break;
            case Item.Type.legs:
                if (compareGear(legs, i))
                {
                    r = legs;
                    legs = i;
                }
                break;
            case Item.Type.feet:
                if (compareGear(feet, i))
                {
                    r = feet;
                    feet = i;
                }
                break;
            case Item.Type.ranged:
                if (compareGear(ranged, i))
                {
                    r = ranged;
                    ranged = i;
                }
                break;
            case Item.Type.weapon://tole se nobe zmer zamenjal ker tega nocmo. equipa nj se samo ce je slot prazen
                if (weapon_0 == null)
                    weapon_0 = i;
                else if (weapon_1 == null)
                    weapon_1 = i;
                    break;
            case Item.Type.shield:
                if (compareGear(shield, i))
                {
                    r = shield;
                    shield = i;
                }
                break;
            default:
                r = null;
                break;
        }
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
        return r;
    }

    public void SetLoadoutItem(Item i, int index)
    {//vrne item s katermu smo ga zamenjal al pa null ce je biu prej prazn slot
        Item r = i;
        switch (i.type)
        {
            case Item.Type.head:
                    head = i;
                break;
            case Item.Type.chest:
                    chest = i;
                break;
            case Item.Type.hands:
                    hands = i;
                break;
            case Item.Type.legs:
                    legs = i;
                break;
            case Item.Type.feet:
                    feet = i;
                break;
            case Item.Type.ranged:
                    ranged = i;
                break;
            case Item.Type.weapon://tole se nobe zmer zamenjal ker tega nocmo. equipa nj se samo ce je slot prazen
                if (index == 0)
                    weapon_0 = i;
                else
                    weapon_1 = i;
                break;
            case Item.Type.shield:
                    shield = i;
                break;
            default:
                break;
        }
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
    /// <summary>
    /// funkcija vrne true ce je item upgrade zdejsnjemu gearu in ga doda direkt na playerjev loadout tukej nj bi sla vsa tista logika k je treba. zaenkrat smao pogleda ce je null in ga doda not ce je null
    /// </summary>
    /// <param name="x"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    private bool compareGear(Item x, Item i)
    {
        if (x == null) return true;
        else return false;
    }

    public void RemoveItemLoadout(Item.Type t, int index) {
        switch (t)
        {
            case Item.Type.head:
                head = null;
                break;
            case Item.Type.chest:
                chest = null;
                break;
            case Item.Type.hands:
                hands = null;
                break;
            case Item.Type.legs:
                legs = null;
                break;
            case Item.Type.feet:
                feet = null;
                break;
            case Item.Type.ranged:
                ranged = null;
                break;
            case Item.Type.weapon://tole se nobe zmer zamenjal ker tega nocmo. equipa nj se samo ce je slot prazen
                if (index == 0)
                    weapon_0 = null;
                else
                    weapon_1 = null;
                break;
            case Item.Type.shield:
                shield = null;
                break;
            default:
                Debug.LogError("Item type doesnt match anything. shits fucked yo");
                break;
        }
    }

    private void Start()
    {
        if (!networkObject.IsOwner) return;
        onItemChangedCallback += UpdateUI;    // Subscribe to the onItemChanged callback
        slots = inventorySlotsParent.GetComponentsInChildren<InventorySlotPersonal>();
    }


    void Update()
    {

        if (networkObject == null) return;
        if (!networkObject.IsOwner) return;
        // Check to see if we should open/close the inventory
        if (Input.GetButtonDown("Inventory"))
        {
        panel_inventory.SetActive(!panel_inventory.activeSelf);
            if (onItemChangedCallback != null)
                onItemChangedCallback.Invoke();
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
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void DropItem(int inventory_slot) {//isto k remove item samo da vrze item v svet.
        if (!networkObject.IsOwner) return;

        Item i = slots[inventory_slot].GetItem();
        items.Remove(i);
        instantiateDroppedItem(i);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    // Update the inventory UI by:
    //		- Adding items
    //		- Clearing empty slots
    //      - upgrading loadout items
    // This is called using a delegate on the Inventory.
    void UpdateUI()
    {

        if(slots==null) slots = inventorySlotsParent.GetComponentsInChildren<InventorySlotPersonal>();
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

        //update loadout. hardcoded pain lol

        if (this.head != null)
            loadout_head.AddItem(this.head);
        else
            loadout_head.ClearSlot();

        if (this.chest != null)
            loadout_chest.AddItem(this.chest);
        else
            loadout_head.ClearSlot();

        if (this.hands != null)
            loadout_chest.AddItem(this.hands);
        else
            loadout_hands.ClearSlot();

        if (this.legs != null)
            loadout_head.AddItem(this.legs);
        else
            loadout_legs.ClearSlot();

        if (this.feet != null)
            loadout_head.AddItem(this.feet);
        else
            loadout_feet.ClearSlot();

        if (this.ranged != null)
            loadout_ranged.AddItem(this.ranged);
        else
            loadout_ranged.ClearSlot();

        if (this.weapon_0 != null)
            loadout_weapon_0.AddItem(this.weapon_0);
        else
            loadout_weapon_0.ClearSlot();

        if (this.weapon_1 != null)
            loadout_weapon_1.AddItem(this.weapon_1);
        else
            loadout_weapon_1.ClearSlot();

        if (this.shield != null)
            loadout_shield.AddItem(this.shield);
        else
            loadout_shield.ClearSlot();
    }


    public void RemoveItem(int inventory_slot) // to se klice z slota na OnDrop eventu ko vrzemo item iz inventorija
    {
        Item itemToRemove = slots[inventory_slot].GetItem();
        this.Remove(itemToRemove);
    }

    /// <summary>
    /// klice se ko iz inventorija dropamo item na loadout panel
    /// startingParent je inventorij, target je loadout
    /// </summary>
    /// <param name="startingParent"></param>
    /// <param name="targetParent"></param>
    private void InventoryToLoadout(RectTransform loa)//
    {
        int loadout_index = getIndexFromName(loa.name);
        Item loadout_item = null;
        loadout_item = PopLoadoutItem(this.draggedItemParent.GetComponent<InventorySlotPersonal>().GetItem().type, loadout_index);

        int inv_index = getIndexFromName(this.draggedItemParent.name);
        Item inventory_item = this.items[inv_index];//poisce item glede na id-ju slota. id dobi iz imena tega starsa.


        SetLoadoutItem(inventory_item, loadout_index);//to bo zmer slo cez ker je slot ze prazen. smo ga izpraznli z popom
        RemoveItem(inv_index);
        if (loadout_item != null)
        {//loadout ni bil prazen prej tko da rabmo item dat v inventorij
            Add(loadout_item,1);
        }

    }

    private Item PopLoadoutItem(Item.Type t,int index)
    {
        Item i = null;
        switch (t)
        {
            case Item.Type.head:
                i = head;
                head = null;
                break; ;
            case Item.Type.chest:
                i=chest;
                chest = null;
                break;
            case Item.Type.hands:
                i =hands;
                hands = null;
                break;
            case Item.Type.legs:
                i= legs;
                legs = null;
                break;
            case Item.Type.feet:
                i= feet;
                feet = null;
                break;
            case Item.Type.ranged:
                i= ranged;
                ranged = null;
                break;
            case Item.Type.weapon:
                if (index == 0)
                {
                    i = weapon_0;
                    weapon_0 = null;
                }
                else
                {
                    i = weapon_1;
                    weapon_1 = null;
                }
                break;
            case Item.Type.shield:
                i= shield;
                shield = null;
                break;
            default:
                Debug.LogError("Item type doesnt match anything. shits fucked yo");
                break;
        }
        return i;
    }

    /// <summary>
    /// ko je drop event panela klice to metodo. vse se handla potem tukaj v tej skripti.
    /// </summary>
    /// <param name="invSlot"></param>
    internal void handleInventorySlotOnDragDropEvent(RectTransform invSlot)
    {
        if (invSlot.GetComponent<InventorySlot>() is InventorySlotLoadout && this.draggedItemParent.GetComponent<InventorySlot>() is InventorySlotPersonal)
        {
            Debug.Log("Premikamo iz inventorija v loadout");
            InventoryToLoadout(invSlot);
        }
        else if (invSlot.GetComponent<InventorySlot>() is InventorySlotPersonal && this.draggedItemParent.GetComponent<InventorySlot>() is InventorySlotLoadout)
        {
            LoadoutToInventory(invSlot);
            Debug.Log("Premikamo iz loadouta v inventorij");
        }
        else if (invSlot.GetComponent<InventorySlot>() is InventorySlotPersonal && this.draggedItemParent.GetComponent<InventorySlot>() is InventorySlotPersonal)
        {
            Debug.Log("Premikamo item znotraj inventorija");
        }
        else if (invSlot.GetComponent<InventorySlot>() is InventorySlotLoadout && this.draggedItemParent.GetComponent<InventorySlot>() is InventorySlotLoadout)
        {
            Debug.Log("Premikamo item znotraj loadouta.");
            LoadoutToLoadout(invSlot);
        }
        this.draggedItemParent = null;

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    /// <summary>
    /// edina stvar ki jo lahko menja sta weapona tko da tukaj nebo kompliciranja
    /// </summary>
    /// <param name="invSlot"></param>
    private void LoadoutToLoadout(RectTransform invSlot)
    {
            Item temp = weapon_1;
            weapon_1 = weapon_0;
            weapon_0 = temp;
    }

    private void LoadoutToInventory(RectTransform invSlot)
    {
        if (this.items.Count < this.inventorySlotsParent.transform.childCount)
        {
            Item.Type t = this.draggedItemParent.GetComponent<InventorySlotLoadout>().type;
            Item loadout_item = null;
            loadout_item = PopLoadoutItem(t,getIndexFromName(this.draggedItemParent.name));

            if (loadout_item != null)
            {//da rabmo item dat v inventorij
                Add(loadout_item, 1);
            }
        }
        else
        {
            Debug.Log("No Space in inventory!");
        }
    }

    private void instantiateDroppedItem(Item item) // instantiate it when dropped
    {
        NetworkManager.Instance.InstantiateInteractable_object(0, transform.position + transform.forward);
    }

    private int getIndexFromName(string name)
    {
        string[] a = name.Split('(');
        string[] b = a[a.Length - 1].Split(')');
        return Int32.Parse(b[0]);
    }
}
