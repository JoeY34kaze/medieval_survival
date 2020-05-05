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
    public int dragged_gameobjectSiblingIndex = -1;
    public int draggedParent_parent_sibling_index = -1;

    [HideInInspector] public Predmet[] predmeti_personal = new Predmet[20]; // seznam itemov, ubistvu inventorij
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

   
    internal InventorySlotPersonal[] personal_inventory_slots;  // predstavlajo slote v inventoriju, vsak drzi en item. 

    private DynamicCharacterAvatar avatar;
    internal NetworkPlayerCombatHandler combatHandler;
    //-------------------------------LOADOUT SLOTS-----------------------------
    internal InventorySlotLoadout loadout_head;
    internal InventorySlotLoadout loadout_chest;
    internal InventorySlotLoadout loadout_hands;
    internal InventorySlotLoadout loadout_legs;
    internal InventorySlotLoadout loadout_feet;
    internal InventorySlotLoadout loadout_backpack;//ni loadout item ubistvu. logika je cist locena ker je prioriteta da se backpack lahko cimlazje fukne dol. tle ga mam samo za izrisovanje v inventorij panel

    internal InventorySlotBar[] bar_slots;

    [HideInInspector] internal Predmet[] predmeti_hotbar;
    private List<Predmet> previous_combines_inventories = new List<Predmet>();

    [HideInInspector] internal Predmet head;
    [HideInInspector] internal Predmet chest;


    [HideInInspector] internal Predmet hands;
    [HideInInspector] internal Predmet legs;
    [HideInInspector] internal Predmet feet;

    public delegate void OnLoadoutChanged();
    public OnLoadoutChanged onLoadoutChangedCallback;


    [HideInInspector]
    internal Predmet backpack;
    private Camera c;



    public Transform backpackSpot; //tukaj se parenta backpack
    public panel_bar_handler barPanel;
    internal NetworkPlayerNeutralStateHandler neutralStateHandler;
    private NetworkPlayerStats stats;

    private List<PredmetRecepie> craftingQueue;
    private IEnumerator craftingRoutine;
    private int craftingTimeRemaining = 0;
    internal int draggedGameobjectParentSiblingIndex;

    internal int draggedGameobjectParent_parentSiblingIndex;
    internal int draggedGameobjectParent_parent_parentSiblingIndex;
    internal int draggedGameobjectParent_parent_parent_parentSiblingIndex;
    private List<string> slots_to_clear;
    private readonly int bar_slots_Length = 10;
    private readonly int personal_inventory_space=20;

    private void Start()
    {
        try
        {
            predmeti_hotbar = new Predmet[this.bar_slots.Length];
            predmeti_personal = new Predmet[personal_inventory_slots.Length];
        }catch(Exception e){
        //client je owner
        }

        this.craftingQueue = new List<PredmetRecepie>();
        this.combatHandler = GetComponent<NetworkPlayerCombatHandler>();
        this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        this.avatar = GetComponent<DynamicCharacterAvatar>();
        this.stats = GetComponent<NetworkPlayerStats>();
        this.slots_to_clear = null;
    }

    internal void on_UI_linked() {//klice se z UILogic.on_local_player_linked

    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        
        if (networkObject.IsOwner)
        {
            onItemChangedCallback += UpdateUI;    // Subscribe to the onItemChanged callback
            onItemChangedCallback += Display_new_items_on_canvas;
        }

        if (networkObject.IsServer || networkObject.IsOwner) {
            if(this.predmeti_hotbar==null)
                this.predmeti_hotbar = new Predmet[this.bar_slots_Length];
            if(this.predmeti_personal==null)
                this.predmeti_personal = new Predmet[personal_inventory_space];
        }

        onLoadoutChangedCallback += refresh_UMA_equipped_gear;//vsi rabjo vidt loadout

        

       // if (networkObject.IsOwner && networkObject.IsServer) instantiate_server_weapons_for_testing();
        if (networkObject.IsOwner) UpdateUI();//bugfix
    }

    void Display_new_items_on_canvas()
    {
        if (networkObject.IsOwner) {
            List<Predmet> all_items = combine_inventories();
            List<Predmet> razlika = get_difference_from_inventories(all_items, this.previous_combines_inventories);
            this.previous_combines_inventories = all_items;
            UILogic.Instance.OnInventoryChanged(razlika);
        }
    }

    private List<Predmet> get_difference_from_inventories(List<Predmet> all_items, List<Predmet> previous_combines_inventories)
    {
        List<Predmet> razlika = new List<Predmet>();

        foreach (Predmet p in all_items)
        {
            bool found = false;
            foreach (Predmet s in previous_combines_inventories)
            {
                if (p.item_id == s.item_id)
                {
                    if (p.quantity - s.quantity != 0)
                        razlika.Add(new Predmet(p.getItem(), p.quantity - s.quantity));
                    found = true;
                    break;
                }
            }
            //nismo najdli
            if(!found)razlika.Add(p);
        }
        return razlika;
    }

    /// <summary>
    /// naredi list vseh predmetov, sesteje njihove kvantitete in vrne nazaj. uporablja se samo za display novo dobljenih itemov
    /// </summary>
    /// <returns></returns>
    private List<Predmet> combine_inventories()
    {
        List<Predmet> sum = new List<Predmet>();

        //personal
        foreach (Predmet p in this.predmeti_personal) {
            if (p == null) continue;
            bool found = false;
            foreach (Predmet s in sum) {
                if (s.item_id == p.item_id)
                {
                    s.quantity += p.quantity;
                    found = true;
                    break;
                }
            }
            if(!found)sum.Add(new Predmet(p.getItem(), p.quantity));
        }
        //hotbar
        foreach (Predmet p in this.predmeti_hotbar)
        {
            if (p == null) continue;
            bool found = false;
            foreach (Predmet s in sum)
            {
                if (s.item_id == p.item_id)
                {
                    s.quantity += p.quantity;
                    found = true;
                    break;
                }
            }
            if (!found) sum.Add(new Predmet(p.getItem(), p.quantity));
        }
        //potential backpack
        if(this.backpack !=null)
            foreach (Predmet p in this.backpack_inventory.nci.predmeti)
            {
                bool found = false;
                if (p == null) continue;
                foreach (Predmet s in sum)
                {
                    if (s.item_id == p.item_id)
                    {
                        s.quantity += p.quantity;
                        found = true;
                        break;
                    }
                }
                if (!found) sum.Add(new Predmet(p.getItem(), p.quantity));
            }

        //LOADOUT!!!!!!!!!!!!!!

        if (this.head != null)
            sum.Add(new Predmet(this.head.getItem(), 1));

        if (this.chest != null)
            sum.Add(new Predmet(this.chest.getItem(), 1));

        if (this.hands != null)
            sum.Add(new Predmet(this.hands.getItem(), 1));

        if (this.legs != null)
            sum.Add(new Predmet(this.legs.getItem(),1));

        if (this.feet != null)
            sum.Add(new Predmet(this.feet.getItem(),1));


        return sum;
    }

    /// <summary>
    /// KO SE DATA NALOZI MORAMO NEKAK PAMETNO POSKRBET DA SE STVARI APPLAYAjo
    /// </summary>
    internal void OnPlayerDataLoaded()
    {
        Debug.Log("Applying Inventory.");
        if (networkObject.IsServer)
            sendNetworkUpdate(true, true);//server sporoci vsem kaj ima oblecen. mislm da je tot o za inventory ko se nalovda
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
        if (personal_inventory_slots == null) return;
        if (predmeti_personal.Length != personal_inventory_slots.Length) return;

        for (int i = 0; i < predmeti_personal.Length; i++)
        {
            if (predmeti_personal[i] != null)  // If there is an item to add
            {
                personal_inventory_slots[i].AddPredmet(this.predmeti_personal[i]);   // Add it
            }
            else
            {
                // Otherwise clear the slot
                personal_inventory_slots[i].ClearSlot();
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

        if (this.slots_to_clear == null)
        {
            this.slots_to_clear = new List<string>();
            foreach (string s in avatar.CurrentWardrobeSlots)
            {
                if (!s.Equals("Hair"))
                    this.slots_to_clear.Add(s);
            }
        }

        avatar.ClearSlots(this.slots_to_clear);

        if (this.head != null)
        {
            update_gear_on_player(this.head.getItem());
        }

        if (this.chest != null)
        {
            update_gear_on_player(this.chest.getItem());
        }

        if (this.hands != null)
        {
            update_gear_on_player(this.hands.getItem());
        }

        if (this.legs != null)
        {
            update_gear_on_player(this.legs.getItem());
        }

        if (this.feet != null)
        {
            update_gear_on_player(this.feet.getItem());
        }

        avatar.BuildCharacter();
    }

    private void update_gear_on_player(Item item)
    {
        if (item == null) return;
        if (item.UMARecipes.Length == 0) return;

        foreach (UMATextRecipe t in item.UMARecipes) {
            Debug.Log("this goes on slot: " + t.wardrobeSlot);
            avatar.SetSlot(t.wardrobeSlot, t.DisplayValue);//display value MORA BIT ENAK imanu datoteke/recepta sicer ne dela!!!!  tko da je treba popravt zmer k dodas nov armor!
        }
    }

    /// <summary>
    /// vrne true ce lahko pobere item (ce je prazn slot kjerkoli), ce je gear in ma plac u loadoutu, ce je stackable in ga loh stlacmo na stack k ze obstaja
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    internal bool canPickupPredmetFullStack(Predmet c)
    {
        if (c != null)
        {
            if (hasPersonalSpace() || hasBackpackSpace() || hasBarSpace()) return true;

            if (c.getItem().type == Item.Type.head && getHeadItem() == null ||
                c.getItem().type == Item.Type.chest && getChestItem() == null ||
                c.getItem().type == Item.Type.hands && getHandsItem() == null ||
                c.getItem().type == Item.Type.legs && getLegsItem() == null ||
                c.getItem().type == Item.Type.feet && getFeetItem() == null) return true;

            if (c.getItem().stackSize > 1) return CanBePlacedOnExistingStacksInFull(c);
        }
        else return true;
        return false;
    }

    private bool CanBePlacedOnExistingStacksInFull(Predmet p)
    {
        //poglej najprej invnetorij v while zanki
        int kol = p.quantity;
        //while (getNumberOfEligibleNonEmptyStacks_personalInventory(resp.item) > 0) { //gre itak cez vse stacke in proba dodat tko da itak ze vse ujame
        foreach (Predmet stack in this.predmeti_personal)
            if (stack != null)
                if (stack.getItem() != null)
                    if (stack.item_id==p.item_id)
                        if (stack.quantity < stack.getItem().stackSize)
                        {
                            kol -= stack.getItem().stackSize - stack.quantity;
                            if (kol <= 0) return true;
                        }
        //}
        //poglej backpack
        if (this.backpack != null)
            foreach (Predmet stack in this.backpack_inventory.nci.predmeti)
                if (stack != null)
                    if (stack.getItem() != null)
                        if (stack.item_id == p.item_id)
                            if (stack.quantity < stack.getItem().stackSize)
                            {
                                kol -= stack.getItem().stackSize - stack.quantity;
                                if (kol <= 0) return true;
                            }
        //poglej hotbar
        foreach (Predmet stack in this.predmeti_hotbar)
            if (stack != null)
                if (stack.getItem() != null)
                    if (stack.item_id == p.item_id)
                        if (stack.quantity < stack.getItem().stackSize)
                        {
                            kol -= stack.getItem().stackSize - stack.quantity;
                            if (kol <= 0) return true;
                        }

        return false;
    }

    internal void OnRemotePlayerDataSet()
    {
        if (onLoadoutChangedCallback != null)
            onLoadoutChangedCallback.Invoke();
        else
        {
            Debug.LogWarning("refreshing uma gear doesnt work for remote players. check this out what that is...");
            refresh_UMA_equipped_gear();
        }
    }

    private bool hasPersonalSpace()
    {
        foreach (Predmet p in this.predmeti_personal) if (p == null) return true;
        return false;
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
        if (i.getItem() == null) return null;
        Predmet r = i;
        switch (i.getItem().type)
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
        if (pobran_objekt.quantity > pobran_objekt.getItem().stackSize)
        {
            while (pobran_objekt.quantity > pobran_objekt.getItem().stackSize)
            {
                Predmet k = new Predmet(pobran_objekt.getItem(), pobran_objekt.getItem().stackSize, pobran_objekt.current_durabilty, pobran_objekt.creator);
                pobran_objekt.quantity -= pobran_objekt.getItem().stackSize;
                handleItemPickup(k, true);
            }
        }



        Predmet resp = try_to_upgrade_loadout(pobran_objekt);
        if (resp != null && resp.getItem().stackSize > 1)
            resp = tryToAddPredmetToExistingStack(resp);

        if (resp != null)
        {
            if ((resp.getItem().type == Item.Type.weapon || resp.getItem().type == Item.Type.shield || resp.getItem().type == Item.Type.ranged || resp.getItem().type == Item.Type.tool || resp.getItem().type == Item.Type.placeable) && hasBarSpace())
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
    internal Predmet tryToAddPredmetToExistingStack(Predmet resp)
    {
        //poglej najprej invnetorij v while zanki
        Predmet p = resp;
        //while (getNumberOfEligibleNonEmptyStacks_personalInventory(resp.item) > 0) { //gre itak cez vse stacke in proba dodat tko da itak ze vse ujame
        foreach (Predmet stack in this.predmeti_personal)
            if (stack != null)
                if (stack.getItem() != null)
                    if (stack.item_id==p.item_id)
                        if (stack.quantity < stack.getItem().stackSize)
                        {
                            p = stack.addQuantity(p);
                            if (p == null)
                                return null;
                        }
        //}
        //poglej backpack
        if (this.backpack != null)
            foreach (Predmet stack in this.backpack_inventory.nci.predmeti)
                if (stack != null)
                    if (stack.getItem() != null)
                        if (stack.getItem().Equals(p.getItem()))
                            if (stack.quantity < stack.getItem().stackSize)
                            {
                                p = stack.addQuantity(p);
                                if (p == null)
                                    return null;
                            }
        //poglej hotbar
        foreach (Predmet stack in this.predmeti_hotbar)
            if (stack != null)
                if (stack.getItem() != null)
                    if (stack.getItem().Equals(p.getItem()))
                        if (stack.quantity < stack.getItem().stackSize)
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
                if (p.getItem() != null)
                    if (p.getItem().Equals(item))
                        if (p.quantity < p.getItem().stackSize)
                            c++;
        return c;
    }

    private int getNumberOfEligibleNonEmptyStacks_backpack(Item item)
    {
        if (this.backpack == null) return 0;

        int c = 0;
        foreach (Predmet p in this.backpack_inventory.nci.predmeti)
            if (p != null)
                if (p.getItem() != null)
                    if (p.getItem().Equals(item))
                        if (p.quantity < p.getItem().stackSize)
                            c++;
        return c;
    }

    private int getNumberOfEligibleNonEmptyStacks_hotbar(Item item)
    {
        int c = 0;
        foreach (Predmet p in this.predmeti_hotbar)
            if (p != null)
                if (p.getItem() != null)
                    if (p.getItem().Equals(item))
                        if (p.quantity < p.getItem().stackSize)
                            c++;
        return c;
    }

    #region right click

    internal void OnRightClickPersonalInventory(GameObject g)//tole lahko potem pri ciscenju kode z malo preurejanja damo v uno tavelko metodo
    {
        if (UILogic.Instance.currentActiveContainer != null) {//personal -> chest
            NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

            InventorySlot from = g.GetComponent<InventorySlot>();
            int indexFrom = getIndexFromName(from.name);
            

            ncbh.localRequestPersonalToContainer(indexFrom, -1);
        }
        else
            handleInventorySlotOnDragDropEvent(null, g.transform, true);
    }

    internal void OnRightClickBackpack(GameObject g)//ce smo kliknli z desno na backpack. proba dat na loadout, ce ni prazn nrdi swap
    {
        if (networkObject.IsOwner)
        {
            if (UILogic.Instance.currentActiveContainer != null)
            {// -> chest
                NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

                InventorySlot from = g.GetComponent<InventorySlot>();
                int indexFrom = getIndexFromName(from.name);


                ncbh.localRequestBackpackToContainer(indexFrom, -1);
            }
            else
            {
                int index_backpack = getIndexFromName(gameObject.name);
                this.backpack_inventory.localPlayerRequestBackpackToLoadout(index_backpack);
            }
        }
    }

    internal int get_index_of_next_item_on_hotbar_ascending(int current_index)
    {
        if (current_index == -1) current_index = 0;
        else
        current_index = (current_index + 1) % predmeti_hotbar.Length;

        for (int i = 0; i < this.predmeti_hotbar.Length; i++) {
            if (predmeti_hotbar[(current_index + i) % predmeti_hotbar.Length] != null)
                return (current_index + i) % predmeti_hotbar.Length;
        }
        return -1;
    }

    internal int get_index_of_next_item_on_hotbar_descending(int current_index)
    {
        if (current_index < 1) current_index = 9;
        else
            current_index = (current_index - 1) % predmeti_hotbar.Length;

        for (int i = 0; i < this.predmeti_hotbar.Length; i++)
        {
            if (predmeti_hotbar[(current_index - i) % predmeti_hotbar.Length] != null)
                return (current_index - i) % predmeti_hotbar.Length;
        }
        return -1;
    }

    internal void OnRightClickBar(GameObject g)//tole lahko potem pri ciscenju kode z malo preurejanja damo v uno tavelko metodo
    {
        if (UILogic.Instance.currentActiveContainer != null)
        {// -> chest
            NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

            InventorySlot from = g.GetComponent<InventorySlot>();
            int indexFrom = getIndexFromName(from.name);


            ncbh.localRequestBarToContainer(indexFrom, -1);
        }
        //else
          //  handleInventorySlotOnDragDropEvent(null, g.transform, true);
    }

    internal void OnRightClickLoadout(GameObject g)//tole lahko potem pri ciscenju kode z malo preurejanja damo v uno tavelko metodo
    {
        if (UILogic.Instance.currentActiveContainer!=null)
        {//personal -> chest
            NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

            InventorySlot from = g.GetComponent<InventorySlot>();
            int indexFrom = (int)((InventorySlotLoadout)from).type;//getIndexFromName(from.name);


            ncbh.localRequestLoadoutToContainer(indexFrom, -1);
        }
        else
            handleInventorySlotOnDragDropEvent(null, g.transform, true);
    }

    internal void OnRightClickContainer(GameObject g)//tole lahko potem pri ciscenju kode z malo preurejanja damo v uno tavelko metodo
    {
        if (UILogic.Instance.currentActiveContainer != null)
        {//personal -> chest
            NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

            InventorySlot from = g.GetComponent<InventorySlot>();
            int indexFrom = getIndexFromName(from.name);


            ncbh.localRequestContainerToPersonal(indexFrom, -1);
        }
       
    }


    #endregion
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

    public bool SetPredmetLoadout(Predmet i)
    {//nevem zakaj vrne bool
        if (!networkObject.IsServer) { Debug.LogError("client dela stvar od serevrja!"); return false; }
        if (i == null) return false;

        Predmet r = i;
        switch (i.getItem().type)
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
    internal void setBarPredmet(Predmet b, int bar_index)
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

    /// <summary>
    /// vrne true ce smo ubil celotn stack - da se poslje barslotselectionupdate
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    internal bool reduceCurrentActivePlaceable(int index)
    {
        
        if (this.predmeti_hotbar[index].quantity > 1) this.predmeti_hotbar[index].quantity -= 1;
        else {
            predmeti_hotbar[index] = null;
            sendNetworkUpdate(true, false);
            neutralStateHandler.selected_index=-1;
            return true;
        }

        sendNetworkUpdate(true, false);
        return false;
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
            Debug.LogWarning("networkObject is null.");
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

    internal void localPlayerDropFromBarRequest(int v)
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_DROP_ITEM_FROM_BAR, Receivers.Server, v);
    }

    public override void DropItemFromBar(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            if (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
            {
                int index = args.GetNext<int>();

                Predmet p = this.predmeti_hotbar[index];
                if (p != null)
                    instantiateDroppedPredmet(p);

                this.predmeti_hotbar[index]=null;
                neutralStateHandler.sendBarUpdate();


                

            }
        }
    }

    internal void DropItemFromLoadout(Item.Type type, int index)
    {
        if (!networkObject.IsOwner) return;
        Camera c = Camera.main;
        networkObject.SendRpc(RPC_DROP_ITEM_FROM_LOADOUT_REQUEST, Receivers.Server, type.ToString(), index, c.transform.position + (c.transform.forward * 3), c.transform.forward);
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
        networkObject.SendRpc(RPC_INVENTORY_TO_LOADOUT_REQUEST, Receivers.Server, loadout_index, this.draggedItemParent.GetComponent<InventorySlotPersonal>().GetPredmet().getItem().type.ToString(), getIndexFromName(this.draggedItemParent.name));


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
                //Debug.LogError("Item type doesnt match anything. shits fucked yo");
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



            networkObject.SendRpc(p, RPC_SEND_PERSONAL_INVENTORY_UPDATE, this.predmeti_personal.ObjectToByteArray(),this.predmeti_hotbar.ObjectToByteArray());
        }

        if (loadout)
        {

            Predmet[] ar = new Predmet[8];
            ar[0] = this.head;
            ar[1] = this.chest;
            ar[2] = this.hands;
            ar[3] = this.legs;
            ar[4] = this.feet;
            ar[5] = this.backpack;


            networkObject.SendRpc(p, RPC_SEND_LOADOUT_UPDATE,ar.ObjectToByteArray()
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
            networkObject.SendRpc(RPC_SEND_PERSONAL_INVENTORY_UPDATE, Receivers.Owner, this.predmeti_personal.ObjectToByteArray(), this.predmeti_hotbar.ObjectToByteArray());
        }

        if (loadout)
        {
            Predmet[] ar = new Predmet[8];
            ar[0] = this.head;
            ar[1] = this.chest;
            ar[2] = this.hands;
            ar[3] = this.legs;
            ar[4] = this.feet;
            ar[5] = this.backpack;

            networkObject.SendRpc(RPC_SEND_LOADOUT_UPDATE, Receivers.All,ar.ObjectToByteArray()
                );

            if (onLoadoutChangedCallback != null)
                onLoadoutChangedCallback.Invoke();
        }
        if (neutralStateHandler == null) this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        if (Selected_PREDMET_IsNotInHotbar()) neutralStateHandler.ClearActiveWeapons();
    }

    private bool Selected_PREDMET_IsNotInHotbar()
    {


        if (combatHandler == null) combatHandler = GetComponent<NetworkPlayerCombatHandler>();
        if (neutralStateHandler == null) neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        int activeItem = -1;
        if (combatHandler.GetCurrentlyActiveWeapon() != null) activeItem = combatHandler.GetCurrentlyActiveWeapon().item_id;
        if (combatHandler.GetCurrentlyActiveRanged() != null) activeItem = combatHandler.GetCurrentlyActiveRanged().item_id;
        if (neutralStateHandler.activeTool != null) activeItem = neutralStateHandler.activeTool.item_id;
        if (neutralStateHandler.current_placeable_item != null) activeItem = neutralStateHandler.activePlaceable.item_id;

        int activeShield = -1;
        if (combatHandler.GetCurrentlyActiveShield() != null) activeShield = combatHandler.GetCurrentlyActiveShield().item_id;

        bool zamenjan_shield = false;
        bool zamenjan_item = false;
        if (neutralStateHandler.selected_index > -1)
        {
            if (predmeti_hotbar[neutralStateHandler.selected_index] == null && activeItem != -1)
            {
                zamenjan_item = true;
            }
            else if (predmeti_hotbar[neutralStateHandler.selected_index] != null)
                if (predmeti_hotbar[neutralStateHandler.selected_index].item_id != activeItem)
                    zamenjan_item = true;
        }
        if (neutralStateHandler.selected_index_shield > -1)
        {
            if (predmeti_hotbar[neutralStateHandler.selected_index_shield] == null && activeShield != -1)
            {
                zamenjan_shield = true;
            }
            else if (predmeti_hotbar[neutralStateHandler.selected_index_shield] != null)
                if (predmeti_hotbar[neutralStateHandler.selected_index_shield].item_id != activeShield)
                    zamenjan_shield = true;
        }
        if (zamenjan_shield || zamenjan_item)
        {
            //mormo poslat vsem da nj sinhronizirajo active item. - zunej te metode
            return true;
        }
        return false;
    }

    public override void SendPersonalInventoryUpdate(RpcArgs args)//dobi samo owner
    {
        //to bi mogu dobit samo owner in NOBEN drug, sicer je nrdit ESP hack najbolj trivialna stvar na planetu
        //Debug.Log(" personal inventory rpc receive: owner server id: " + GetComponent<NetworkPlayerStats>().server_id + " | networkId : " + networkObject.Owner.NetworkId);
        if (args.Info.SendingPlayer.NetworkId != 0) return;//ce ni poslov server al pa ce je prejeu en drug k owner(kar s eneb smel nrdit sploh!)



        //inventory
        this.predmeti_personal = args.GetNext<byte[]>().ByteArrayToObject<Predmet[]>();
        this.predmeti_hotbar = args.GetNext<byte[]>().ByteArrayToObject<Predmet[]>();

        //ce smo zarad armor standa povozil trenutno equippan weapon mormo to updejtat..
        if (neutralStateHandler == null) this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        if (Selected_PREDMET_IsNotInHotbar()) neutralStateHandler.ClearActiveWeapons();

        combatHandler.update_equipped_weapons();

        if (onLoadoutChangedCallback != null)
            onLoadoutChangedCallback.Invoke();
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();

    }

    public override void SendLoadoutUpdate(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId != 0 || networkObject.IsServer) return;

        Predmet[] ar = args.GetNext<byte[]>().ByteArrayToObject<Predmet[]>();
        this.head = ar[0];
        this.chest = ar[1];
        this.hands = ar[2];
        this.legs = ar[3];
        this.feet = ar[4];
        this.backpack = ar[5];


        if (onLoadoutChangedCallback != null)
            onLoadoutChangedCallback.Invoke();
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();

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
        if (!networkObject.IsServer) return;
        Predmet p = new Predmet(Mapper.instance.items[0]);
        int k = 0;
        foreach (Item i in Mapper.instance.items)
        {
            p = new Predmet(i, 1, 1000);
            instantiateDroppedPredmet(p, transform.position + Vector3.up * 2,( transform.forward*k++)*0.1f);
        }
    }

    internal void instantiateDroppedPredmet(Predmet p)
    {
        instantiateDroppedPredmet(p, transform.position + new Vector3(0, 1, 0), transform.forward);
    }

    internal void instantiateDroppedPredmet(Predmet p, Vector3 camera_vector, Vector3 camera_forward) // instantiate it when dropped - zapakiral v rpc da se poslje vseskup na server
    {
        if (!networkObject.IsServer) { Debug.LogError("instanciacija objekta k smo ga dropal ni na serverjvu!"); return; }


        networkObject.SendRpc(RPC_NETWORK_INSTANTIATION_SERVER_REQUEST, Receivers.Server, p.ObjectToByteArray(), camera_vector, camera_forward);
    }

    private int getNetworkIdFromInteractableObject(Item item)
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
        //obleke
        if (networkObject.IsServer)
            sendNetworkUpdate(false, true);//tole poslje vsem, ne samo temu k ga rab. optimiziacija ksnej.
    }

    public override void NetworkInstantiationServerRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) { Debug.LogError("instanciacija na clientu ne na serverju!"); return; }
        Predmet p = args.GetNext<byte[]>().ByteArrayToObject<Predmet>();

        Vector3 pos = args.GetNext<Vector3>();
        Vector3 dir = args.GetNext<Vector3>();
        int net_id = getNetworkIdFromInteractableObject(p.getItem());
        if (net_id != -1)
        { //item is interactable object
            Interactable_objectBehavior b = NetworkManager.Instance.InstantiateInteractable_object(net_id, pos);
            //apply force on clients, sets predmet
            b.gameObject.GetComponent<Interactable>().setStartingInstantiationParameters(p, pos, dir);
        }

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

        int inv_index = args.GetNext<int>();
        Predmet inventory_item = this.predmeti_personal[inv_index];//poisce item glede na id-ju slota. id dobi z rpc k ga poda z imena tega starsa

        //
        if (inventory_item.getItem().type == Item.Type.head ||//cce je item za u loadout. ce je backpack ga itak nemormo dobit v inventorij ??
            inventory_item.getItem().type == Item.Type.chest ||
            inventory_item.getItem().type == Item.Type.hands ||
            inventory_item.getItem().type == Item.Type.legs ||
            inventory_item.getItem().type == Item.Type.feet) { }
        else
            return;
        


            Predmet loadout_item = null;
        loadout_item = PopLoadoutItem(type);

        


        if (SetPredmetLoadout(inventory_item))//to bo zmer slo cez ker je slot ze prazen. smo ga izpraznli z popom. vrne true ce je item biu valid za nek loadout slot.
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
                if (t == this.predmeti_personal[index].getItem().type)//ce se item ujema naj se zamenja
                {

                    Predmet inventory_item = this.predmeti_personal[index];
                    if (loadout_item != null)
                    {
                        Add(loadout_item, index);
                        SetPredmetLoadout(inventory_item);
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
            if (true)// Vector3.Distance(transform.position, resource_obj.transform.position) < 3f tole je prej pisal not ampak ker je hit detection na serverju je neklak pointless in sam zjebe stvar pr nabiranju lesa
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

            //if (neutralStateHandler.isNotSelected(bar_index, -1))
            //{
                if (this.predmeti_personal[inv_index] != null)
                {

                    Predmet b = popPersonalPredmet(inv_index);
                    Predmet i = popBarPredmet(bar_index);

                    setBarPredmet(b, bar_index);
                    setPersonalIventoryPredmet(i, inv_index);

                    sendNetworkUpdate(true, false);

                }
            //}
        }
    }



    public override void BarToPersonalRequest(RpcArgs args)//bar, personal
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            int inv_index = args.GetNext<int>();
            int bar_index = args.GetNext<int>();
            //if (neutralStateHandler.isNotSelected(bar_index, -1))
            //{
                if (this.predmeti_hotbar[bar_index] != null)
                {
                    Predmet b = popPersonalPredmet(inv_index);
                    Predmet i = popBarPredmet(bar_index);
                    setBarPredmet(b, bar_index);
                    setPersonalIventoryPredmet(i, inv_index);
                    sendNetworkUpdate(true, false);
                }
            //}
        }

    }

    public override void BarToBarRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            int a = args.GetNext<int>();
            int b = args.GetNext<int>();

            //if (neutralStateHandler.isNotSelected(a, b))
            //{ //TODO ce je trenutno izbran item je blokiran pri menjavi. spremenit tko da lhako menja ampak se zamenja potem tud index v neutralStateHandlerju
                Predmet x = popBarPredmet(a);
                setBarPredmet(popBarPredmet(b), a);
                setBarPredmet(x, b);
                sendNetworkUpdate(true, false);
            //}
        }

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
        if (this.backpack != null)
            this.backpack_inventory.requestUIUpdate();
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

    internal int getQuantityOfItemOnPlayer(Item item)
    {
        //prevert inv, backpack, bar

        int q = 0;

        foreach (Predmet p in this.predmeti_personal)
            if (p != null)
                if (p.getItem().Equals(item))
                    q += p.quantity;

        if(this.backpack!=null)
            foreach (Predmet p in this.backpack_inventory.nci.predmeti)
                if (p != null)
                    if (p.getItem().Equals(item))
                        q += p.quantity;

        foreach (Predmet p in this.predmeti_hotbar)
            if (p != null)
                if (p.getItem().Equals(item))
                    q += p.quantity;

        return q;
    }

    internal void localStartCraftingRequest(Item i, int current, int skin)
    {
        //Debug.Log("sending crafting request! " + i.Display_name + " x" + current);
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
            if(quantity>0)
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
    {   //zaenkrat lahko iteme dobimo samo od personal inventorija, backpacka al pa hotbara. 
        //vemo da mamo dovolj itemov nekje ker smo to pregledal predn smo skocil v to metodo kar pomen da nerabmo? vracat itemov ker naceloma nesmemo failat
        //loop cez vse matse in jih brisemo iz inventorija dokler ne zbrisemo zadost.
        for (int i = 0; i < p.ingredients.Length; i++)
        {
            int q = p.ingredient_quantities[i];//tolkle moramo zbrisat iz nekje
            DeleteIngredients(p.ingredients[i], q);
        }
        Predmet a_baby = new Predmet(p.Product, p.final_quantity, p.Product.Max_durability, stats.playerName);
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
                
                if (this.predmeti_hotbar[i] != null)
                    if (this.predmeti_hotbar[i].getItem().Equals(item))
                    {
                        if (this.predmeti_hotbar[i].quantity <= q)
                        {
                            //pobral bomo cel stack.
                            q -= this.predmeti_hotbar[i].quantity;
                            this.predmeti_hotbar[i] = null;
                        }
                        else
                        {//poberemo samo del stacka in smo zakljucli
                            this.predmeti_hotbar[i].quantity -= q;
                            return;
                        }
                    }
            }

        //pol bi sli spraznit backpack -------------------------------------TLE MOGOCE PRIDE DO BUGGA KER NEVEM LIH KKO SE PRENASA ARRAY OBJEKTOV AL JE COPY AL JE REFERENCA..
        if (q > 0 && this.backpack!=null)
        {
            
            for (int i = 0; i < this.backpack_inventory.nci.predmeti.Length; i++)
            {
                
                if (this.backpack_inventory.nci.predmeti[i] != null)
                    if (this.backpack_inventory.nci.predmeti[i].getItem().Equals(item))
                    {
                        if (this.backpack_inventory.nci.predmeti[i].quantity <= q)
                        {
                            //pobral bomo cel stack.
                            q -= this.backpack_inventory.nci.predmeti[i].quantity;
                            this.backpack_inventory.nci.predmeti[i] = null;
                        }
                        else
                        {//poberemo samo del stacka in smo zakljucli
                            this.backpack_inventory.nci.predmeti[i].quantity -= q;
                            return;
                        }
                    }
            }
        }

        if (q > 0)//najprej bi spraznil hotbar
            for (int i = 0; i < this.predmeti_personal.Length; i++)
            {
                
                if (this.predmeti_personal[i] != null)
                    if (this.predmeti_personal[i].getItem().Equals(item))
                    {
                        if (this.predmeti_personal[i].quantity <= q)
                        {
                            //pobral bomo cel stack.
                            q -= this.predmeti_personal[i].quantity;
                            this.predmeti_personal[i] = null;
                        }
                        else
                        {//poberemo samo del stacka in smo zakljucli
                            this.predmeti_personal[i].quantity -= q;
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
            UILogic.Instance.GetComponentInChildren<craftingPanelHandler>().updateCraftingQueueWithServerData(r, args.GetNext<int>());
        }


    }
    internal void localSendCraftingQueueUpdateRequest()
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_CRAFTING_QUEUE_UPDATE_REQUEST, Receivers.Server);
        }

    public override void CraftingQueueUpdateRequest(RpcArgs args)
    {
        networkObject.SendRpc(args.Info.SendingPlayer, RPC_ITEM_CRAFTING_RESPONSE, getItemIdsFromCraftingQueueNetworkString(this.craftingQueue), this.craftingTimeRemaining);
    }



    #endregion


    #region containers
    internal void onContainerOpen(NetworkContainer container, Predmet[] predmeti)
    {
        if (UILogic.Instance.allows_UI_opening)
        {
            if (UILogic.Instance.currently_openened_container.Equals(container))
            {
                UILogic.Instance.setContainerPanelActiveForContainer(predmeti);
                UILogic.Instance.setCurrentActiveContainer(container);
            }
        }
    }
    internal void onContainerOpenWithUpkeep(NetworkContainer container, Predmet[] predmeti, int a,int b, int c, int d)
    {
        if (UILogic.Instance.allows_UI_opening)
        {
            if (UILogic.Instance.currently_openened_container.Equals(container))
            {
                UILogic.Instance.setContainerPanelActiveForFlagContainer(predmeti, a,b,c,d);
                UILogic.Instance.setCurrentActiveContainer(container);
            }
        }
    }

    //--------Te metode tipa handleBackpackToContainer(RectTransform invSlot) se klicejo direkt z ItemDropHandler.OnDrop(PointerEventData eventData)

    internal void handleBackpackToContainer(RectTransform invSlot)
    {
        NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int indexTo = getIndexFromName(to.name);
        ncbh.localRequestBackpackToContainer(indexFrom, indexTo);
    }

    internal void handleContainerToBackpack(RectTransform invSlot)
    {
        NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int indexTo = getIndexFromName(to.name);
        ncbh.localRequestContainerToBackpack(indexFrom, indexTo);
    }

    internal void handleContainerToPersonal(RectTransform invSlot)
    {
        NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int indexTo = getIndexFromName(to.name);
        ncbh.localRequestContainerToPersonal(indexFrom, indexTo);
    }

    internal void handleBarToContainer(RectTransform invSlot)
    {
        NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int indexTo = getIndexFromName(to.name);
        ncbh.localRequestBarToContainer(indexFrom, indexTo);
    }

    internal void handleContainerToBar(RectTransform invSlot)
    {
        NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int indexTo = getIndexFromName(to.name);
        ncbh.localRequestContainerToBar(indexFrom, indexTo);
    }

    internal void handleLoadoutToContainer(RectTransform invSlot)
    {
        NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int indexTo = getIndexFromName(to.name);
        ncbh.localRequestLoadoutToContainer(indexFrom, indexTo);
    }

    internal void handleContainerToLoadout(RectTransform invSlot)
    {
        NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        //int indexTo = getIndexFromName(to.name);
        ncbh.localRequestContainerToLoadout(indexFrom, -1);
    }

    internal void handlePersonalToContainer(RectTransform invSlot)
    {
        //rabmo dobit taprav container kterga mamo zdle izbranga. ko ga dobimo klicemo z containerja metodo za manipulacijo z itemi.
        //najlazje bo to dobit kr z UILOgic, ker smo nastavli trenutni container ob odprtju containerja.
        NetworkContainer ncbh= UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int indexTo = getIndexFromName(to.name);

        ncbh.localRequestPersonalToContainer(indexFrom, indexTo);
    }

    internal void handleContainerToContainer(RectTransform invSlot)
    {
        NetworkContainer ncbh = UILogic.Instance.currentActiveContainer;

        InventorySlot from = this.draggedItemParent.GetComponent<InventorySlot>();
        int indexFrom = getIndexFromName(from.name);
        InventorySlot to = invSlot.GetComponent<InventorySlot>();
        int indexTo = getIndexFromName(to.name);

        ncbh.localRequestContainerToContainer(indexFrom, indexTo);
    }

    internal void localPlayerDropItemFromContainerRequest(int v)
    {
        UILogic.Instance.currentActiveContainer.localRequestDropItemContainer(v);
    }

    #endregion

    /// <summary>
    /// cancels all current crafts and puts the items in inventory
    /// </summary>
    internal void cancelAllCrafting_server()
    {
        //throw new NotImplementedException();
    }

    internal void localPlayerSplitStackRequest(InventorySlot currentSlot, int amount)
    {
        Debug.Log("Local player requested stack splitting. checking local legitimacy and sending to server");
        if (networkObject.IsOwner)
        {
            if (currentSlot is InventorySlotPersonal)//ce je inventroySlot in ni hotbarPanel
                networkObject.SendRpc(RPC_SPLIT_REQUEST_PERSONAL_INVENTORY, Receivers.Server, currentSlot.index, amount);
            else if (currentSlot is InventorySlotBar)
                networkObject.SendRpc(RPC_SPLIT_REQUEST_BAR_INVENTORY, Receivers.Server, currentSlot.index, amount);
            else if (currentSlot is InventorySlotBackpack)
                this.backpack_inventory.localPlayerSplitRequest(currentSlot, amount);

        }
    }

    public override void SplitRequestPersonalInventory(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            ServerSplitStackPersonalInventory(args.GetNext<int>(), args.GetNext<int>());
        
        }
    }

    /// <summary>
    /// splits the stack on index i by amount of amount
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    private void ServerSplitStackPersonalInventory(int i, int amount)
    {
        if (!hasBarSpace() && !hasBackpackSpace() && !hasPersonalSpace()) { 
            
            return;
        }

        if (this.predmeti_personal[i] != null) {
            if (this.predmeti_personal[i].quantity >= amount) {

                Predmet original = this.predmeti_personal[i];
                Predmet new_stack = new Predmet(original);
                original.quantity -= amount;
                if (original.quantity == 0) original = null;
                new_stack.quantity = amount;

                //i se prepise ker ga ne rabmo
                ///najprej personal inventory
                bool placed = false;
                for (i = 0; i < this.predmeti_personal.Length; i++) {
                    if (this.predmeti_personal[i] == null)
                    {
                        predmeti_personal[i] = new_stack;
                        placed = true;
                        break;
                    }
                }

                //nato backpack ce ga imamo
                if (!placed)
                    if (this.backpack != null)
                        for (i = 0; i < this.backpack_inventory.nci.predmeti.Length; i++) {
                            if (this.backpack_inventory.nci.predmeti[i] == null)
                            {
                                this.backpack_inventory.nci.predmeti[i] = new_stack;
                                placed = true;
                                break;
                            }
                        }
                //nato se hotbar se ima plac
                if (!placed)
                    for (i = 0; i < this.predmeti_hotbar.Length; i++)
                        if (this.predmeti_hotbar[i] == null)
                        {
                            this.predmeti_hotbar[i] = new_stack;
                            placed = true;
                            break;
                        }


                if (!placed)//revert kind of
                {
                    Debug.LogError("newly created stack was not placed. This is not possible because we checked for space beforehand.");
                    if (original != null)
                        original.addQuantity(new_stack);
                    else
                        this.predmeti_personal[i] = new_stack;
                    new_stack = null;
                }

                sendNetworkUpdate(true,false);
            }
        }
    }

    public override void SplitRequestBarInventory(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            int i = args.GetNext<int>();
            int amount = args.GetNext<int>();
            if (!hasBarSpace() && !hasBackpackSpace() && !hasPersonalSpace())
            {

                return;
            }

            if (this.predmeti_hotbar[i] != null)
            {
                if (this.predmeti_hotbar[i].quantity >= amount)
                {

                    Predmet original = this.predmeti_hotbar[i];
                    Predmet new_stack = new Predmet(original);
                    original.quantity -= amount;

                    if (original.quantity == 0) original = null;

                    new_stack.quantity = amount;

                    //i se prepise ker ga ne rabmo
                    bool placed = false;
                    for (i = 0; i < this.predmeti_hotbar.Length; i++)
                        if (this.predmeti_hotbar[i] == null)
                        {
                            this.predmeti_hotbar[i] = new_stack;
                            placed = true;
                            break;
                        }

                    /// personal inventory
                    if (!placed)
                        for (i = 0; i < this.predmeti_personal.Length; i++)
                        {
                            if (this.predmeti_personal[i] == null)
                            {
                                predmeti_personal[i] = new_stack;
                                placed = true;
                                break;
                            }
                        }

                    //nato backpack ce ga imamo
                    if (!placed)
                        if (this.backpack != null)
                            for (i = 0; i < this.backpack_inventory.nci.predmeti.Length; i++)
                            {
                                if (this.backpack_inventory.nci.predmeti[i] == null)
                                {
                                    this.backpack_inventory.nci.predmeti[i] = new_stack;
                                    placed = true;
                                    break;
                                }
                            }


                    if (!placed)//revert kind of
                    {
                        Debug.LogError("newly created stack was not placed. This is not possible because we checked for space beforehand.");
                        if (original != null)
                            original.addQuantity(new_stack);
                        else
                            this.predmeti_hotbar[i] = new_stack;
                        new_stack = null;
                    }

                    sendNetworkUpdate(true, false);
                }
            }
        }
    }
}
