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
    internal NetworkBackpack backpack_inventory;
    public int draggedParent_sibling_index = -1;
    public int draggedParent_parent_sibling_index = -1;

    public int space = 20; // kao space inventorija
    public Predmet[] predmeti_personal = new Predmet[20]; // seznam itemov, ubistvu inventorij
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    //public GameObject panel_inventory; //celotna panela za inventorij, to se izrise ko prtisnes "i"

    public Transform[] panel_personalInventorySlots;
    InventorySlotPersonal[] slots;  // predstavlajo slote v inventoriju, vsak drzi en item. 



    private DynamicCharacterAvatar avatar;
    public NetworkPlayerCombatHandler combatHandler;
    //-------------------------------LOADOUT SLOTS-----------------------------
    public InventorySlotLoadout loadout_head;
    public InventorySlotLoadout loadout_chest;
    public InventorySlotLoadout loadout_hands;
    public InventorySlotLoadout loadout_legs;
    public InventorySlotLoadout loadout_feet;
    public InventorySlotLoadout loadout_backpack;//ni loadout item ubistvu. logika je cist locena ker je prioriteta da se backpack lahko cimlazje fukne dol. tle ga mam samo za izrisovanje v inventorij panel

    public InventorySlotBar[] bar_slots;
    [SerializeField]
    public Predmet[] predmeti_hotbar;

    private Predmet head;
    private Predmet chest;


    private Predmet hands;
    private Predmet legs;
    private Predmet feet;

    public delegate void OnLoadoutChanged();
    public OnLoadoutChanged onLoadoutChangedCallback;

    public Predmet backpack;
    private Camera c;



    public Transform backpackSpot; //tukaj se parenta backpack
    public backpack_local_panel_handler backpackPanel;
    public panel_bar_handler barPanel;
    internal NetworkPlayerNeutralStateHandler neutralStateHandler;
    private NetworkPlayerStats stats;

    private List<PredmetRecepie> craftingQueue;
    private IEnumerator craftingRoutine;
    private int craftingTimeRemaining = 0;

    private void Start()
    {
        this.craftingQueue = new List<PredmetRecepie>();
        this.combatHandler = GetComponent<NetworkPlayerCombatHandler>();
        this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        this.avatar = GetComponent<DynamicCharacterAvatar>();
        this.stats = GetComponent<NetworkPlayerStats>();
        predmeti_hotbar = new Predmet[this.bar_slots.Length];

        slots = new InventorySlotPersonal[panel_personalInventorySlots.Length];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = panel_personalInventorySlots[i].GetComponent<InventorySlotPersonal>();
        predmeti_personal = new Predmet[slots.Length];

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

        if (networkObject.IsOwner && networkObject.IsServer) instantiate_server_weapons_for_testing();
        if (networkObject.IsOwner) UpdateUI();//bugfix
    }

    /// <summary>
    /// vrne kter weapon ima trenutno v roki. ni nujno da ima ksn weapon sploh v roki mind you.
    /// </summary>
    /// <returns></returns>
    internal Predmet GetWeaponItemInHand()
    {
        return combatHandler.GetCurrentlyActiveWeapon();
    }

    /// <summary>
    /// vrne kter shield ima trenutno v roki. ni nujno da ima ksn shield sploh v roki mind you.
    /// </summary>
    /// <returns></returns>
    internal Predmet GetShieldItemInHand()
    {
        return combatHandler.GetCurrentlyActiveShield();
    }

    /// <summary>
    /// vrne kter ranged weap ima trenutno v roki. ni nujno da ima ksn ranged sploh v roki mind you.
    /// </summary>
    /// <returns></returns>
    internal Predmet GetRangedItemInHand()
    {
        return combatHandler.GetCurrentlyActiveRanged();
    }

    /// <summary>
    /// potegne z hotbara - samo server klice
    /// </summary>
    /// <returns>item k smo ga sunli z hotbara</returns>
    internal Predmet PopWeaponPredmetInHand()
    {
        if (!networkObject.IsServer) return null;
        if (combatHandler.GetCurrentlyActiveWeapon() != null)
        {
            Predmet p = this.predmeti_hotbar[neutralStateHandler.selected_index];
            this.predmeti_hotbar[neutralStateHandler.selected_index] = null;
            return p;
        }
        return null;
    }

    /// <summary>
    /// potegne z hotbara
    /// </summary>
    /// <returns>item k smo ga sunli z hotbara</returns>
    internal Predmet PopShieldPredmetInHand()
    {
        if (!networkObject.IsServer) return null;
        if (combatHandler.GetCurrentlyActiveShield() != null)
        {
            Predmet i = this.predmeti_hotbar[neutralStateHandler.selected_index_shield];
            this.predmeti_hotbar[neutralStateHandler.selected_index_shield] = null;
            return i;
        }
        return null;
    }

    /// <summary>
    /// potegne z hotbara
    /// </summary>
    /// <returns>item k smo ga sunli z hotbara</returns>
    internal Predmet PopRangedItemInHand()
    {
        if (!networkObject.IsServer) return null;
        if (combatHandler.GetCurrentlyActiveRanged() != null)
        {
            Predmet i = this.predmeti_hotbar[neutralStateHandler.selected_index];
            this.predmeti_hotbar[neutralStateHandler.selected_index] = null;
            return i;
        }
        return null;
    }



    /// <summary>
    /// vrne item, ki je na tem mestu ampak ga ne zbrise. za get+delete = pop
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    internal Predmet getBarPredmet(int v)
    {
        if (v == -1) return null;
        return this.predmeti_hotbar[v];
    }

    public void refresh_UMA_equipped_gear()
    {
        if (avatar == null) return;
        avatar.ClearSlots();

        if (this.head != null)
        {
            avatar.SetSlot("Helmet", this.head.item.uma_item_recipe_name);
        }

        if (this.chest != null)
        {
            avatar.SetSlot("Chest", this.chest.item.uma_item_recipe_name);
        }

        if (this.hands != null)
        {
            avatar.SetSlot("Hands", this.hands.item.uma_item_recipe_name);
        }

        if (this.legs != null)
        {
            avatar.SetSlot("Legs", this.legs.item.uma_item_recipe_name);
        }

        if (this.feet != null)
        {
            avatar.SetSlot("Feet", this.feet.item.uma_item_recipe_name);
        }

        avatar.BuildCharacter();
    }


    /// <summary>
    /// proba upgrejdat loadout z itemom i. vrne item i ce ni upgrade. vrne item s katermu ga je zamenov ce je upgrade bil, vrne null ce je biu prazn slot prej
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public Predmet try_to_upgrade_loadout(Predmet i)
    {//vrne item s katermu smo ga zamenjal al pa null ce je biu prej prazn slot
        if (!networkObject.IsServer) { Debug.LogError("client se ukvarja z metodo k je server designated.."); return null; }
        if (i == null) return null;
        if (i.item == null) return null;
        Predmet r = i;
        switch (i.item.type)
        {
            case Item.Type.head:
                if (compareGear(head, i))
                {
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
            default:
                break;
        }
        if (onItemChangedCallback != null && networkObject.IsServer)//ker se nkol ne izvede na clientu ta metoda in server itak nebo nkol vidu inventorija od drugih na ekranu.. probably
            onItemChangedCallback.Invoke();
        return r;
    }

    internal Predmet popPersonalPredmet(int inv_index)
    {
        Predmet i = null;
        if (networkObject.IsServer)
        {
            i = this.predmeti_personal[inv_index];
            this.predmeti_personal[inv_index] = null;
        }
        return i;
    }

    internal void setPersonalIventoryPredmet(Predmet b, int inv_index)
    {
        if (networkObject.IsServer)
        {
            if (this.predmeti_personal[inv_index] != null)
            {
                Debug.LogError("Overriding an item in personal inventory. i hope its intentional brah.");
            }
            this.predmeti_personal[inv_index] = b;
        }
    }

    internal void requestUiUpdate()
    {
        if (onItemChangedCallback != null)//za backpack ker se ne steje v dejanski loadout ampak je svoja stvar
            onItemChangedCallback.Invoke();
    }

    internal Predmet getHeadItem()
    {
        return this.head;
    }
    internal Predmet getChestItem()
    {
        return this.chest;
    }
    internal Predmet getHandsItem()
    {
        return this.hands;
    }
    internal Predmet getLegsItem()
    {
        return this.legs;
    }
    internal Predmet getFeetItem()
    {
        return this.feet;
    }

    internal Predmet getBackpackItem()
    {
        return this.backpack;
    }

    internal bool handleItemPickup(Predmet p)
    {
        return handleItemPickup(p, false);
    }

    /// <summary>
    /// vrne true ce smo pobral item, vrne false ce faila oziroma ce ni nikjer placa
    /// </summary>
    /// <param name="pobran_objekt"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    internal bool handleItemPickup(Predmet pobran_objekt, bool inRecursion)
    {
        if (!networkObject.IsServer)
        {
            Debug.LogError("client se ukvarja z inventorijem, to se mora samo server.");
            return false;
        }
        if (pobran_objekt == null) return false;

        //kaj ce ima pobran objekt preveliko kvantiteto? - razbij magar z rekurzijo
        if (pobran_objekt.quantity > pobran_objekt.item.stackSize)
        {
            while (pobran_objekt.quantity > pobran_objekt.item.stackSize)
            {
                Predmet k = new Predmet(pobran_objekt.item, pobran_objekt.item.stackSize, pobran_objekt.durability, pobran_objekt.creator);
                pobran_objekt.quantity -= pobran_objekt.item.stackSize;
                handleItemPickup(k, true);
            }
        }



        Predmet resp = try_to_upgrade_loadout(pobran_objekt);
        if (resp != null && resp.item.stackSize > 1)
            resp = tryToAddPredmetToExistingStack(resp);

        if (resp != null)
        {
            if ((resp.item.type == Item.Type.weapon || resp.item.type == Item.Type.shield || resp.item.type == Item.Type.ranged || resp.item.type == Item.Type.tool) && hasBarSpace())
            {
                BarAddFirst(resp);
            }
            else if (hasInventoryEmptySlot())
            {
                AddFirst(resp);
            }
            else if (this.backpack != null)
            {//ce ima backpack
                if (this.backpack_inventory.hasSpace())
                {
                    this.backpack_inventory.putFirst(resp);
                    this.backpack_inventory.sendBackpackItemsUpdate();
                }
                else if (hasBarSpace())
                {
                    BarAddFirst(resp);
                }
                else
                {
                    //mormo instantiatat
                    instantiateDroppedPredmet(resp);
                }
            }
            else if (hasBarSpace())
            {
                BarAddFirst(resp);
            }
            else
            {
                //mormo instantiatat
                instantiateDroppedPredmet(resp);
            }
        }
        if (!inRecursion) sendNetworkUpdate(true, true);//posljemo obojeee optimizacija later - ce smo znotrej rekurzije ne posljemo, bo poslala glavna metoda potem ko bo vse ze nrjen
        return true;
    }

    /// <summary>
    /// pogleda ves inventorij in poskusi dodat ze obstojecim stackom tega itema. vrne del itema, ki ga ni mogu dat na stack, vrne celoten item nazaj ce ni nobenga stacka, vrne null ce mu je uspelo vse dat na nek stack.
    /// </summary>
    /// <param name="resp"></param>
    /// <returns></returns>
    private Predmet tryToAddPredmetToExistingStack(Predmet resp)
    {
        //poglej najprej invnetorij v while zanki
        Predmet p = resp;
        //while (getNumberOfEligibleNonEmptyStacks_personalInventory(resp.item) > 0) { //gre itak cez vse stacke in proba dodat tko da itak ze vse ujame
        foreach (Predmet stack in this.predmeti_personal)
            if (stack != null)
                if (stack.item != null)
                    if (stack.item.Equals(p.item))
                        if (stack.quantity < stack.item.stackSize)
                        {
                            p = stack.addQuantity(p);
                            if (p == null)
                                return null;
                        }
        //}
        //poglej backpack
        if (this.backpack != null)
            foreach (Predmet stack in this.backpack_inventory.getAll())
                if (stack != null)
                    if (stack.item != null)
                        if (stack.item.Equals(p.item))
                            if (stack.quantity < stack.item.stackSize)
                            {
                                p = stack.addQuantity(p);
                                if (p == null)
                                    return null;
                            }
        //poglej hotbar
        foreach (Predmet stack in this.predmeti_hotbar)
            if (stack != null)
                if (stack.item != null)
                    if (stack.item.Equals(p.item))
                        if (stack.quantity < stack.item.stackSize)
                        {
                            p = stack.addQuantity(p);
                            if (p == null)
                                return null;
                        }

        return p;
    }

    private int getNumberOfEligibleNonEmptyStacks_personalInventory(Item item)
    {
        int c = 0;
        foreach (Predmet p in this.predmeti_personal)
            if (p != null)
                if (p.item != null)
                    if (p.item.Equals(item))
                        if (p.quantity < p.item.stackSize)
                            c++;
        return c;
    }

    private int getNumberOfEligibleNonEmptyStacks_backpack(Item item)
    {
        if (this.backpack == null) return 0;

        int c = 0;
        foreach (Predmet p in this.backpack_inventory.getAll())
            if (p != null)
                if (p.item != null)
                    if (p.item.Equals(item))
                        if (p.quantity < p.item.stackSize)
                            c++;
        return c;
    }

    private int getNumberOfEligibleNonEmptyStacks_hotbar(Item item)
    {
        int c = 0;
        foreach (Predmet p in this.predmeti_hotbar)
            if (p != null)
                if (p.item != null)
                    if (p.item.Equals(item))
                        if (p.quantity < p.item.stackSize)
                            c++;
        return c;
    }

    internal void OnRightClick(GameObject g)//tole lahko potem pri ciscenju kode z malo preurejanja damo v uno tavelko metodo
    {
        handleInventorySlotOnDragDropEvent(null, g.transform, true);
    }

    internal void OnRightClickBackpack(GameObject gameObject)//ce smo kliknli z desno na backpack. proba dat na loadout, ce ni prazn nrdi swap
    {
        if (networkObject.IsOwner)
        {
            int index_backpack = getIndexFromName(gameObject.name);
            this.backpack_inventory.localPlayerRequestBackpackToLoadout(index_backpack);
        }
    }

    internal bool hasInventoryEmptySlot()
    {
        foreach (Predmet i in this.predmeti_personal)
            if (i == null)
                return true;
        return false;
    }

    internal bool hasBarSpace()
    {
        foreach (Predmet i in this.predmeti_hotbar)
            if (i == null) return true;
        return false;
    }

    public bool SetLoadoutItem(Predmet i)
    {//nevem zakaj vrne bool
        if (!networkObject.IsServer) { Debug.LogError("client dela stvar od serevrja!"); return false; }
        if (i == null) return false;

        Predmet r = i;
        switch (i.item.type)
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
            case Item.Type.backpack:
                backpack = i;
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
    private bool compareGear(Predmet x, Predmet i)
    {
        if (x == null) return true;
        else return false;
    }

    public void RemoveItemLoadout(Item.Type t)
    {
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
            case Item.Type.backpack:
                this.backpack = null;
                break;
            default:
                Debug.LogError("Item type doesnt match anything. shits fucked yo");
                break;
        }
    }

    public Predmet popPredmetLoadout(Item.Type t)
    {
        Predmet ret = null;
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
            case Item.Type.backpack:
                ret = backpack;
                backpack = null;
                break;
            default:
                Debug.LogError("Item type doesnt match anything. shits fucked yo");
                break;
        }
        return ret;
    }

    public Predmet GetItemLoadout(Item.Type t)
    {
        Predmet ret = null;
        switch (t)
        {
            case Item.Type.head:
                ret = head;
                break;
            case Item.Type.chest:
                ret = chest;
                break;
            case Item.Type.hands:
                ret = hands;
                break;
            case Item.Type.legs:
                ret = legs;
                break;
            case Item.Type.feet:
                ret = feet;
                break;
            case Item.Type.backpack:
                ret = backpack;
                break;
            default:
                Debug.LogError("Item type doesnt match anything. shits fucked yo");
                break;
        }
        return ret;
    }

    internal bool tryToAddItem(Predmet predmet_za_dodat)
    {
        if (predmet_za_dodat == null) return false;

        if (hasBarSpace())
        {
            BarAddFirst(predmet_za_dodat);
        }
        else if (hasInventoryEmptySlot())
        {
            AddFirst(predmet_za_dodat);
        }
        else if (hasBackpackSpace())
        {
            backpack_inventory.AddFirst(predmet_za_dodat);
        }
        else
        {
            instantiateDroppedPredmet(predmet_za_dodat, transform.position + new Vector3(0, 1, 0), transform.forward);
            return false;
        }
        return true;
    }

    /// <summary>
    /// povozi kar je blo prej not
    /// </summary>
    /// <param name="b"></param>
    /// <param name="bar_index"></param>
    internal void setBarItem(Predmet b, int bar_index)
    {
        if (bar_index < this.predmeti_hotbar.Length)
        {
            this.predmeti_hotbar[bar_index] = b;
        }
    }

    internal void BarAddFirst(Predmet onStand)
    {
        if (networkObject.IsServer)
        {
            for (int i = 0; i < this.predmeti_hotbar.Length; i++)
            {
                if (this.predmeti_hotbar[i] == null)
                {
                    this.predmeti_hotbar[i] = onStand;
                    return;
                }
            }
        }
    }

    internal bool hasBackpackSpace()
    {
        if (this.backpack_inventory != null)
        {
            if (this.backpack_inventory.hasSpace())
                return true;
        }
        return false;
    }

    internal Predmet popBarPredmet(int bar_index)
    {
        if (bar_index < this.predmeti_hotbar.Length)
        {
            Predmet r = this.predmeti_hotbar[bar_index];
            this.predmeti_hotbar[bar_index] = null;
            return r;
        }
        Debug.LogError("Size mismatch");
        return null;
    }

    void Update()
    {

        if (networkObject == null)
        {
            Debug.LogError("networkObject is null.");
            return;
        }
        if (!networkObject.IsOwner) return;
        // Check to see if we should open/close the inventory
        //Debug.Log("items - " + this.items.Length);
        if (this.predmeti_personal.Length == 0) this.predmeti_personal = new Predmet[20];//hacky bug fix. makes me sick about this brah. mrde dat u onNetworkConnected al pa kej


    }

    public void addToPersonalInventory(Predmet item, int index)
    {
        if (!networkObject.IsServer) return;
        if (index < 0)
        {//dodaj na prvo mesto ki je null
            addToPersonalInventoryFirstEmpty(item);
        }
        else
        {//dodaj na mesto oznaceno z index ce ni polno, sicer na prvo prazno mesto
            if (predmeti_personal[index] != null) addToPersonalInventoryFirstEmpty(item);
            else predmeti_personal[index] = item;
        }
    }

    private void addToPersonalInventoryFirstEmpty(Predmet item)
    {
        if (!networkObject.IsServer) return;
        for (int i = 0; i < predmeti_personal.Length; i++)
        {
            if (predmeti_personal[i] == null)
            {
                predmeti_personal[i] = item;
                return;
            }
        }
    }

    private void removePersonalInventoryItem(int index)
    {
        if (!networkObject.IsServer) return;
        if (index > -1 || index < predmeti_personal.Length)
            predmeti_personal[index] = null;
    }

    private void removePersonalInventoryItem(Predmet i, int index)
    {
        if (!networkObject.IsServer) return;
        if (index == -1)
        {
            removePersonalInventoryItemFirstMatch(i);
        }
        else
        {
            predmeti_personal[index] = null;
        }
    }

    private void removePersonalInventoryItemFirstMatch(Predmet it)
    {
        if (!networkObject.IsServer) return;
        for (int i = 0; i < predmeti_personal.Length; i++)
        {
            if (predmeti_personal[i].Equals(it))
            {
                predmeti_personal[i] = null;
                return;
            }
        }
    }

    public void Add(Predmet item, int index)
    {
        if (!networkObject.IsServer) return;
        addToPersonalInventory(item, index);//nekej bo treba nrdit za hranjenje kolicine. recimo kamen pa take fore
    }

    public void AddFirst(Predmet item)
    {
        if (!networkObject.IsServer) return;
        addToPersonalInventory(item, -1);//nekej bo treba nrdit za hranjenje kolicine. recimo kamen pa take fore
    }
    public void RemoveFirst(Predmet item)
    {
        if (!networkObject.IsServer) return;
        removePersonalInventoryItem(item, -1);

    }

    public void Remove(int inventory_slot) // to se klice z slota na OnDrop eventu ko vrzemo item iz inventorija
    {
        if (!networkObject.IsServer) return;
        removePersonalInventoryItem(inventory_slot);
    }

    public void DropItemFromPersonalInventory(int inventory_slot)
    {//isto k remove item samo da vrze item v svet.
        if (!networkObject.IsOwner) return;
        Camera c = Camera.main;

        networkObject.SendRpc(RPC_DROP_ITEM_FROM_PERSONAL_INVENTORY_REQUEST, Receivers.Server, inventory_slot, c.transform.position + (c.transform.forward * 3), c.transform.forward);


    }

    internal void DropItemFromLoadout(Item.Type type, int index)
    {
        if (!networkObject.IsOwner) return;
        Camera c = Camera.main;
        networkObject.SendRpc(RPC_DROP_ITEM_FROM_LOADOUT_REQUEST, Receivers.Server, type.ToString(), index, c.transform.position + (c.transform.forward * 3), c.transform.forward);
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
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (predmeti_personal[i] != null)  // If there is an item to add
            {
                slots[i].AddPredmet(this.predmeti_personal[i]);   // Add it
            }
            else
            {
                // Otherwise clear the slot
                slots[i].ClearSlot();
            }
        }

        //update loadout. hardcoded pain lol

        if (this.head != null)
            loadout_head.AddPredmet(this.head);
        else
            loadout_head.ClearSlot();

        if (this.chest != null)
            loadout_chest.AddPredmet(this.chest);
        else
            loadout_chest.ClearSlot();

        if (this.hands != null)
            loadout_hands.AddPredmet(this.hands);
        else
            loadout_hands.ClearSlot();

        if (this.legs != null)
            loadout_legs.AddPredmet(this.legs);
        else
            loadout_legs.ClearSlot();

        if (this.feet != null)
            loadout_feet.AddPredmet(this.feet);
        else
            loadout_feet.ClearSlot();

        if (this.backpack != null)
            loadout_backpack.AddPredmet(this.backpack);
        else
            loadout_backpack.ClearSlot();

        //Update bar slots
        if (this.predmeti_hotbar.Length == this.bar_slots.Length)
            for (int i = 0; i < this.bar_slots.Length; i++)
            {
                if (this.predmeti_hotbar[i] != null)
                    bar_slots[i].AddPredmet(this.predmeti_hotbar[i]);
                else
                    bar_slots[i].ClearSlot();
            }
        else { Debug.Log("error - fix this"); }
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
        networkObject.SendRpc(RPC_INVENTORY_TO_LOADOUT_REQUEST, Receivers.Server, loadout_index, this.draggedItemParent.GetComponent<InventorySlotPersonal>().GetPredmet().item.type.ToString(), getIndexFromName(this.draggedItemParent.name));


    }

    private Predmet PopLoadoutItem(Item.Type t)
    {
        if (!networkObject.IsServer) { Debug.LogError("client probava delat stvar k je sam na serverju.."); return null; }

        Predmet i = null;
        switch (t)
        {
            case Item.Type.head:
                i = head;
                head = null;
                break; ;
            case Item.Type.chest:
                i = chest;
                chest = null;
                break;
            case Item.Type.hands:
                i = hands;
                hands = null;
                break;
            case Item.Type.legs:
                i = legs;
                legs = null;
                break;
            case Item.Type.feet:
                i = feet;
                feet = null;
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
        else
        {//right click event
            if (from is InventorySlotPersonal)
            {
                InventoryToLoadout(null, true);
            }
            else if (from is InventorySlotLoadout)
            {
                LoadoutToInventory(null, true);
            }
        }
        this.draggedItemParent = null;//tole nastavmo tud na drag handlerju just in case

        //GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();// je tole client side?? bomo spoznal pr reworku combata..

    }

    internal void handleBackpackSlotOnDragDropEvent(RectTransform invSlot, Transform parent)//sprozi se ko potegnemo backpackslot nekam legit
    {
        if (!networkObject.IsOwner) return;
        if (parent == null)
            parent = this.draggedItemParent;
        this.draggedItemParent = parent.GetComponent<RectTransform>();

        InventorySlot from = parent.GetComponent<InventorySlot>();

        InventorySlot to = invSlot.GetComponent<InventorySlot>();

        if (!to.Equals(from))
        {
            if (to is InventorySlotLoadout)//premikamo iz backpacka na loadout
            {

                BackpackToLoadout(getIndexFromName(from.name));//id zvohamo naprej
            }
            else if (to is InventorySlotPersonal)//iz backpacka v inventorij
            {
                throw new NotImplementedException();

            }
            else if (to is InventorySlotBackpack)//premikamo znotraj backpacka
            {

                InventoryToInventory(invSlot);
            }
        }


    }

    internal void handleLoadoutToBackpackDrag(RectTransform invSlot)
    {
        if (networkObject.IsOwner)
        {
            int index = getIndexFromName(invSlot.name);

            InventorySlotLoadout from = this.draggedItemParent.GetComponent<InventorySlotLoadout>();
            string type = from.type.ToString();
            int weap = from.index;

            InventorySlot to = invSlot.GetComponent<InventorySlot>();
            int backpack_index = getIndexFromName(to.name);
            /*
             string item type
             int weapon index
             int backpackIndex
             */
            this.backpack_inventory.localPlayerLoadoutToBackpackRequest(type, weap, backpack_index);
        }
    }

    internal void handlePersonalToBackpackDrag(RectTransform invSlot)
    {
        if (networkObject.IsOwner)
        {
            InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();

            int inv_index = getIndexFromName(from.name);
            InventorySlot to = invSlot.GetComponent<InventorySlot>();
            int backpack_index = getIndexFromName(to.name);
            this.backpack_inventory.localPlayerInventorySwapRequest(backpack_index, inv_index);
        }
    }

    internal void handleBackpackToPersonalDrag(RectTransform invSlot)
    {
        if (networkObject.IsOwner)
        {
            InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();

            int backpack_index = getIndexFromName(from.name);
            InventorySlot to = invSlot.GetComponent<InventorySlot>();
            int inv_index = getIndexFromName(to.name);
            this.backpack_inventory.localPlayerInventorySwapRequest(backpack_index, inv_index);
        }
    }
    private void BackpackToLoadout(int index)
    {
        this.backpack_inventory.localPlayerRequestBackpackToLoadout(index);
    }


    internal void handleBackpackToBackpack(RectTransform invSlot)
    {
        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();

        int index1 = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int index2 = getIndexFromName(to.name);
        this.backpack_inventory.localPlayerBackpackToBackpackRequest(index1, index2);
    }


    public void sendNetworkUpdateToPlayer(NetworkingPlayer p, bool inv, bool loadout)
    {
        if (!networkObject.IsServer) { Debug.LogError("client poskusa posiljat networkupdate k je samo od serverja.."); return; }
        if (inv)//no security risk since its always sent to owner
        {


            //Debug.Log(" personal inventory rpc SEND: owner server id: " + GetComponent<NetworkPlayerStats>().server_id + " | networkId : " + networkObject.Owner.NetworkId);
            networkObject.SendRpc(p, RPC_SEND_PERSONAL_INVENTORY_UPDATE, getItemsNetwork());
        }

        if (loadout)
        {
            networkObject.SendRpc(p, RPC_SEND_LOADOUT_UPDATE,
                (this.head == null) ? "-1" : this.head.toNetworkString(),
                (this.chest == null) ? "-1" : this.chest.toNetworkString(),
                (this.hands == null) ? "-1" : this.hands.toNetworkString(),
                (this.legs == null) ? "-1" : this.legs.toNetworkString(),
                (this.feet == null) ? "-1" : this.feet.toNetworkString(),
                (this.backpack == null) ? "-1" : this.backpack.toNetworkString()
                );

            if (onLoadoutChangedCallback != null)
                onLoadoutChangedCallback.Invoke();
        }
    }


    public void sendNetworkUpdate(bool inv, bool loadout) //LOADOUT JE SAMO ZA UMA OBLEKE!!!!!!
    {
        if (!networkObject.IsServer) { Debug.LogError("client poskusa posiljat networkupdate k je samo od serverja.."); return; }
        if (inv)//no security risk since its always sent to owner
        {
            //Debug.Log(" personal inventory rpc SEND: owner server id: " + GetComponent<NetworkPlayerStats>().server_id + " | networkId : " + networkObject.Owner.NetworkId);
            networkObject.SendRpc(RPC_SEND_PERSONAL_INVENTORY_UPDATE, Receivers.Owner, getItemsNetwork());
        }

        if (loadout)
        {
            networkObject.SendRpc(RPC_SEND_LOADOUT_UPDATE, Receivers.All,
                (this.head == null) ? "-1" : this.head.toNetworkString(),
                (this.chest == null) ? "-1" : this.chest.toNetworkString(),
                (this.hands == null) ? "-1" : this.hands.toNetworkString(),
                (this.legs == null) ? "-1" : this.legs.toNetworkString(),
                (this.feet == null) ? "-1" : this.feet.toNetworkString(),
                (this.backpack == null) ? "-1" : this.backpack.toNetworkString()
                );

            if (onLoadoutChangedCallback != null)
                onLoadoutChangedCallback.Invoke();
        }
        if (neutralStateHandler == null) this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        if (SelectedWeaponIsNotInHotbar()) neutralStateHandler.ClearActiveWeapons();
    }

    private bool SelectedWeaponIsNotInHotbar()
    {


        if (combatHandler == null) combatHandler = GetComponent<NetworkPlayerCombatHandler>();
        if (neutralStateHandler == null) neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        int activeItem = -1;
        if (combatHandler.GetCurrentlyActiveWeapon() != null) activeItem = combatHandler.GetCurrentlyActiveWeapon().item.id;
        if (combatHandler.GetCurrentlyActiveRanged() != null) activeItem = combatHandler.GetCurrentlyActiveRanged().item.id;
        if (neutralStateHandler.activeTool != null) activeItem = neutralStateHandler.activeTool.item.id;

        int activeShield = -1;
        if (combatHandler.GetCurrentlyActiveShield() != null) activeShield = combatHandler.GetCurrentlyActiveShield().item.id;

        bool zamenjan_shield = false;
        bool zamenjan_item = false;
        if (neutralStateHandler.selected_index > -1)
        {
            if (predmeti_hotbar[neutralStateHandler.selected_index] == null && activeItem != -1)
            {
                zamenjan_item = true;
            }
            else if (predmeti_hotbar[neutralStateHandler.selected_index] != null)
                if (predmeti_hotbar[neutralStateHandler.selected_index].item.id != activeItem)
                    zamenjan_item = true;
        }
        if (neutralStateHandler.selected_index_shield > -1)
        {
            if (predmeti_hotbar[neutralStateHandler.selected_index_shield] == null && activeShield != -1)
            {
                zamenjan_shield = true;
            }
            else if (predmeti_hotbar[neutralStateHandler.selected_index_shield] != null)
                if (predmeti_hotbar[neutralStateHandler.selected_index_shield].item.id != activeShield)
                    zamenjan_shield = true;
        }
        if (zamenjan_shield || zamenjan_item)
        {
            //mormo poslat vsem da nj sinhronizirajo active item. - zunej te metode
            return true;
        }
        return false;
    }

    public override void SendPersonalInventoryUpdate(RpcArgs args)
    {
        //to bi mogu dobit samo owner in NOBEN drug, sicer je nrdit ESP hack najbolj trivialna stvar na planetu
        //Debug.Log(" personal inventory rpc receive: owner server id: " + GetComponent<NetworkPlayerStats>().server_id + " | networkId : " + networkObject.Owner.NetworkId);
        if (args.Info.SendingPlayer.NetworkId != 0) return;//ce ni poslov server al pa ce je prejeu en drug k owner(kar s eneb smel nrdit sploh!)



        //inventory
        string networkStringPersonalAndHotbar = args.GetNext<string>();
        Predmet[] payload = parseItemsNetworkFormat(networkStringPersonalAndHotbar);//velikost tega je this.personal_inventory_objects.Length +this.hotbarItems.length



        for (int i = 0; i < payload.Length; i++)
        {
            if (i < this.predmeti_personal.Length)//pise na personal inventorija
            {
                this.predmeti_personal[i] = payload[i];
            }
            else
            {//pise na hotbara
                this.predmeti_hotbar[i - this.predmeti_personal.Length] = payload[i];

            }
        }
        //ce smo zarad armor standa povozil trenutno equippan weapon mormo to updejtat..
        if (neutralStateHandler == null) this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        if (SelectedWeaponIsNotInHotbar()) neutralStateHandler.ClearActiveWeapons();

        combatHandler.update_equipped_weapons();

        if (onLoadoutChangedCallback != null)
            onLoadoutChangedCallback.Invoke();
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();

    }

    public override void SendLoadoutUpdate(RpcArgs args)//ce je host se tole senkrat prepece cez, i dont give a fuck honestly..
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;


        this.head = Predmet.createNewPredmet(args.GetNext<string>());


        this.chest = Predmet.createNewPredmet(args.GetNext<string>());
        this.hands = Predmet.createNewPredmet(args.GetNext<string>());
        this.legs = Predmet.createNewPredmet(args.GetNext<string>());

        this.feet = Predmet.createNewPredmet(args.GetNext<string>());

        this.backpack = Predmet.createNewPredmet(args.GetNext<string>());

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
        foreach (Predmet i in predmeti_personal)
            if (i == null)
                cunt++;
        return cunt;
    }

    public void instantiate_server_weapons_for_testing()
    {
        Predmet p = new Predmet(null);
        for (int i = 0; i < 50; i++)
        {
            Item it = Mapper.instance.getItemById(i);
            while (it == null) it = Mapper.instance.getItemById(++i);
            p = new Predmet(it, 1, 1000);
            instantiateDroppedPredmet(p, transform.position + Vector3.up * 2, transform.forward);
        }
    }

    internal void instantiateDroppedPredmet(Predmet p)
    {
        instantiateDroppedPredmet(p, transform.position + new Vector3(0, 1, 0), transform.forward);
    }

    internal void instantiateDroppedPredmet(Predmet p, Vector3 camera_vector, Vector3 camera_forward) // instantiate it when dropped - zapakiral v rpc da se poslje vseskup na server
    {
        if (!networkObject.IsServer) { Debug.LogError("instanciacija objekta k smo ga dropal ni na serverjvu!"); return; }


        networkObject.SendRpc(RPC_NETWORK_INSTANTIATION_SERVER_REQUEST, Receivers.Server, p.toNetworkString(), camera_vector, camera_forward);
    }

    private int getNetworkIdFromItem(Item item)
    {
        GameObject[] prefabs = NetworkManager.Instance.Interactable_objectNetworkObject;

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i].Equals(item.prefab_pickup))
                return i;
        }

        Debug.LogWarning("Id of item not found. Item is probably registered as something different from Interactable_objectNetworkObject. Like for example backpack.");
        return -1;

    }

    internal int getIndexFromName(string name)
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
        if (networkObject.IsServer)
            sendNetworkUpdate(false, true);//tole poslje vsem, ne samo temu k ga rab. optimiziacija ksnej.
    }

    public override void NetworkInstantiationServerRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) { Debug.LogError("instanciacija na clientu ne na serverju!"); return; }
        Predmet p = new Predmet(null);
        p.setParametersFromNetworkString(args.GetNext<string>());
        Vector3 pos = args.GetNext<Vector3>();
        Vector3 dir = args.GetNext<Vector3>();
        int net_id = getNetworkIdFromItem(p.item);
        if (net_id == -1) return;//item is not interactable object
        Interactable_objectBehavior b = NetworkManager.Instance.InstantiateInteractable_object(net_id, pos);

        //apply force on clients, sets predmet
        b.gameObject.GetComponent<Interactable>().setStartingInstantiationParameters(p, pos, dir);

    }

    public override void DropItemFromPersonalInventoryRequest(RpcArgs args)
    {

        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client probava dropat item, to mora met server cez.. al pa request ni od ownerja"); return; }
        int inventory_slot = args.GetNext<int>();
        Vector3 camera_vector = args.GetNext<Vector3>();
        Vector3 camera_forward = args.GetNext<Vector3>();
        //Item i = slots[inventory_slot].GetItem();//mogoce nerabmo sploh slotov za server. sj rab vidt samo array itemov. sloti so bl k ne samo za ownerja da vidi inventorij graficno. optimizacija ksnej
        Predmet i = this.predmeti_personal[inventory_slot];
        removePersonalInventoryItem(inventory_slot);
        instantiateDroppedPredmet(i, camera_vector, camera_forward);

        //rpc update
        sendNetworkUpdate(true, false);
        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..
            onItemChangedCallback.Invoke();//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
    }

    public override void DropItemFromLoadoutRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client probava dropat item, to mora met server cez.. al pa request ni od ownerja"); return; }
        string type_s = args.GetNext<string>();
        Item.Type t = getItemTypefromString(type_s);
        int loadout_index = args.GetNext<int>();
        Vector3 camera_vector = args.GetNext<Vector3>();
        Vector3 camera_forward = args.GetNext<Vector3>();
        Predmet i = PopLoadoutItem(t);
        instantiateDroppedPredmet(i, camera_vector, camera_forward);

        //rpc update
        sendNetworkUpdate(false, true);

        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..
            onItemChangedCallback.Invoke();//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
    }

    internal Item.Type getItemTypefromString(string s)
    {
        foreach (Item.Type itemType in Enum.GetValues(typeof(Item.Type)))
        {
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
        Predmet loadout_item = null;
        loadout_item = PopLoadoutItem(type);

        int inv_index = args.GetNext<int>();
        Predmet inventory_item = this.predmeti_personal[inv_index];//poisce item glede na id-ju slota. id dobi z rpc k ga poda z imena tega starsa


        if (SetLoadoutItem(inventory_item))//to bo zmer slo cez ker je slot ze prazen. smo ga izpraznli z popom. vrne true ce je item biu valid za nek loadout slot.
            Remove(inv_index);
        if (loadout_item != null)
        {//loadout ni bil prazen prej tko da rabmo item dat v inventorij
            Add(loadout_item, inv_index);
        }

        //rpc update
        sendNetworkUpdate(true, true);

        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onItemChangedCallback.Invoke();
        if (onLoadoutChangedCallback != null)//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onLoadoutChangedCallback.Invoke();
    }
    /// <summary>
    /// z loadouta da na inventorij ce ima podan index, brez indexa (right click) ga da na prvi mozni slot u inventoriju ce ni poln, ce je poln pogleda ce ima backapack pa da na prvi mest v backpacku, sicer ga vrze na tla
    /// </summary>
    /// <param name="args"></param>
    public override void LoadoutToInventoryRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client dela nekej kar mora server"); return; }

        int index = args.GetNext<int>();
        Item.Type t = getItemTypefromString(args.GetNext<string>());
        int loadout_index = args.GetNext<int>();

        Predmet loadout_item = null;
        loadout_item = PopLoadoutItem(t);
        if (loadout_item == null)
        {
            Debug.LogError("dragged loadout item is null. this is not possible.");
            return;
        }



        if (index != -1)
        {//za right click
            if (this.predmeti_personal[index] != null)//ce smo potegnil na item k ze obstaja.
            {
                if (t == this.predmeti_personal[index].item.type)//ce se item ujema naj se zamenja
                {

                    Predmet inventory_item = this.predmeti_personal[index];
                    if (loadout_item != null)
                    {
                        Add(loadout_item, index);
                        SetLoadoutItem(inventory_item);
                    }
                }
                else if (hasInventoryEmptySlot()) //ce se ne ujema ga mormo dodat na prvo prazno mesto v inventoriju
                {
                    //da rabmo item dat v inventorij
                    AddFirst(loadout_item);
                }
                else
                {
                    Debug.Log("No Space in inventory and cannot switch!");
                }
            }
            else
            {//dodaj na ta slot.
                Add(loadout_item, index);
            }
        }
        else if (hasInventoryEmptySlot())
        {
            //da rabmo item dat v inventorij
            AddFirst(loadout_item);
        }
        else if (backpackHasSpace())
        {//ce ma plac u backpacku ga dodaj pa sinhronizirej
            this.backpack_inventory.putFirst(loadout_item);
            this.backpack_inventory.sendBackpackItemsUpdate();
        }
        else
        {
            Debug.Log("No Space in inventory and cannot place in inventory! Dropping item instead. we have no camera data though");
            instantiateDroppedPredmet(loadout_item, transform.position + new Vector3(0, 1, 0), transform.forward);
        }
        //rpc update
        sendNetworkUpdate(true, true);

        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onItemChangedCallback.Invoke();
        if (onLoadoutChangedCallback != null)//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onLoadoutChangedCallback.Invoke();

    }


    private bool backpackHasSpace()
    {
        if (this.backpack_inventory != null)
            if (this.backpack_inventory.hasSpace())
                return true;
        return false;
    }


    public override void InventoryToInventoryRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client dela nekej kar mora server"); return; }
        int index1 = args.GetNext<int>();
        int index2 = args.GetNext<int>();
        Predmet temp = predmeti_personal[index1];
        this.predmeti_personal[index1] = this.predmeti_personal[index2];
        this.predmeti_personal[index2] = temp;

        //rpc update
        sendNetworkUpdate(true, false);

        if (onItemChangedCallback != null)//najbrz nepotrebno ker je serverj in ne owner ampak ne skodi. optimizacija ksnej..
            onItemChangedCallback.Invoke();//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
    }

    /// <summary>
    /// ne obstaja vec ker weaponi niso vec u loadoutu ampak se berejo direkt z hotbara
    /// </summary>
    /// <param name="args"></param>
    public override void LoadoutToLoadoutRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) { Debug.LogError("client dela nekej kar mora server"); return; }



        //rpc update
        sendNetworkUpdate(false, true);

        if (onLoadoutChangedCallback != null)//najbrz nepotrebno ker se itak klice senkat v rpcju. optimizacija ksnej
            onLoadoutChangedCallback.Invoke();
    }

    internal void ServerSendAll(NetworkingPlayer p)
    {
        if (networkObject.IsServer)
        {
            //naceloma je treba poslat samo args.info.sendingPlayer - optimize later
            sendNetworkUpdateToPlayer(p, false, true);
        }
    }

    #region Gathering
    internal void requestResourceHitServer(Item i, GameObject resource_obj)
    {
        if (networkObject.IsServer)
        {
            if (Vector3.Distance(transform.position, resource_obj.transform.position) < 3f)
            {//mal security-a i guess
                Debug.Log("We hit a resource!");
                //ugotov z ktermu itemom smo udarli in kaj smo udarli

                NetworkResource nrs = resource_obj.GetComponent<NetworkResource>();
                if (i.type == Item.Type.tool || i.type == Item.Type.weapon)
                {
                    //dob vn resource - na podlagi gathering rate-a od tega itema
                    Predmet resource_received = nrs.onHitReturnItemWithQuantity(i, stats.playerName);
                    if (i != null)
                        handleItemPickup(resource_received);//proba dat v inventorij, ce ne more ga dropa
                }
            }
        }
    }
    #endregion

    internal void handleBackpackToBar(RectTransform invSlot)
    {
        backpack_inventory.localPlayerBackpackToBarRequest(getIndexFromName(this.draggedItemParent.name), getIndexFromName(invSlot.name));

    }

    internal void handleBarToBackpack(RectTransform invSlot)
    {
        backpack_inventory.localPlayerBarToBackpackRequest(getIndexFromName(invSlot.name), getIndexFromName(this.draggedItemParent.name));
    }


    internal void handleBarToBar(RectTransform invSlot)
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_BAR_TO_BAR_REQUEST, Receivers.Server, getIndexFromName(invSlot.name), getIndexFromName(this.draggedItemParent.name));
    }

    internal void handleBarToPersonal(RectTransform invSlot)
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_BAR_TO_PERSONAL_REQUEST, Receivers.Server, getIndexFromName(invSlot.name), getIndexFromName(this.draggedItemParent.name));
    }

    internal void handlePersonalToBar(RectTransform invSlot)
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_PERSONAL_TO_BAR_REQUEST, Receivers.Server, getIndexFromName(invSlot.name), getIndexFromName(this.draggedItemParent.name));
    }

    public override void PersonalToBarRequest(RpcArgs args)//bar, personal
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            int bar_index = args.GetNext<int>();
            int inv_index = args.GetNext<int>();

            if (neutralStateHandler.isNotSelected(bar_index, -1))
            {
                if (this.predmeti_personal[inv_index] != null)
                {

                    Predmet b = popPersonalPredmet(inv_index);
                    Predmet i = popBarPredmet(bar_index);

                    setBarItem(b, bar_index);
                    setPersonalIventoryPredmet(i, inv_index);

                    sendNetworkUpdate(true, false);

                }
            }
        }
    }



    public override void BarToPersonalRequest(RpcArgs args)//bar, personal
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            int inv_index = args.GetNext<int>();
            int bar_index = args.GetNext<int>();
            if (neutralStateHandler.isNotSelected(bar_index, -1))
            {
                if (this.predmeti_hotbar[bar_index] != null)
                {
                    Predmet b = popPersonalPredmet(inv_index);
                    Predmet i = popBarPredmet(bar_index);
                    setBarItem(b, bar_index);
                    setPersonalIventoryPredmet(i, inv_index);
                    sendNetworkUpdate(true, false);
                }
            }
        }

    }

    public override void BarToBarRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            int a = args.GetNext<int>();
            int b = args.GetNext<int>();

            if (neutralStateHandler.isNotSelected(a, b))
            { //TODO ce je trenutno izbran item je blokiran pri menjavi. spremenit tko da lhako menja ampak se zamenja potem tud index v neutralStateHandlerju
                Predmet x = popBarPredmet(a);
                setBarItem(popBarPredmet(b), a);
                setBarItem(x, b);
                sendNetworkUpdate(true, false);
            }
        }

    }


    //skopiran iz nci


    /// <summary>
    /// zapise nekak v nek network format da pol plunes u rpc. magar csv al pa nekej
    /// </summary>
    /// <returns></returns>
    internal string getItemsNetwork()//zapakira inventoriy items brezz hotbara!
    {
        string s = "";
        for (int i = 0; i < this.predmeti_personal.Length + this.predmeti_hotbar.Length; i++)
        {
            if (i < this.predmeti_personal.Length)//bere iz personal inventorija
            {
                if (this.predmeti_personal[i] != null)
                    s = s + "|" + this.predmeti_personal[i].toNetworkString();
                else
                    s = s + "|-1";
            }
            else
            {//bere z hotbara
                if (this.predmeti_hotbar[i - this.predmeti_personal.Length] != null)
                    s = s + "|" + this.predmeti_hotbar[i - this.predmeti_personal.Length].toNetworkString();
                else
                    s = s + "|-1";
            }
        }
        Debug.Log(s);
        return s;
    }

    internal Predmet[] parseItemsNetworkFormat(string s)
    {//implementacija te metode je garbage ker bo itak zamenjan ksnej z kšnmu serialized byte array al pa kej namest stringa. optimizacija ksnej
        string[] ss = s.Split('|');
        Predmet[] rez = new Predmet[ss.Length - 1];//zacne se z "" zato en slot sfali
        for (int i = 1; i < ss.Length; i++)
        {//zacne z 1 ker je ss[0] = ""
            rez[i - 1] = Predmet.createNewPredmet(ss[i]);//ce je format networkstringa ured vrne predmet sicer vrne null
        }
        return rez;
    }

    /// <summary>
    /// UILogic klice ko se odpre inventory panel
    /// </summary>
    internal void requestLocalUIUpdate()
    {
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
        if (onLoadoutChangedCallback != null)
            onLoadoutChangedCallback.Invoke();
    }

    #region Crafting

    public int getMaxNumberOfPossibleCraftsForRecipe(PredmetRecepie p)
    {
        int minimum = int.MaxValue;
        for (int i = 0; i < p.ingredients.Length; i++)
        {
            //get max number of crafts for this particular item.
            int q = p.ingredient_quantities[i];
            int pool = getQuantityOfItemOnPlayer(p.ingredients[i]);

            if (pool / q < minimum) minimum = pool / q;
        }


        return minimum;
    }

    private int getQuantityOfItemOnPlayer(Item item)
    {
        //prevert inv, backpack, bar

        int q = 0;

        foreach (Predmet p in this.predmeti_personal)
            if (p != null)
                if (p.item.Equals(item))
                    q += p.quantity;

        if(this.backpack!=null)
            foreach (Predmet p in this.backpack_inventory.getAll())
                if (p != null)
                    if (p.item.Equals(item))
                        q += p.quantity;

        foreach (Predmet p in this.predmeti_hotbar)
            if (p != null)
                if (p.item.Equals(item))
                    q += p.quantity;

        return q;
    }

    internal void localStartCraftingRequest(Item i, int current, int skin)
    {
        Debug.Log("sending crafting request! " + i.Display_name + " x" + current);
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_ITEM_CRAFTING_REQUEST, Receivers.Server, i.id, current, skin);

    }

    public override void ItemCraftingRequest(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId && networkObject.IsServer)
        {
            PredmetRecepie recept = Mapper.instance.getPredmetRecepieForItemid(args.GetNext<int>());
            int quantity = args.GetNext<int>();
            int skin_id = args.GetNext<int>();

            int max_possible_crafts = getMaxNumberOfPossibleCraftsForRecipe(recept);
            if (max_possible_crafts < quantity) quantity = max_possible_crafts;


            //security checks n shit za tier pa take fore




            //djmo craftat quantity objektov
            for (int i = 0; i < quantity; i++)
            {
                pushToCrafting(recept);
            }

            networkObject.SendRpc(args.Info.SendingPlayer, RPC_ITEM_CRAFTING_RESPONSE, getItemIdsFromCraftingQueueNetworkString(this.craftingQueue), this.craftingTimeRemaining);
        }
    }

    private string getItemIdsFromCraftingQueueNetworkString(List<PredmetRecepie> craftingQueue)
    {
        string s = "";
        for (int i = 0; i < this.craftingQueue.Count; i++)
        {

            if (this.craftingQueue[i] != null)
                s = s + "|" + this.craftingQueue[i].Product.id;
            else
                s = s + "|-1";


        }
        //Debug.Log(s);
        if (s.Equals("")) s = "-1";
        return s;
    }

    private List<PredmetRecepie> getCraftingListFromNetworkStringIds(string s)
    {
        string[] k = s.Split('|');
        List<PredmetRecepie> r = new List<PredmetRecepie>();
        int res = -1;
        foreach (string l in k)
        {
            if (int.TryParse(l, out res))
            {
                if (res != -1)
                {
                    r.Add(Mapper.instance.getPredmetRecepieForItemid(res));
                }
            }
        }
        return r;
    }

    private void pushToCrafting(PredmetRecepie recept)
    {
        this.craftingQueue.Add(recept);
        if (this.craftingRoutine == null)
        {//tkole mora bit sa mamo eno instanco coroutine in jo lahko interruptamo! - SINGLETON
            this.craftingRoutine = CraftingCoroutine();
            StartCoroutine(this.craftingRoutine);
        }
    }

    private void cancelCraft(PredmetRecepie p, int priblizni_index)
    {
        //ce je item ta k se trenutno crafta mormo ubit coroutine in jo na novo zastartat
        PredmetRecepie odstranjen = RemoveFromCraftingQueue(p, priblizni_index);



        if (priblizni_index == 0)
            restartCraftingCoroutine();
        //ce nismo odstranil trenutnega pol nimamo kej skrbet.
    }
    internal void localCancelCraftRequest(PredmetRecepie p, int index_sibling)
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_ITEM_CRAFTING_CANCEL_REQUEST, Receivers.Server, p.Product.id, index_sibling);
    }

    public override void ItemCraftingCancelRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            RemoveFromCraftingQueue(Mapper.instance.getPredmetRecepieForItemid(args.GetNext<int>()), args.GetNext<int>());
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_ITEM_CRAFTING_RESPONSE, getItemIdsFromCraftingQueueNetworkString(this.craftingQueue), this.craftingTimeRemaining);
        }
    }
    /// <summary>
    /// vrne predmet k smo ga odstranil
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private PredmetRecepie RemoveFromCraftingQueue(PredmetRecepie p, int priblizni_index)
    {
        if (priblizni_index == 0) {
            //mormo odstrant in prekint coroutine
            PredmetRecepie r = this.craftingQueue[priblizni_index];
            this.craftingQueue.Remove(r);
            StopCoroutine(this.craftingRoutine);
            this.craftingRoutine = null;

                this.craftingRoutine = CraftingCoroutine();//pohendla tud ce je queue enak 0
                StartCoroutine(this.craftingRoutine);
            if (this.craftingQueue.Count > 0) this.craftingTimeRemaining = this.craftingQueue[0].crafting_time;
            else this.craftingTimeRemaining = 0;
            return r;

        }
        else if (this.craftingQueue[priblizni_index].Product.Equals(p.Product))
        {
            PredmetRecepie r = this.craftingQueue[priblizni_index];
            this.craftingQueue.Remove(r);
            return r;
        }
        else
        {
            Debug.LogError("Index in item tip k se crafta se ne ujemata.. magar poisc najblizji item tega timpa in njega odstran");
            return null;
        }
    }

    private void restartCraftingCoroutine()
    {
        StopCoroutine(this.craftingRoutine);
        this.craftingRoutine = null;
        this.craftingRoutine = CraftingCoroutine();
        StartCoroutine(this.craftingRoutine);
    }

    private IEnumerator CraftingCoroutine()
    {

        while (this.craftingQueue.Count > 0)
        {
            PredmetRecepie p = craftingQueue[0];
            for (int i = p.crafting_time; i > 0; i--) {
                this.craftingTimeRemaining = i;
                yield return new WaitForSecondsRealtime(1);
            }
            this.craftingTimeRemaining = 0;
            yield return new WaitForSecondsRealtime(p.crafting_time);

            if (getMaxNumberOfPossibleCraftsForRecipe(p) > 0)
                CraftingTransaction(p);//tle se skor vse dejansko nrdi
            else
                Debug.Log("tried crafting stuff but didnt have required items");
            this.craftingQueue.Remove(p);

        }
        this.craftingRoutine = null;
    }

    //this is where magic happens pobrat mormo resource, jih zbrisat in ustvart nov item. ce se umes zalomi mormo obnovit nazaj stanje - dodat material nazaj
    private void CraftingTransaction(PredmetRecepie p)
    {
        //zaenkrat lahko iteme dobimo samo od personal inventorija, backpacka al pa hotbara. 
        //vemo da mamo dovolj itemov nekje ker smo to pregledal predn smo skocil v to metodo kar pomen da nerabmo? vracat itemov ker naceloma nesmemo failat


        //loop cez vse matse in jih brisemo iz inventorija dokler ne zbrisemo zadost.

        for (int i = 0; i < p.ingredients.Length; i++)
        {
            int q = p.ingredient_quantities[i];//tolkle moramo zbrisat iz nekje
            DeleteIngredients(p.ingredients[i], q);
        }

        Predmet a_baby = new Predmet(p.Product, p.final_quantity, p.Product.durability, stats.playerName);
        handleItemPickup(a_baby);//POSLJE TUD POTREBNI NETWORKUPDATE
    }

    /// <summary>
    /// zbrise tolkle itemov z kjerkoli lahko in NE poslje updejta. updejt se poslje nekje v prejsnji metodi s katere klicemo sicer bi med craftanjem poslal en kup nepotrebnih updejtov.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="q"></param>
    private void DeleteIngredients(Item item, int q)
    {

        if (q > 0)//najprej bi spraznil hotbar
            for (int i = 0; i < this.predmeti_hotbar.Length; i++)
            {
                Predmet p = this.predmeti_hotbar[i];
                if (p != null)
                    if (p.item.Equals(item))
                    {
                        if (p.quantity <= q)
                        {
                            //pobral bomo cel stack.
                            q -= p.quantity;
                            p = null;
                        }
                        else
                        {//poberemo samo del stacka in smo zakljucli
                            p.quantity -= q;
                            return;
                        }
                    }
            }

        //pol bi sli spraznit backpack -------------------------------------TLE MOGOCE PRIDE DO BUGGA KER NEVEM LIH KKO SE PRENASA ARRAY OBJEKTOV AL JE COPY AL JE REFERENCA..
        if (q > 0 && this.backpack!=null)
        {
            Predmet[] predmeti = this.backpack_inventory.getAll();
            for (int i = 0; i < predmeti.Length; i++)
            {
                Predmet p = predmeti[i];
                if (p != null)
                    if (p.item.Equals(item))
                    {
                        if (p.quantity <= q)
                        {
                            //pobral bomo cel stack.
                            q -= p.quantity;
                            p = null;
                        }
                        else
                        {//poberemo samo del stacka in smo zakljucli
                            p.quantity -= q;
                            return;
                        }
                    }
            }
        }

        if (q > 0)//najprej bi spraznil hotbar
            for (int i = 0; i < this.predmeti_personal.Length; i++)
            {
                Predmet p = this.predmeti_personal[i];
                if (p != null)
                    if (p.item.Equals(item))
                    {
                        if (p.quantity <= q)
                        {
                            //pobral bomo cel stack.
                            q -= p.quantity;
                            p = null;
                        }
                        else
                        {//poberemo samo del stacka in smo zakljucli
                            p.quantity -= q;
                            return;
                        }
                    }
            }
    }

    public override void ItemCraftingResponse(RpcArgs args)//vrne nazaj crafting queue
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            List<PredmetRecepie> r = getCraftingListFromNetworkStringIds(args.GetNext<string>());
            GetComponentInChildren<craftingPanelHandler>().updateCraftingQueueWithServerData(r, args.GetNext<int>());
        }
    }

    #endregion
}
