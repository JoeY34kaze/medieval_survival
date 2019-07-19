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
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;


            }
            else {
                this.GuildPanel.SetActive(true);
                ngm.SetMemberPanel(true);
                this.hasOpenWindow = true;
                this.inventoryPanel.SetActive(false);
                this.guildModificationPanel.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;


            }  
        }

        if (Input.GetButtonDown("Inventory"))
        {
            if (this.inventoryPanel.activeSelf)
            {
                this.inventoryPanel.SetActive(false);
                this.hasOpenWindow = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;




            }
            else {
                this.inventoryPanel.SetActive(true);
                npi.requestLocalUIUpdate();
                if(this.GuildPanel.activeSelf) ngm.SetMemberPanel(false);
                this.guildModificationPanel.SetActive(false);
                this.GuildPanel.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;


            }
            //guild modificationPanel se odpre z button eventa na guild managerju...

            
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
        }
        else {
            clear();
        }

    }

    public void clear() {
        this.inventoryPanel.SetActive(false);
        this.guildModificationPanel.SetActive(false);
        if (this.GuildPanel.activeSelf) ngm.SetMemberPanel(false);
        this.GuildPanel.SetActive(false);
        this.hasOpenWindow = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

       //GetComponentInParent<NetworkPlayerAnimationLogic>().hookChestRotation = true;
        //GetComponentInParent<NetworkPlayerMovement>().lockMovement = false;
        //GetComponentInParent<player_camera_handler>().lockCamera = false;
    }
}
