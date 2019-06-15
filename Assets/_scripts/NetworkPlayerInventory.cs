using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
using UMA;
using UMA.CharacterSystem;

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


    private DynamicCharacterAvatar avatar;

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

    public delegate void OnLoadoutChanged();
    public OnLoadoutChanged onLoadoutChangedCallback;


    private Item ranged;
    private Item weapon_0;
    private Item weapon_1;
    private Item shield;


    private Camera c;
    public Transform backpackSpot;

    private void Start()
    {
        this.avatar = GetComponent<DynamicCharacterAvatar>();

        slots = new InventorySlotPersonal[panel_personalInventorySlots.Length];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = panel_personalInventorySlots[i].GetComponent<InventorySlotPersonal>();
        items = new Item[slots.Length];
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();

        if (networkObject.IsOwner)
        {
            onItemChangedCallback += UpdateUI;    // Subscribe to the onItemChanged callback
        }

        onLoadoutChangedCallback += refresh_UMA_equipped_gear;//vsi rabjo vidt loadout

        networkObject.SendRpc(RPC_REQUEST_LOADOUT_ON_CONNECT, Receivers.Server);//owner poslje na server. server poslje vsem networkUpdate. pomoje lahko ponucamo samo en rpc za to ker so razlicni naslovniki
    }


    void refresh_UMA_equipped_gear()
    {
        avatar.ClearSlots();

        if (this.head != null)
        {
            avatar.SetSlot("Helmet", this.head.recipeName);
        }

        if (this.chest != null)
        {
            avatar.SetSlot("Chest", this.chest.recipeName);
        }

        if (this.hands != null)
        {
            avatar.SetSlot("Hands", this.hands.recipeName);
        }

        if (this.legs != null)
        {
            avatar.SetSlot("Legs", this.legs.recipeName);
        }

        if (this.feet != null)
        {
            avatar.SetSlot("Feet", this.feet.recipeName);
        }

        avatar.BuildCharacter();
    }

    /// <summary>
    /// proba upgrejdat loadout z itemom i. vrne item i ce ni upgrade. vrne item s katermu ga je zamenov ce je upgrade bil, vrne null ce je biu prazn slot prej
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public Item try_to_upgrade_loadout(Item i) {//vrne item s katermu smo ga zamenjal al pa null ce je biu prej prazn slot
        if (!networkObject.IsServer) { Debug.LogError("client se ukvarja z metodo k je server designated.."); return null; }
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
        if (onItemChangedCallback != null  && networkObject.IsServer)//ker se nkol ne izvede na clientu ta metoda in server itak nebo nkol vidu inventorija od drugih na ekranu.. probably
            onItemChangedCallback.Invoke();
        return r;
    }

    internal Item getHeadItem()
    {
        return this.head;
    }
    internal Item getChestItem()
    {
        return this.chest;
    }
    internal Item getHandsItem()
    {
        return this.hands;
    }
    internal Item getLegsItem()
    {
        return this.legs;
    }
    internal Item getFeetItem()
    {
        return this.feet;
    }

    internal Item getWeapon_0Item()
    {
        return this.weapon_0;
    }

    internal Item getWeapon_1Item()
    {
        return this.weapon_1;
    }
    internal Item getShieldItem()
    {
        return this.shield;
    }
    internal Item getRangedItem()
    {
        return this.ranged;
    }

    internal void handleItemPickup(Item item, int quantity)
    {
        if (!networkObject.IsServer) {
            Debug.LogError("client se ukvarja z inventorijem, to se mora samo server.");
            return;
        }

        Item resp = try_to_upgrade_loadout(item);
        if (resp != null)
            AddFirst(resp, quantity);
        else
        {//vse updejtat ker ta funkcija negre cez uno tavelko...


            NetworkPlayerCombatHandler n = GetComponent<NetworkPlayerCombatHandler>();
            //za weapone treba ksnej poskrbet da je server authoritative. rework pending
            n.update_equipped_weapons();
            n.setCurrentWeaponToFirstNotEmpty();//poslat rpc ?yes
        }
        sendNetworkUpdate(true, true);//posljemo obojeee optimizacija later
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
    {//nevem zakaj vrne bool
        if (!networkObject.IsServer) { Debug.LogError("client dela stvar od serevrja!"); return false;}
        if (i == null) return false;

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

    public Item PopItemLoadout(Item.Type t, int index)
    {
        Item ret = null;
        switch (t)
        {
            case Item.Type.head:
                ret = head;
                head = null;
                break;
            case Item.Type.chest:
                ret = chest;
                chest = null;
                break;
            case Item.Type.hands:
                ret = hands;
                hands = null;
                break;
            case Item.Type.legs:
                ret = legs;
                legs = null;
                break;
            case Item.Type.feet:
                ret = feet;
                feet = null;
                break;
            case Item.Type.ranged:
                ret = ranged;
                ranged = null;
                break;
            case Item.Type.weapon://tole se nobe zmer zamenjal ker tega nocmo. equipa nj se samo ce je slot prazen
                if (index == 0)
                {
                    ret = weapon_0;
                    weapon_0 = null;
                }
                else
                {
                    ret = weapon_1;
                    weapon_1 = null;
                }
                break;
            case Item.Type.shield:
                ret = shield;
                shield = null;
                break;
            default:
                Debug.LogError("Item type doesnt match anything. shits fucked yo");
                break;
        }
        return ret;
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
        if (!networkObject.IsServer) return;
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
        if (!networkObject.IsServer) return;
        for (int i = 0; i < items.Length; i++) {
            if (items[i] == null) {
                items[i] = item;
                return;
            }
        }
    }

    private void removePersonalInventoryItem(int index)
    {
        if (!networkObject.IsServer) return;
        if (index > -1 || index < items.Length)
            items[index] = null;
    }

    private void removePersonalInventoryItem(Item i, int index) {
        if (!networkObject.IsServer) return;
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
        if (!networkObject.IsServer) return;
        for (int i = 0; i < items.Length; i++) {
            if (items[i].Equals(it)) {
                items[i] = null;
                return;
            }
        }
    }

    public void Add(Item item, int quantity, int index)
    {
        if (!networkObject.IsServer) return;
        addToPersonalInventory(item,quantity,index);//nekej bo treba nrdit za hranjenje kolicine. recimo kamen pa take fore
    }

    public void AddFirst(Item item, int quantity)
    {
        if (!networkObject.IsServer) return;
        addToPersonalInventory(item, quantity, -1);//nekej bo treba nrdit za hranjenje kolicine. recimo kamen pa take fore
    }
    public void RemoveFirst(Item item)
    {
        if (!networkObject.IsServer) return;
        removePersonalInventoryItem(item, -1);

    }

    public void Remove(int inventory_slot) // to se klice z slota na OnDrop eventu ko vrzemo item iz inventorija
    {
        if (!networkObject.IsServer) return;
        removePersonalInventoryItem(inventory_slot);
    }

    public void DropItemFromPersonalInventory(int inventory_slot) {//isto k remove item samo da vrze item v svet.
        if (!networkObject.IsOwner) return;
        Camera c = Camera.main;
        
        networkObject.SendRpc(RPC_DROP_ITEM_FROM_PERSONAL_INVENTORY_REQUEST, Receivers.Server, inventory_slot, c.transform.position + (c.transform.forward * 3),c.transform.forward);


    }

    internal void DropItemFromLoadout(Item.Type type, int index)
    {
        if (!networkObject.IsOwner) return;
        Camera c = Camera.main;
        networkObject.SendRpc(RPC_DROP_ITEM_FROM_LOADOUT_REQUEST, Receivers.Server, type.ToString(), index, c.transform.position + (c.transform.forward * 3),c.transform.forward);
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
            loadout_chest.ClearSlot();

        if (this.hands != null)
            loadout_hands.AddItem(this.hands);
        else
            loadout_hands.ClearSlot();

        if (this.legs != null)
            loadout_legs.AddItem(this.legs);
        else
            loadout_legs.ClearSlot();

        if (this.feet != null)
            loadout_feet.AddItem(this.feet);
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
        if (!networkObject.IsOwner) return;
        int loadout_index = 0;
        if (!rightClick)
        {
            loadout_index = getIndexFromName(loa.name);
        }
        else {//right click- loa je null
            if (weapon_0 != null) loadout_index = 1;
        }
        networkObject.SendRpc(RPC_INVENTORY_TO_LOADOUT_REQUEST, Receivers.Server, loadout_index, this.draggedItemParent.GetComponent<InventorySlotPersonal>().GetItem().type.ToString(), getIndexFromName(this.draggedItemParent.name));


    }

    private Item PopLoadoutItem(Item.Type t,int index)
    {
        if (!networkObject.IsServer) { Debug.LogError("client probava delat stvar k je sam na serverju.."); return null; }

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
        if (!networkObject.IsOwner) return;
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

        //GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();// je tole client side?? bomo spoznal pr reworku combata..

    }

    public void sendNetworkUpdate(bool inv, bool loadout) //LOADOUT JE SAMO ZA UMA OBLEKE!!!!!!
    {
        if (!networkObject.IsServer) { Debug.LogError("client poskusa posiljat networkupdate k je samo od serverja.."); return; }
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

            //poslat ownerju

            Debug.Log(" personal inventory rpc SEND: owner server id: " + GetComponent<NetworkPlayerStats>().server_id + " | networkId : " + networkObject.Owner.NetworkId);
            networkObject.SendRpc(RPC_SEND_PERSONAL_INVENTORY_UPDATE,Receivers.Owner,
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
            
            if (this.ranged != null) l5 = (short)this.ranged.id;//NE DELA - BO TREBA UPDEJTAT. ZAENKRAT SE UPORABLA GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();   KER JE BLO ZE PREJ IMPLEMENTIRAN!!
            if (this.weapon_0 != null) l6 = (short)this.weapon_0.id;//NE DELA - BO TREBA UPDEJTAT. ZAENKRAT SE UPORABLA GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();   KER JE BLO ZE PREJ IMPLEMENTIRAN!!
            if (this.weapon_1 != null) l7 = (short)this.weapon_1.id;//NE DELA - BO TREBA UPDEJTAT. ZAENKRAT SE UPORABLA GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();   KER JE BLO ZE PREJ IMPLEMENTIRAN!!
            if (this.shield != null) l8 = (short)this.shield.id;//NE DELA - BO TREBA UPDEJTAT. ZAENKRAT SE UPORABLA GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();   KER JE BLO ZE PREJ IMPLEMENTIRAN!!
            


           // GetComponent<NetworkPlayerCombatHandler>().send_network_update_weapons();//weapon trenutno equipan pa shield

            //mogoce zamenjat z proximity. nevem ce sicer ker gear morjo vidt vsi da nebo prletu lokalno en nagex k je u resnic do konca pogearan
            networkObject.SendRpc(RPC_SEND_LOADOUT_UPDATE, Receivers.All,
                l0, l1, l2, l3, l4, l5, l6, l7, l8
                );

            if (onLoadoutChangedCallback != null)
                onLoadoutChangedCallback.Invoke();
        }
    }

    public override void SendPersonalInventoryUpdate(RpcArgs args)
    {//nc zrihtan za kolicino
        //to bi mogu dobit samo owner in NOBEN drug, sicer je nrdit ESP hack najbolj trivialna stvar na planetu
        Debug.Log(" personal inventory rpc receive: owner server id: " + GetComponent<NetworkPlayerStats>().server_id + " | networkId : " + networkObject.Owner.NetworkId);
        if (args.Info.SendingPlayer.NetworkId != 0) return;//ce ni poslov server al pa ce je prejeu en drug k owner(kar s eneb smel nrdit sploh!)




        for (int i = 0; i < 20; i++)
        {
            short item_id = args.GetNext<short>();
            if (item_id > 0) this.items[i] = Mapper.instance.getItemById((int)item_id);
        }
        if (onLoadoutChangedCallback != null)
            onLoadoutChangedCallback.Invoke();
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();

    }

    public override void SendLoadoutUpdate(RpcArgs args)//ce je host se tole senkrat prepece cez, i dont give a fuck honestly..
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;

        int i = (int)args.GetNext<short>();
        if (i >= 0) this.head = Mapper.instance.getItemById(i);
        else this.head = null;

        i = (int)args.GetNext<short>();
        if (i >= 0) this.chest = Mapper.instance.getItemById(i);
        else this.chest = null;

        i = (int)args.GetNext<short>();
        if (i >= 0) this.hands = Mapper.instance.getItemById(i);
        else this.hands = null;

        i = (int)args.GetNext<short>();
        if (i >= 0) this.legs = Mapper.instance.getItemById(i);
        else this.legs = null;

        i = (int)args.GetNext<short>();
        if (i >= 0) this.feet = Mapper.instance.getItemById(i);
        else this.feet = null;


        i = (int)args.GetNext<short>();
        if (i >= 0) this.ranged = Mapper.instance.getItemById(i);//NE DELA - BO TREBA UPDEJTAT. ZAENKRAT SE UPORABLA GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();   KER JE BLO ZE PREJ IMPLEMENTIRAN!!
        else this.ranged = null;

        i = (int)args.GetNext<short>();
        if (i >= 0) this.weapon_0 = Mapper.instance.getItemById(i);//NE DELA - BO TREBA UPDEJTAT. ZAENKRAT SE UPORABLA GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();   KER JE BLO ZE PREJ IMPLEMENTIRAN!!
        else this.weapon_0 = null;

        i = (int)args.GetNext<short>();
        if (i >= 0) this.weapon_1 = Mapper.instance.getItemById(i);//NE DELA - BO TREBA UPDEJTAT. ZAENKRAT SE UPORABLA GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();   KER JE BLO ZE PREJ IMPLEMENTIRAN!!
        else this.weapon_1 = null;

        i = (int)args.GetNext<short>();
        if (i >= 0) this.shield = Mapper.instance.getItemById(i);//NE DELA - BO TREBA UPDEJTAT. ZAENKRAT SE UPORABLA GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();   KER JE BLO ZE PREJ IMPLEMENTIRAN!!
        else this.shield = null;


        GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();


        if (onLoadoutChangedCallback != null)
            onLoadoutChangedCallback.Invoke();
        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..
            onItemChangedCallback.Invoke();//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej

    }

    /// <summary>
    /// menjamo pozicije itemov znotraj inventorija. ubistvu ni nekih komplikacij.
    /// </summary>
    /// <param name="invSlot"></param>
    private void InventoryToInventory(RectTransform invSlot)
    {
        if (!networkObject.IsOwner) return;
        int index1 = getIndexFromName(invSlot.name);
        int index2 = getIndexFromName(this.draggedItemParent.name);

        networkObject.SendRpc(RPC_INVENTORY_TO_INVENTORY_REQUEST, Receivers.Server, index1, index2);

    }

    /// <summary>
    /// edina stvar ki jo lahko menja sta weapona tko da tukaj nebo kompliciranja
    /// </summary>
    /// <param name="invSlot"></param>
    private void LoadoutToLoadout(RectTransform invSlot)
    {
        if (!networkObject.IsOwner) return;

        networkObject.SendRpc(RPC_LOADOUT_TO_LOADOUT_REQUEST, Receivers.Server);

    }

    private void LoadoutToInventory(RectTransform invSlot, bool rightClick)
    {
        if (!networkObject.IsOwner) return;
        int index = -1;

        if (!rightClick) index = getIndexFromName(invSlot.name);
        string type_s = this.draggedItemParent.GetComponent<InventorySlotLoadout>().type.ToString();

        int loadout_index = getIndexFromName(this.draggedItemParent.name);

        networkObject.SendRpc(RPC_LOADOUT_TO_INVENTORY_REQUEST, Receivers.Server, index, type_s, loadout_index);

    }


    private int getFreePersonalInventorySpace()
    {
        int cunt = 0;
        foreach (Item i in items)
            if (i == null)
                cunt++;
        return cunt;
    }

    internal void instantiateDroppedItem(Item item, int quantity, Vector3 camera_vector, Vector3 camera_forward) // instantiate it when dropped - zapakiral v rpc da se poslje vseskup na server
    {
        if (!networkObject.IsServer) { Debug.LogError("instanciacija objekta k smo ga dropal ni na serverjvu!");  return; }


        networkObject.SendRpc(RPC_NETWORK_INSTANTIATION_SERVER_REQUEST, Receivers.Server, getNetworkIdFromItem(item), camera_vector, camera_forward);
    }

    private int getNetworkIdFromItem(Item item)
    {
        GameObject[] items = NetworkManager.Instance.Interactable_objectNetworkObject;

        for (int i = 0; i < items.Length; i++) {
            if (items[i].GetComponent<ItemPickup>().i.id == item.id)
                return i;
        }
        throw new Exception("failed to get Id for networkBehaviour instantiation");

        
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

    

    public override void RequestLoadoutOnConnect(RpcArgs args)
    {
        //obleke - weapone bo treba dodat
        if(networkObject.IsServer)
            sendNetworkUpdate(false, true);//tole poslje vsem, ne samo temu k ga rab. optimiziacija ksnej.
    }

    public override void NetworkInstantiationServerRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) { Debug.LogError("instanciacija na clientu ne na serverju!");  return; }
        int net_id = args.GetNext<int>();
        Vector3 pos = args.GetNext<Vector3>();
        Vector3 dir = args.GetNext<Vector3>();
        Interactable_objectBehavior b = NetworkManager.Instance.InstantiateInteractable_object(net_id, pos);

        //apply force on clients
        b.gameObject.GetComponent<Interactable>().setForce(pos,dir);

    }

    public override void DropItemFromPersonalInventoryRequest(RpcArgs args)
    {

        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client probava dropat item, to mora met server cez.. al pa request ni od ownerja");  return; }
        int inventory_slot = args.GetNext<int>();
        Vector3 camera_vector = args.GetNext<Vector3>();
        Vector3 camera_forward = args.GetNext<Vector3>();
        Item i = slots[inventory_slot].GetItem();//mogoce nerabmo sploh slotov za server. sj rab vidt samo array itemov. sloti so bl k ne samo za ownerja da vidi inventorij graficno. optimizacija ksnej
        removePersonalInventoryItem(inventory_slot);
        instantiateDroppedItem(i, 1, camera_vector, camera_forward);

        //rpc update
        sendNetworkUpdate(true, false);
        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..
            onItemChangedCallback.Invoke();//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
    }

    public override void DropItemFromLoadoutRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client probava dropat item, to mora met server cez.. al pa request ni od ownerja"); return; }
        string type_s = args.GetNext<string>();
        Item.Type type = getItemTypefromString(type_s);
        int index = args.GetNext<int>();
        Vector3 camera_vector = args.GetNext<Vector3>();
        Vector3 camera_forward = args.GetNext<Vector3>();
        Item i = PopLoadoutItem(type, index);
        instantiateDroppedItem(i, 1, camera_vector, camera_forward);

        //rpc update
        sendNetworkUpdate(false, true);

        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..
            onItemChangedCallback.Invoke();//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
    }

    internal Item.Type getItemTypefromString(string s) {
        foreach (Item.Type itemType in Enum.GetValues(typeof(Item.Type))) {
            if (itemType.ToString().Equals(s)) { return itemType; }
        }
        Debug.LogError("Item.Type mismatch! fix this shit");
        return Item.Type.resource;
    }

    public override void InventoryToLoadoutRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client dela nekej kar mora server"); return; }

        int loadout_index = args.GetNext<int>();
        string type_s = args.GetNext<string>();
        Item.Type type = getItemTypefromString(type_s);
        Item loadout_item = null;
        loadout_item = PopLoadoutItem(type, loadout_index);

        int inv_index = args.GetNext<int>();
        Item inventory_item = this.items[inv_index];//poisce item glede na id-ju slota. id dobi z rpc k ga poda z imena tega starsa


        if (SetLoadoutItem(inventory_item, loadout_index))//to bo zmer slo cez ker je slot ze prazen. smo ga izpraznli z popom. vrne true ce je item biu valid za nek loadout slot.
            Remove(inv_index);
        if (loadout_item != null)
        {//loadout ni bil prazen prej tko da rabmo item dat v inventorij
            Add(loadout_item, 1, inv_index);
        }

        //rpc update
        sendNetworkUpdate(true, true);

        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onItemChangedCallback.Invoke();
        if (onLoadoutChangedCallback != null)//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onLoadoutChangedCallback.Invoke();
    }

    public override void LoadoutToInventoryRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client dela nekej kar mora server"); return; }

        int index = args.GetNext<int>();
        Item.Type t = getItemTypefromString(args.GetNext<string>());
        int loadout_index = args.GetNext<int>();

        Item loadout_item = null;
        loadout_item = PopLoadoutItem(t, loadout_index);
        if (loadout_item == null)
        {
            Debug.LogError("dragged loadout item is null. this is not possible.");
            return;
        }

        if (index != -1)
        {//za right click
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
            else
            {//dodaj na ta slot.
                Add(loadout_item, 1, index);
            }
        }
        else if (hasInventorySpace())
        {
            //da rabmo item dat v inventorij
            AddFirst(loadout_item, 1);
        }
        else
        {
            Debug.Log("No Space in inventory and cannot place in inventory!");
        }

        //rpc update
        sendNetworkUpdate(true, true);

        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onItemChangedCallback.Invoke();
        if (onLoadoutChangedCallback != null)//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onLoadoutChangedCallback.Invoke();

    }

    public override void InventoryToInventoryRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client dela nekej kar mora server"); return; }
        int index1 = args.GetNext<int>();
        int index2 = args.GetNext<int>();
        Item temp = items[index1];
        this.items[index1] = this.items[index2];
        this.items[index2] = temp;

        //rpc update
        sendNetworkUpdate(true, false);

        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..
            onItemChangedCallback.Invoke();//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
    }

    public override void LoadoutToLoadoutRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client dela nekej kar mora server"); return; }

        Item temp = weapon_1;
        weapon_1 = weapon_0;
        weapon_0 = temp;

        //rpc update
        sendNetworkUpdate(false, true);

        if (onLoadoutChangedCallback != null)//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onLoadoutChangedCallback.Invoke();
    }
}
