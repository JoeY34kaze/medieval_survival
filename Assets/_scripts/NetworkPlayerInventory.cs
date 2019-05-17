using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

public class NetworkPlayerInventory : NetworkPlayerInventoryBehavior
{
    public Transform draggedItemParent = null;//mrde bolsa resitev obstaja ker nemaram statikov uporablat ampak lej. dela
    public int draggedParent_sibling_index = -1;

    public int space = 20; // kao space inventorija
    public Item[] items = new Item[20]; // seznam itemov, ubistvu inventorij
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public GameObject panel_inventory; //celotna panela za inventorij, to se izrise ko prtisnes "i"
    public Transform[] panel_personalInventorySlots;
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

    private Camera c;




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
                {
                    weapon_0 = i;
                    r = null;
                }
                else if (weapon_1 == null)
                {
                    weapon_1 = i;
                    r = null;
                }
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

    internal void handleItemPickup(Item item, int quantity)
    {
        Item resp=try_to_upgrade_loadout(item);
        if (resp != null)
            AddFirst(resp, quantity);
        else
        {//vse updejtat ker ta funkcija negre cez uno tavelko...


            NetworkPlayerCombatHandler n = GetComponent<NetworkPlayerCombatHandler>();
            n.update_equipped_weapons();//tole bo treba v delegata
            n.setCurrentWeaponToFirstNotEmpty();

            sendNetworkUpdate(false, true);//samo loadout poslemo
        }
    }

    internal void OnRightClick(GameObject g)//tole lahko potem pri ciscenju kode z malo preurejanja damo v uno tavelko metodo
    {
        handleInventorySlotOnDragDropEvent(null, g.transform, true);
    }

    internal bool hasInventorySpace()
    {
        foreach (Item i in this.items)
            if (i == null)
                return true;
        return false;
    }

    public bool SetLoadoutItem(Item i, int index)
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
                return false;
        }
        return true;
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

    protected override void NetworkStart()
    {
        base.NetworkStart();

        if (networkObject.IsOwner)
        {
            onItemChangedCallback += UpdateUI;    // Subscribe to the onItemChanged callback
            slots = new InventorySlotPersonal[panel_personalInventorySlots.Length];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = panel_personalInventorySlots[i].GetComponent<InventorySlotPersonal>();
            items = new Item[slots.Length];
        }
        else if (networkObject.IsServer)
        {
            slots = new InventorySlotPersonal[panel_personalInventorySlots.Length];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = panel_personalInventorySlots[i].GetComponent<InventorySlotPersonal>();
            items = new Item[slots.Length];
        }
        else
        {
            //prazno da nemore vidt esp hack - zaenkrat se tud tle nrdi ker nekej buga sicer.
            slots = new InventorySlotPersonal[panel_personalInventorySlots.Length];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = panel_personalInventorySlots[i].GetComponent<InventorySlotPersonal>();
            items = new Item[slots.Length];
        }

    }


    void Update()
    {

        if (networkObject == null) {
            Debug.LogError("networkObject is null.");
            return;
        }
        if (!networkObject.IsOwner) return;
        // Check to see if we should open/close the inventory
        //Debug.Log("items - " + this.items.Length);
        if (this.items.Length == 0) this.items = new Item[20];//hacky bug fix. makes me sick about this brah. mrde dat u onNetworkConnected al pa kej

        if (Input.GetButtonDown("Inventory"))
        {
        panel_inventory.SetActive(!panel_inventory.activeSelf);
            if (onItemChangedCallback != null)
                onItemChangedCallback.Invoke();
            else
                Debug.LogError("onItemChangedCallback je null.");

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

    internal int GetRanged()
    {
        if (this.ranged == null) return 0;//unarmed
        return this.ranged.id;
    }

    internal int GetWeapon1()
    {
        if (this.weapon_1 == null) return 0;//unarmed
        return this.weapon_1.id;
    }

    internal int GetShield()
    {
        if (this.shield == null) return 1;//unarmed
        return this.shield.id;
    }

    internal int GetWeapon0()
    {
        if (this.weapon_0 == null) return 0;//unarmed
        return this.weapon_0.id;
    }

    public void addToPersonalInventory(Item item, int quantity, int index) {
        if (index < 0)
        {//dodaj na prvo mesto ki je null
            addToPersonalInventoryFirstEmpty(item);
        }
        else {//dodaj na mesto oznaceno z index ce ni polno, sicer na prvo prazno mesto
            if (items[index] != null) addToPersonalInventoryFirstEmpty(item);
            else items[index] = item;
        }
    }

    private void addToPersonalInventoryFirstEmpty(Item item)
    {
        for (int i = 0; i < items.Length; i++) {
            if (items[i] == null) {
                items[i] = item;
                return;
            }
        }
    }

    private void removePersonalInventoryItem(int index)
    {
        if (index > -1 || index < items.Length)
            items[index] = null;
    }

    private void removePersonalInventoryItem(Item i, int index) {
        if (index == -1)
        {
            removePersonalInventoryItemFirstMatch(i);
        }
        else {
            items[index] = null;
        }
    }

    private void removePersonalInventoryItemFirstMatch(Item it)
    {
        for (int i = 0; i < items.Length; i++) {
            if (items[i].Equals(it)) {
                items[i] = null;
                return;
            }
        }
    }

    public void Add(Item item, int quantity, int index)
    {
        if (!networkObject.IsOwner) return;
        addToPersonalInventory(item,quantity,index);//nekej bo treba nrdit za hranjenje kolicine. recimo kamen pa take fore
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void AddFirst(Item item, int quantity)
    {
        if (!networkObject.IsOwner) return;
        addToPersonalInventory(item, quantity, -1);//nekej bo treba nrdit za hranjenje kolicine. recimo kamen pa take fore
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
    public void RemoveFirst(Item item)
    {
        if (!networkObject.IsOwner) return;
        removePersonalInventoryItem(item, -1);
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void Remove(int inventory_slot) // to se klice z slota na OnDrop eventu ko vrzemo item iz inventorija
    {
        if (!networkObject.IsOwner) return;
        removePersonalInventoryItem(inventory_slot);
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    public void DropItemFromPersonalInventory(int inventory_slot) {//isto k remove item samo da vrze item v svet.
        if (!networkObject.IsOwner) return;
        Item i = slots[inventory_slot].GetItem();
        removePersonalInventoryItem(inventory_slot);
        instantiateDroppedItem(i,1);

        //rpc update
        sendNetworkUpdate(true, false);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

    internal void DropItemFromLoadout(Item.Type type, int index)
    {
        if (!networkObject.IsOwner) return;
        Item i = PopLoadoutItem(type, index);
        instantiateDroppedItem(i,1);

        //rpc update
        sendNetworkUpdate(false, true);

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
        if (!networkObject.IsOwner) return;
        //personal inventory
        for (int i = 0; i < slots.Length; i++)
        {
            if (items[i]!=null)  // If there is an item to add
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




    /// <summary>
    /// klice se ko iz inventorija dropamo item na loadout panel
    /// startingParent je inventorij, target je loadout
    /// </summary>
    /// <param name="startingParent"></param>
    /// <param name="targetParent"></param>
    private void InventoryToLoadout(RectTransform loa, bool rightClick)//
    {

        int loadout_index = 0;
        if (!rightClick)
        {
            loadout_index = getIndexFromName(loa.name);
        }
        else {//right click- loa je null
            if (weapon_0 != null) loadout_index = 1;
        }


        Item loadout_item = null;
        loadout_item = PopLoadoutItem(this.draggedItemParent.GetComponent<InventorySlotPersonal>().GetItem().type, loadout_index);

        int inv_index = getIndexFromName(this.draggedItemParent.name);
        Item inventory_item = this.items[inv_index];//poisce item glede na id-ju slota. id dobi iz imena tega starsa.


        if (SetLoadoutItem(inventory_item, loadout_index))//to bo zmer slo cez ker je slot ze prazen. smo ga izpraznli z popom. vrne true ce je item biu valid za nek loadout slot.
            Remove(inv_index);
        if (loadout_item != null)
        {//loadout ni bil prazen prej tko da rabmo item dat v inventorij
            Add(loadout_item, 1, inv_index);
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
    internal void handleInventorySlotOnDragDropEvent(RectTransform invSlot, Transform parent, bool rightClick)
    {
        if (parent == null)
            parent = this.draggedItemParent;
        this.draggedItemParent = parent.GetComponent<RectTransform>();

        InventorySlot from = parent.GetComponent<InventorySlot>();

        if (invSlot != null)
        {
            InventorySlot to = invSlot.GetComponent<InventorySlot>();

            if (!to.Equals(from))
            {
                if (invSlot.GetComponent<InventorySlot>() is InventorySlotLoadout && from is InventorySlotPersonal)
                {
                    //Debug.Log("Premikamo iz inventorija v loadout");
                    InventoryToLoadout(invSlot, false);
                }
                else if (invSlot.GetComponent<InventorySlot>() is InventorySlotPersonal && from is InventorySlotLoadout)
                {
                    LoadoutToInventory(invSlot, false);
                    //Debug.Log("Premikamo iz loadouta v inventorij");
                }
                else if (invSlot.GetComponent<InventorySlot>() is InventorySlotPersonal && from is InventorySlotPersonal)
                {
                    //Debug.Log("Premikamo item znotraj inventorija");
                    InventoryToInventory(invSlot);
                }
                else if (invSlot.GetComponent<InventorySlot>() is InventorySlotLoadout && from is InventorySlotLoadout)
                {
                    //Debug.Log("Premikamo item znotraj loadouta.");
                    LoadoutToLoadout(invSlot);
                }
            }
        }
        else {//right click event
            if (from is InventorySlotPersonal)
            {
                InventoryToLoadout(null, true);
            }
            else if (from is InventorySlotLoadout) {
                LoadoutToInventory(null,true);
            }
        }
        this.draggedItemParent = null;//tole nastavmo tud na drag handlerju just in case

        GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();//tole bo treba v delegata

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();




        sendNetworkUpdate(true,true);


    }

    private void sendNetworkUpdate(bool inv, bool loadout)
    {
        
        if (inv)
        {
            short i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15, i16, i17, i18, i19;



            if (this.items[0] == null) i0 = -1;
            else i0 = (short)this.items[0].id;

            if (this.items[1] == null) i1 = -1;
            else i1 = (short)this.items[1].id;

            if (this.items[2] == null) i2 = -1;
            else i2 = (short)this.items[2].id;

            if (this.items[3] == null) i3 = -1;
            else i3 = (short)this.items[3].id;

            if (this.items[4] == null) i4 = -1;
            else i4 = (short)this.items[4].id;

            if (this.items[5] == null) i5 = -1;
            else i5 = (short)this.items[5].id;

            if (this.items[6] == null) i6 = -1;
            else i6 = (short)this.items[6].id;

            if (this.items[7] == null) i7 = -1;
            else i7 = (short)this.items[7].id;

            if (this.items[8] == null) i8 = -1;
            else i8 = (short)this.items[8].id;

            if (this.items[9] == null) i9 = -1;
            else i9 = (short)this.items[9].id;

            if (this.items[10] == null) i10 = -1;
            else i10 = (short)this.items[10].id;

            if (this.items[11] == null) i11 = -1;
            else i11 = (short)this.items[11].id;

            if (this.items[12] == null) i12 = -1;
            else i12 = (short)this.items[12].id;

            if (this.items[13] == null) i13 = -1;
            else i13 = (short)this.items[13].id;

            if (this.items[14] == null) i14 = -1;
            else i14 = (short)this.items[14].id;

            if (this.items[15] == null) i15 = -1;
            else i15 = (short)this.items[15].id;

            if (this.items[16] == null) i16 = -1;
            else i16 = (short)this.items[16].id;

            if (this.items[17] == null) i17 = -1;
            else i17 = (short)this.items[17].id;

            if (this.items[18] == null) i18 = -1;
            else i18 = (short)this.items[18].id;

            if (this.items[19] == null) i19 = -1;
            else i19 = (short)this.items[19].id;

            networkObject.SendRpc(RPC_SEND_PERSONAL_INVENTORY_UPDATE, Receivers.Server,
                i0,
                i1,
                i2,
                i3,
                i4,
                i5,
                i6,
                i7,
                i8,
                i9,
                i10,
                i11,
                i12,
                i13,
                i14,
                i15,
                i16,
                i17,
                i18,
                i19);
        }

        if (loadout)
        {
            short l0 = -1, l1 = -1, l2 = -1, l3 = -1, l4 = -1, l5 = -1, l6 = -1, l7 = -1, l8 = -1;

            if (this.head != null) l0 = (short)this.head.id;
            if (this.chest != null) l1 = (short)this.chest.id;
            if (this.hands != null) l2 = (short)this.hands.id;
            if (this.legs != null) l3 = (short)this.legs.id;
            if (this.feet != null) l4 = (short)this.feet.id;

            if (this.ranged != null) l5 = (short)this.ranged.id;
            if (this.weapon_0 != null) l6 = (short)this.weapon_0.id;
            if (this.weapon_1 != null) l7 = (short)this.weapon_1.id;
            if (this.shield != null) l8 = (short)this.shield.id;


            networkObject.SendRpc(RPC_SEND_LOADOUT_UPDATE, Receivers.OthersProximity,
                l0, l1, l2, l3, l4, l5, l6, l7, l8
                );
        }
    }

    /// <summary>
    /// menjamo pozicije itemov znotraj inventorija. ubistvu ni nekih komplikacij.
    /// </summary>
    /// <param name="invSlot"></param>
    private void InventoryToInventory(RectTransform invSlot)
    {
        int index1 = getIndexFromName(invSlot.name);
        int index2 = getIndexFromName(this.draggedItemParent.name);
        Item temp = items[index1];
        this.items[index1] = this.items[index2];
        this.items[index2] = temp;
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

    private void LoadoutToInventory(RectTransform invSlot, bool rightClick)
    {
        int index = -1;

        if (!rightClick) index = getIndexFromName(invSlot.name);
        Item.Type t = this.draggedItemParent.GetComponent<InventorySlotLoadout>().type;

        Item loadout_item = null;
        int loadout_index = getIndexFromName(this.draggedItemParent.name);
        loadout_item = PopLoadoutItem(t, loadout_index);
        if (loadout_item == null) {
            Debug.LogError("dragged loadout item is null. this is not possible.");
            return;
        }

        if (index != -1) {//za right click
            if (this.items[index] != null)//ce smo potegnil na item k ze obstaja.
            {
                if (t == this.items[index].type)//ce se item ujema naj se zamenja
                {

                    Item inventory_item = this.items[index];
                    if (loadout_item != null)
                    {
                        Add(loadout_item, 1, index);
                        SetLoadoutItem(inventory_item, loadout_index);
                    }
                }
                else if (hasInventorySpace()) //ce se ne ujema ga mormo dodat na prvo prazno mesto v inventoriju
                {
                    //da rabmo item dat v inventorij
                    AddFirst(loadout_item, 1);
                }
                else
                {
                    Debug.Log("No Space in inventory and cannot switch!");
                }
            }
            else {//dodaj na ta slot.
                Add(loadout_item, 1, index);
            }
        }
        else if (hasInventorySpace())
        {
            //da rabmo item dat v inventorij
            if (!rightClick)
                if (this.items[index] == null)
                    Add(loadout_item, 1, index);
                else
                    AddFirst(loadout_item, 1);
            else AddFirst(loadout_item, 1);
        }
        else
        {
            Debug.Log("No Space in inventory and cannot switch!");
        }
    }


    private int getFreePersonalInventorySpace()
    {
        int cunt = 0;
        foreach (Item i in items)
            if (i == null)
                cunt++;
        return cunt;
    }

    internal void instantiateDroppedItem(Item item, int quantity) // instantiate it when dropped
    {
        if(c==null)c=GetComponentInChildren<Camera>();
        Interactable_objectBehavior b =NetworkManager.Instance.InstantiateInteractable_object(item.id-2, c.transform.position+(c.transform.forward*3));
        b.gameObject.GetComponent<Rigidbody>().AddForce(c.transform.forward*1500);
    }

    private int getIndexFromName(string name)
    {
        string[] a = name.Split('(');
        string[] b = a[a.Length - 1].Split(')');
        int n;
        if (int.TryParse(b[0], out n))
            return n;
        return -1;
    }

    public override void SendPersonalInventoryUpdate(RpcArgs args)
    {//nc zrihtan za kolicino
        if (!networkObject.IsServer) return;

        //server side checks for anticheat


        for (int i = 0; i < 20; i++)
        {
            short item_id = args.GetNext<short>();
            if(item_id>0)this.items[i] = Mapper.instance.getItemById((int)item_id);
        }


    }

    public override void SendLoadoutUpdate(RpcArgs args)
    {
        int i = (int)args.GetNext<short>();
        if(i>0)this.head = Mapper.instance.getItemById(i);

         i = (int)args.GetNext<short>();
        if (i > 0) this.chest = Mapper.instance.getItemById(i);

        i = (int)args.GetNext<short>();
        if (i > 0) this.hands = Mapper.instance.getItemById(i);

        i = (int)args.GetNext<short>();
        if (i > 0) this.legs = Mapper.instance.getItemById(i);

        i = (int)args.GetNext<short>();
        if (i > 0) this.feet = Mapper.instance.getItemById(i);

        i = (int)args.GetNext<short>();
        if (i > 0) this.ranged = Mapper.instance.getItemById(i);

        i = (int)args.GetNext<short>();
        if (i > 0) this.weapon_0 = Mapper.instance.getItemById(i);

        i = (int)args.GetNext<short>();
        if (i > 0) this.weapon_1 = Mapper.instance.getItemById(i);

        i = (int)args.GetNext<short>();
        if (i > 0) this.shield = Mapper.instance.getItemById(i);
    }
}
