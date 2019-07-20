using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// skripta skrbi za celotno logiko kar se tice ui-a. izjema je radial menu, canvas za healthbar, team panel pa take fore. cez ima guild, inventorij, pa take stvari k jih player lahko sam odpira
/// </summary>
public class UILogic : MonoBehaviour
{
    public GameObject guildModificationPanel;
    public GameObject GuildPanel;
    public GameObject inventoryPanel;
    public GameObject crafting_panel;

    public bool hasOpenWindow=false;
    private NetworkGuildManager ngm;
    private NetworkPlayerInventory npi;

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

        if (Input.GetButtonDown("Guild"))
        {
            if (this.GuildPanel.activeSelf)
            {
                ngm.SetMemberPanel(false);
                this.GuildPanel.SetActive(false);
                this.hasOpenWindow = false;
                DisableMouse();


            }
            else {
                this.GuildPanel.SetActive(true);
                ngm.SetMemberPanel(true);
                this.hasOpenWindow = true;
                this.inventoryPanel.SetActive(false);
                this.guildModificationPanel.SetActive(false);
                enableMouse();


            }  
        }

        if (Input.GetButtonDown("Inventory"))
        {
            if (this.inventoryPanel.activeSelf)
            {
                this.inventoryPanel.SetActive(false);
                this.hasOpenWindow = false;
                DisableMouse();




            }
            else {
                this.inventoryPanel.SetActive(true);
                npi.requestLocalUIUpdate();
                if(this.GuildPanel.activeSelf) ngm.SetMemberPanel(false);
                this.guildModificationPanel.SetActive(false);
                this.GuildPanel.SetActive(false);
                enableMouse();


            }
            //guild modificationPanel se odpre z button eventa na guild managerju...
            
            
        }
        if (Input.GetButtonDown("Crafting"))
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
                enableMouse();
            }
        }
    }

    private void handleEscapePressed()
    {
        //ce je odprto ksno okno ga zapri, sicer prikaz main menu
        clear();
    }

    internal void showGuildModificationPanel(bool b, NetworkGuildManager ngm)
    {
        this.guildModificationPanel.SetActive(b);
        if (b)
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void clear() {
        this.inventoryPanel.SetActive(false);
        this.guildModificationPanel.SetActive(false);
        if (this.GuildPanel.activeSelf) ngm.SetMemberPanel(false);
        this.GuildPanel.SetActive(false);
        this.crafting_panel.SetActive(false);
        this.hasOpenWindow = false;

        DisableMouse();
       //GetComponentInParent<NetworkPlayerAnimationLogic>().hookChestRotation = true;
        GetComponentInParent<NetworkPlayerMovement>().lockMovement = false;
        //GetComponentInParent<player_camera_handler>().lockCamera = false;
    }
}
