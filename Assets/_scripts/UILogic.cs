﻿using System;
using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking.Generated;
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

    public bool hasOpenWindow=false;
    private NetworkGuildManager ngm;
    private NetworkPlayerInventory npi;

    public GameObject container_panel_10_slots;
    public GameObject container_panel_20_slots;
    public GameObject container_panel_40_slots;
    public GameObject container_panel_80_slots;

    public GameObject container_panel_10_slots_with_upkeep;

    public Text wood_upkeep_label;
    public Text stone_upkeep_label;
    public Text iron_upkeep_label;
    public Text gold_upkeep_label;

    public NetworkContainer currently_openened_container = null;
    public NetworkPlaceable placeable_for_durability_check = null;

    private GameObject active_container_panel;
    public GameObject[] panelsPredmetiContainer;
    internal NetworkContainer currentActiveContainer;//tole se posreduje npi-ju med premikanjem predmetov. z npi v tole in obratno

    bool chatActive = false;

    public bool allows_UI_opening = false;

    private void Start()
    {
        this.ngm = GameObject.FindGameObjectWithTag("GuildManager").GetComponent<NetworkGuildManager>();
        this.npi = GetComponentInParent<NetworkPlayerInventory>();
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

    private void handleEscapePressed()
    {
        //ce je odprto ksno okno ga zapri, sicer prikaz main menu
        if (this.hasOpenWindow) ClearAll();
        else { Application.Quit(); }//prikazat main menu
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
            GetComponentInParent<NetworkPlayerMovement>().lockMovement = false;
        }
        else {
            ClearAll();
        }

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
        GetComponentInParent<NetworkPlayerAnimationLogic>().hookChestRotation = true;
        GetComponentInParent<NetworkPlayerMovement>().lockMovement = false;
        GetComponentInParent<player_camera_handler>().lockCamera = false;
    }


    private void showInventory()
    {
        this.inventoryPanel.SetActive(true);
        npi.requestLocalUIUpdate();
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
}