using System;
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

    public bool hasOpenWindow=false;
    private NetworkGuildManager ngm;
    private NetworkPlayerInventory npi;

    public GameObject container_panel_10_slots;
    public GameObject container_panel_20_slots;
    public GameObject container_panel_40_slots;
    public GameObject container_panel_80_slots;

    

    private GameObject active_container_panel;
    public GameObject[] panelsPredmetiContainer;
    internal NetworkContainer currentActiveContainer;//tole se posreduje npi-ju med premikanjem predmetov. z npi v tole in obratno

    bool chatActive = false;

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
            clear();
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
            clear();
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
            clear();
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
        if (this.hasOpenWindow) clear();
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
            clear();
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

    public void clear() {
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

    private void UpdateActiveChestPanel(Predmet[] predmeti)
    {
        if (this.active_container_panel != null)
            for (int i = 0; i < this.active_container_panel.transform.childCount; i++) {
                this.panelsPredmetiContainer[i].GetComponent<InventorySlotContainer>().AddPredmet(predmeti[i]);
            }
    }

    private void clearChestPanelPredmeti() {
        if (this.active_container_panel != null)
            for (int i = 0; i < this.active_container_panel.transform.childCount; i++)
            {
                this.panelsPredmetiContainer[i].GetComponent<InventorySlotContainer>().AddPredmet(null);
            }
    }

    internal void setCurrentActiveContainer(NetworkContainer container)
    {
        this.currentActiveContainer = container;
    }

    public void OnGuildModificationButtonClick() {
        clear();
        enableMouse();
        NetworkGuildManager.Instance.OnButtonModifyClick();
        
    }


}
