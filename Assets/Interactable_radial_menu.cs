using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// izkljucno lokalni class, ki vidi samo svojega playerja. player mu poslje informacije kakšen menu naj izrise, uporabnik klikne button, z buttona se klice nazaj na playerjeve skripte, kjer se pohendla networking.
/// </summary>
public class Interactable_radial_menu : MonoBehaviour
{

    private GameObject radialMenu;
    private GameObject player;
    private RMF_RadialMenu menu;

    public Transform elements;

    private int button_count = 4;
    private string[] button_title;

    private GameObject other;

    public Sprite i2;
    public Sprite i3;
    public Sprite i4;
    public Sprite i5;
    public Sprite i6;
    public Sprite i7;
    public Sprite i8;
    public Sprite i9;
    public Sprite i10;

    private void Start()
    {
        this.player = transform.root.gameObject;
        this.radialMenu = transform.GetChild(0).gameObject;
        this.menu = this.radialMenu.GetComponent<RMF_RadialMenu>();
    }

    private void show_menu(GameObject target) {
        Debug.Log("Opening menu! - interaction with a player");
        Clear_current_elements_of_menu();
        radialMenu.SetActive(true);
        this.other = target;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void show_player_interaction_menu(GameObject target_player) {
        

        show_menu(target_player);
        NetworkPlayerStats target_stats = target_player.GetComponent<NetworkPlayerStats>();

        if (target_stats.downed)
        {
            //vse mozne interakcije ko je downan
            /*
            "Pick Up";
           "Steal";
            "Mock";
             "Execute";
            "Tie Up";
            

            this.button_count = 5;
            this.button_title = new string[this.button_count];
            this.button_title[0] = "Pick Up";
            this.button_title[1] = "Steal";
            this.button_title[2] = "Mock";
            this.button_title[3] = "Execute";
            this.button_title[4] = "Tie Up";
            */

            //preber vse objekte

            GameObject btn_0_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_execution");
            GameObject btn_1_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_mock");
            GameObject btn_2_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_pickup");
            GameObject btn_3_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_steal");
            GameObject btn_4_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_tieUp");

            GameObject btn_0 = GameObject.Instantiate(btn_0_r);
            GameObject btn_1 = GameObject.Instantiate(btn_1_r);
            GameObject btn_2 = GameObject.Instantiate(btn_2_r);
            GameObject btn_3 = GameObject.Instantiate(btn_3_r);
            GameObject btn_4 = GameObject.Instantiate(btn_4_r);

            menu.elements.Clear();

            setup_button(btn_0,5);
            setup_button(btn_1, 5);
            setup_button(btn_2, 5);
            setup_button(btn_3, 5);
            setup_button(btn_4, 5);



            Button button = btn_0.transform.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { Debug.Log("EXECUTION!"); });


        }
        else {

        }
    
    }

    private void setup_button(GameObject btn,int index) {
        btn.transform.SetParent(this.elements);
        btn.transform.localPosition = Vector3.zero;
        btn.transform.localRotation = Quaternion.identity;
        btn.transform.GetChild(0).GetComponent<Button>().GetComponent<Image>().sprite = getBtnSpriteFromIndex(index);
        RMF_RadialMenuElement r = btn.GetComponent<RMF_RadialMenuElement>();
        menu.elements.Add(r);
        r.init();
        r.setup();
    }

    private Sprite getBtnSpriteFromIndex(int index)
    {
        switch (index){
            case 2:
                return this.i2;
            case 3:
                return this.i3;
            case 4:
                return this.i4;
            case 5:
                return this.i5;
            case 6:
                return this.i6;
            case 7:
                return this.i7;
            case 8:
                return this.i8;
            case 9:
                return this.i9;
            case 10:
                return this.i10;
            default:
                return null;
        }
    }

    internal void show_ArmorStand_interaction_menu(GameObject stand)
    {
        show_menu(stand);
        /*
        this.button_title[0] = "Take All";
        this.button_title[1] = "Give All";
        this.button_title[2] = "Helmet";
        this.button_title[3] = "Chest";
        this.button_title[4] = "Gloves";
        this.button_title[5] = "Greaves";
        this.button_title[6] = "Boots";
        this.button_title[7] = "Main Weapon";
        this.button_title[8] = "Secondary Weapon";
        this.button_title[9] = "Shield";
        this.button_title[10] = "Ranged Weapon";
        */


        //--------------------------TEST

        GameObject btn_0_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_execution");
        GameObject btn_1_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_mock");
        GameObject btn_2_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_pickup");
        GameObject btn_3_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_steal");
        GameObject btn_4_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_tieUp");

        GameObject btn_0 = GameObject.Instantiate(btn_0_r);
        GameObject btn_1 = GameObject.Instantiate(btn_1_r);
        GameObject btn_2 = GameObject.Instantiate(btn_2_r);
        GameObject btn_3 = GameObject.Instantiate(btn_3_r);
        GameObject btn_4 = GameObject.Instantiate(btn_4_r);

        menu.elements.Clear();

        setup_button(btn_0,5);
        setup_button(btn_1, 5);
        setup_button(btn_2, 5);
        setup_button(btn_3, 5);
        setup_button(btn_4, 5);

        

        //menu.textLabel.text = "Downed Player";

        Button button = btn_0.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { player_interaction_button_execution(); });

        button = btn_1.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { player_interaction_button_mock(); });

        button = btn_0.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { player_interaction_button_pickup(); });

        button = btn_0.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { player_interaction_button_steal(); });

        button = btn_0.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { player_interaction_button_tieUp(); });



        menu.reDraw();
    }


    public void hide_radial_menu() {
        Debug.Log("Hiding radial menu.");
        Clear_current_elements_of_menu();
        this.radialMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    /// <summary>
    /// za radial menu pobrise vse elemente, tko da je prazen.
    /// </summary>
    private void Clear_current_elements_of_menu()
    {
        //throw new NotImplementedException();
    }

    /// <summary>
    /// predpostavlja da je radial menu prazen in da samo gor namecemo stvari.
    /// </summary>
    private void Update_radial_menu_elements() {


    }
    public void radial_menu_button_pressed() {//rabmo zvohat kter button

    }

    public void player_interaction_button_execution()
    {
        Debug.Log("button - execution - " + this.other.name);
    }

    public void player_interaction_button_tieUp()
    {
        Debug.Log("button - tieUp - " + this.other.name);
    }

    public void player_interaction_button_steal() {
        Debug.Log("button - steal - " + this.other.name);
    }

    public void player_interaction_button_pickup()
    {
        Debug.Log("button - pickUp - " + this.other.name);
    }

    public void player_interaction_button_mock()
    {
        Debug.Log("button - mock - " + this.other.name);
    }
}
