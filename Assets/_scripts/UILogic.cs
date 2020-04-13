using System;
using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// skripta skrbi za celotno logiko kar se tice ui-a. izjema je radial menu, canvas za healthbar, team panel pa take fore. cez ima guild, inventorij, pa take stvari k jih player lahko sam odpira
/// </summary>
public class UILogic : MonoBehaviour
{
    public GameObject guildModificationPanel;
    public GameObject GuildPanel;
    public GameObject inventoryPanel;
    public GameObject crafting_panel;
    public GameObject chatInput;
    public GameObject containerPanel;
    public GameObject panel_durability;
    public GameObject trebuchet_rotation_panel;
    public backpack_local_panel_handler backpack_local_panel_handler;

    public bool hasOpenWindow=false;
    public NetworkGuildManager ngm;
    public static NetworkPlayerInventory local_npi;

    public GameObject container_panel_10_slots;
    public GameObject container_panel_20_slots;
    public GameObject container_panel_40_slots;
    public GameObject container_panel_80_slots;

    public GameObject container_panel_10_slots_with_upkeep;

    public Text wood_upkeep_label;
    public Text stone_upkeep_label;
    public Text iron_upkeep_label;
    public Text gold_upkeep_label;

    public Text GuildModificationName;
    public Text GuildModificationTag;
    public Text GuildModificationColor;

    public NetworkContainer currently_openened_container = null;
    public NetworkPlaceable placeable_for_durability_check = null;

    private GameObject active_container_panel;
    public GameObject[] panelsPredmetiContainer;
    internal NetworkContainer currentActiveContainer;//tole se posreduje npi-ju med premikanjem predmetov. z npi v tole in obratno

    private NetworkSiegeTrebuchet active_trebuchet = null;
    public GameObject Button_trebuchet_rotation;
    public Slider slider_trebuchet_rotation;
    public InputField input_trebuchet_rotation;
    private Quaternion original_trebuchet_rotation;

    bool chatActive = false;

    public bool allows_UI_opening = false;

    public static UILogic Instance;
    public static GameObject localPlayerGameObject;
    public static panel_guild_handler PanelGuildHander;
    public static decicions_handler_ui DecisionsHandler;
    public static local_team_panel_handler TeamPanel;
    public Interactable_radial_menu interactable_radial_menu;

    public MenuControllerInGame menuController;


    //----------------za linkat z npi
    public InventorySlotPersonal[] personal_inventory_slots;
    //-------------------------------LOADOUT SLOTS-----------------------------
    public InventorySlotLoadout loadout_head;
    public InventorySlotLoadout loadout_chest;
    public InventorySlotLoadout loadout_hands;
    public InventorySlotLoadout loadout_legs;
    public InventorySlotLoadout loadout_feet;
    public InventorySlotLoadout loadout_backpack;
    public InventorySlotBar[] bar_slots;
    public GameObject menuBasicButtonsPanel;

    public GameObject direction_arrow_up;
    public GameObject direction_arrow_right;
    public GameObject direction_arrow_down;
    public GameObject direction_arrow_left;

    private void Start()
    {
        if (UILogic.Instance != null) Destroy(this); else UILogic.Instance = this;
        UILogic.PanelGuildHander = this.GuildPanel.GetComponent<panel_guild_handler>();
        UILogic.DecisionsHandler = GetComponentInChildren<decicions_handler_ui>();
        UILogic.TeamPanel = GetComponentInChildren<local_team_panel_handler>();
       
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            handleEscapePressed();
        }

        if (Input.GetKeyDown(KeyCode.Return)) {
            //djmo fokusirat not da loh pisemo ke
            if (chatActive)
            {
                chatActive = false;
                GetComponentInChildren<ChatManager>().SendMessage();
            }
            else
            {
                chatActive = true;
                chatInput.GetComponent<InputField>().Select();
                GetComponentInChildren<ChatManager>().onInputFieldSelected();
                GetComponentInParent<NetworkPlayerMovement>().lockMovement = true;
                GetComponentInParent<player_camera_handler>().lockCamera = true;
            }
            
        }

        if (Input.GetButtonDown("Guild") && !chatInput.GetComponent<InputField>().isFocused)
        {
            bool was_active = this.GuildPanel.activeSelf;
            ClearAll();
            if (was_active)
            {
            }
            else {
                this.GuildPanel.SetActive(true);
                this.hasOpenWindow = true;
                NetworkGuildManager.Instance.SetMemberPanel(true);
                NetworkGuildManager.Instance.localPlayerGuildInfoUpdateRequest();
                enableMouse();
            }  
        }

        if (Input.GetButtonDown("Inventory") && !chatInput.GetComponent<InputField>().isFocused)
        {
            bool was_active = this.inventoryPanel.activeSelf;
            ClearAll();
            if (was_active)
            {
            }
            else {
                showInventory();


            }
            //guild modificationPanel se odpre z button eventa na guild managerju...
            
            
        }
        if (Input.GetButtonDown("Crafting") && !chatInput.GetComponent<InputField>().isFocused)
        {
            bool was_active = this.crafting_panel.activeSelf;
            ClearAll();
            if (was_active)
            {
                //nc
            }
            else
            {
                this.crafting_panel.SetActive(true);
                this.hasOpenWindow = true;
                enableMouse();
            }
        }
    }

    internal void OnWeaponDirectionChanged(int current_direction)
    {

        this.direction_arrow_up.SetActive(false);
        this.direction_arrow_down.SetActive(false);
        this.direction_arrow_left.SetActive(false);
        this.direction_arrow_right.SetActive(false);
        if (Prefs.showDirectionalArrow)
        {
            if (current_direction == 0) this.direction_arrow_up.SetActive(true);
            if (current_direction == 1) this.direction_arrow_right.SetActive(true);
            if (current_direction == 2) this.direction_arrow_down.SetActive(true);
            if (current_direction == 3) this.direction_arrow_left.SetActive(true);
        }
        
    }

    internal bool isRadialMenuOpen()
    {
        return this.interactable_radial_menu.transform.GetChild(0).gameObject.activeSelf;
    }

    internal static void set_local_player_gameObject(GameObject player)
    {
        UILogic.localPlayerGameObject = player;
        UILogic.local_npi = player.GetComponent<NetworkPlayerInventory>();
        UILogic.Instance.on_local_player_linked();
    }

    private void on_local_player_linked() {

        //itemDrophandlers
        foreach (ItemDropHandler d in GetComponentsInChildren<ItemDropHandler>()) d.link_local_player();
        //itemDragHandlers
        foreach (ItemDragHandler d in GetComponentsInChildren<ItemDragHandler>()) d.link_local_player();
        //npi linkage wwith slots
        


            //----------------za linkat z npi
        UILogic.local_npi.personal_inventory_slots = this.personal_inventory_slots;
        UILogic.local_npi.loadout_head = this.loadout_head;
        UILogic.local_npi.loadout_chest = this.loadout_chest;
        UILogic.local_npi.loadout_hands = this.loadout_hands;
        UILogic.local_npi.loadout_legs = this.loadout_legs;
        UILogic.local_npi.loadout_feet = this.loadout_feet;
        UILogic.local_npi.loadout_backpack = this.loadout_backpack;
        UILogic.local_npi.bar_slots = this.bar_slots;
        UILogic.local_npi.on_UI_linked();
    }

    private void handleEscapePressed()
    {
        //ce je odprto ksno okno ga zapri, sicer prikaz main menu
        if (this.hasOpenWindow && (!menuController.transform.parent.gameObject.activeSelf || this.menuBasicButtonsPanel.activeSelf)) ClearAll();
        else {
            //prikazat main menu
            set_main_menu_state(true);
        }
    }

    internal void showGuildModificationPanel(bool b, NetworkGuildManager ngm)
    {
        this.guildModificationPanel.SetActive(b);
        if (b)//??
        {
            if (this.GuildPanel.activeSelf) ngm.SetMemberPanel(false);
            this.GuildPanel.SetActive(false);
            this.inventoryPanel.SetActive(false);
            this.hasOpenWindow = true;
            UILogic.localPlayerGameObject.GetComponent<NetworkPlayerMovement>().lockMovement = true;
        }
        else {
            ClearAll();
        }

    }
    private void set_main_menu_state(bool visible) {
        if (visible) {
            this.hasOpenWindow = true;
            enableMouse();
        }
        this.menuController.transform.parent.gameObject.SetActive(visible);
    }

    internal void show_trebuchet_rotation_panel(NetworkSiegeTrebuchet trebuchet) {
        this.active_trebuchet = trebuchet;
        this.original_trebuchet_rotation = trebuchet.get_rotation_of_platform(); ;
        this.trebuchet_rotation_panel.SetActive(true);
        this.hasOpenWindow = true;
        enableMouse();
    }

    public void on_rotation_set_button_clicked() {
        this.active_trebuchet.local_player_change_rotation_request();
        ClearAll();

    }

    public void on_rotation_slider_changed() {//nek bug je da se klice tole takoj na startu skripte pa nevem zakaj. mogoce se na sliderju ob startu nastavjo vrednosti ins e tole sprozi i guess.
        //get float
        float add_rotation = this.slider_trebuchet_rotation.value;
        //update transform rotation
        if (this.active_trebuchet == null) return;
        if (this.active_trebuchet.platform == null) return;
        this.active_trebuchet.platform.rotation = Quaternion.Euler(this.active_trebuchet.platform.rotation.eulerAngles.x,this.original_trebuchet_rotation.eulerAngles.y+add_rotation, this.active_trebuchet.platform.rotation.eulerAngles.z);
        //set value to input
        this.input_trebuchet_rotation.text = add_rotation + " degrees";
        Debug.Log("rotation slider changed");
    }

    public void on_rotation_input_changed()
    {
        //get float
        float add_rotation = 500;
        float.TryParse(this.input_trebuchet_rotation.text, out add_rotation);
        if (add_rotation == 500) return;
        //update transform rotation
        this.active_trebuchet.platform.rotation = Quaternion.Euler(this.active_trebuchet.platform.rotation.eulerAngles.x, this.original_trebuchet_rotation.eulerAngles.y + add_rotation, this.active_trebuchet.platform.rotation.eulerAngles.z);
        //set value to slider ?
        this.slider_trebuchet_rotation.value=add_rotation;
        Debug.Log("rotation input changed");
    }

    public void enableMouse() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableMouse() {
        if (this.hasOpenWindow || this.active_container_panel != null) { Debug.Log("NOT disabling mouse curson because we still have open windows!."); return; }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ClearAll() {
        this.active_trebuchet = null;
        this.trebuchet_rotation_panel.SetActive(false);
        this.inventoryPanel.SetActive(false);
        this.guildModificationPanel.SetActive(false);
        if (this.GuildPanel.activeSelf) ngm.SetMemberPanel(false);
        this.GuildPanel.SetActive(false);
        this.crafting_panel.SetActive(false);
        this.crafting_panel.SetActive(false);
        clearContainerPanel();
        this.hasOpenWindow = false;
        this.currentActiveContainer = null;
        this.panelsPredmetiContainer = null;
        

        this.allows_UI_opening = false;
        this.currently_openened_container = null;

        clear_placeable_durability_lookup();

        NetworkGuildManager.Instance.SetMemberPanel(false);

        DisableMouse();
        set_main_menu_state(false);
        UILogic.localPlayerGameObject.GetComponent<NetworkPlayerAnimationLogic>().hookChestRotation = true;
        UILogic.localPlayerGameObject.GetComponent<NetworkPlayerMovement>().lockMovement = false;
        UILogic.localPlayerGameObject.GetComponent<player_camera_handler>().lockCamera = false;
        this.input_trebuchet_rotation.text = "0 degrees";
    }


    private void showInventory()
    {
        this.inventoryPanel.SetActive(true);
        local_npi.requestLocalUIUpdate();
        this.hasOpenWindow = true;
        enableMouse();
    }

    internal void clearContainerPanel() {
        if(this.active_container_panel!=null)
            if (this.active_container_panel.activeSelf) {
                clearChestPanelPredmeti();
                this.active_container_panel.SetActive(false);
                this.active_container_panel = null;
                this.panelsPredmetiContainer = null;
                clear_upkeep_values();
            }
        this.currentActiveContainer = null;
    }

    internal void setContainerPanelActiveForContainer(Predmet[] predmeti)
    {
        if (this.active_container_panel == null)//ce je samo updejt itemov skipamo inicializacijo panele
        {
            showInventory();
            this.containerPanel.SetActive(true);
            this.panelsPredmetiContainer = new GameObject[predmeti.Length];

            //bi blo tle mogoce fajn nrdit prefabe ui elementov za razlicne cheste al pa crafting statione? pa sam nalimas gor kar rabs
            if (predmeti.Length == 10)
            {
                this.active_container_panel = this.container_panel_10_slots;
            }
            else if (predmeti.Length == 20)
            {
                this.active_container_panel = this.container_panel_20_slots;
            }
            else if (predmeti.Length == 40)
            {
                this.active_container_panel = this.container_panel_40_slots;
            }
            else if (predmeti.Length == 80)
            {
                this.active_container_panel = this.container_panel_80_slots;
            }
            this.active_container_panel.SetActive(true);

            for (int i = 0; i < this.active_container_panel.transform.childCount; i++)
            {
                this.panelsPredmetiContainer[i] = this.active_container_panel.transform.GetChild(i).gameObject;
            }
        }

        UpdateActiveChestPanel(predmeti);

    }


    internal void setContainerPanelActiveForFlagContainer(Predmet[] predmeti, int a, int b, int c,int d )
    {
        if (this.active_container_panel == null)//ce je samo updejt itemov skipamo inicializacijo panele
        {
            showInventory();
            this.containerPanel.SetActive(true);
            this.panelsPredmetiContainer = new GameObject[predmeti.Length];
            
            this.active_container_panel = this.container_panel_10_slots_with_upkeep;
            
            this.active_container_panel.SetActive(true);

            for (int i = 0; i < this.active_container_panel.transform.childCount; i++)//hacky
            {
                if(this.active_container_panel.transform.GetChild(i).gameObject.GetComponent<InventorySlotContainer>()!=null)
                    this.panelsPredmetiContainer[i] = this.active_container_panel.transform.GetChild(i).gameObject;
            }
        }
        update_upkeep_values(a,b,c,d);
        UpdateActiveChestPanel(predmeti);
        

    }

    internal void hide_radial_menu()
    {
        this.interactable_radial_menu.hide_radial_menu();
    }

    private void update_upkeep_values(int a, int b, int c, int d) {
        this.wood_upkeep_label.text = "× " + a;
        this.stone_upkeep_label.text = "× " + b;
        this.iron_upkeep_label.text = "× " + c;
        this.gold_upkeep_label.text = "× " + d;
    }
    private void clear_upkeep_values() {
        this.wood_upkeep_label.text = "×: " + 0;
        this.stone_upkeep_label.text = "×: " + 0;
        this.iron_upkeep_label.text = "×: " + 0;
        this.gold_upkeep_label.text = "×: " + 0;
    }


    private void UpdateActiveChestPanel(Predmet[] predmeti)
    {
  
        if (this.active_container_panel != null)
            for (int i = 0; i < this.panelsPredmetiContainer.Length; i++) {
                if(this.panelsPredmetiContainer[i].GetComponent<InventorySlotContainer>()!=null)
                    this.panelsPredmetiContainer[i].GetComponent<InventorySlotContainer>().AddPredmet(predmeti[i]);
            }
    }

    private void clearChestPanelPredmeti() {
        if (this.active_container_panel != null)
            for (int i = 0; i < this.panelsPredmetiContainer.Length; i++)
            {
                if (this.panelsPredmetiContainer[i].GetComponent<InventorySlotContainer>() != null)
                    this.panelsPredmetiContainer[i].GetComponent<InventorySlotContainer>().AddPredmet(null);
            }
    }

    internal void setCurrentActiveContainer(NetworkContainer container)
    {
        this.currentActiveContainer = container;
    }

    public void OnGuildModificationButtonClick() {
        ClearAll();
        enableMouse();
        NetworkGuildManager.Instance.OnButtonModifyClick();
        
    }

    internal void setup_placeable_durability_lookup(NetworkPlaceable p)
    {
        this.placeable_for_durability_check = p;//samo anredi referenco da jo lahko primerja, ko dobi odgovor z serverja
    }

    internal void clear_placeable_durability_lookup()
    {
        this.placeable_for_durability_check = null;
        this.panel_durability.SetActive(false);
    }

    internal void try_drawing_durability_for_placeable(NetworkPlaceable p)
    {
        if (this.placeable_for_durability_check == p) {
            this.panel_durability.SetActive(true);
            this.panel_durability.GetComponentInChildren<Text>().text = "" + p.p.current_durabilty + " / " + p.p.item.Max_durability;
        }
    }

    internal void clear_durability_panel_for_placeable(NetworkPlaceable p)
    {
        if (this.placeable_for_durability_check == p) {
            clear_placeable_durability_lookup();
        }
    }
}
