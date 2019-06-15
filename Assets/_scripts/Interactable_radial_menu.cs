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
    private NetworkPlayerInteraction interaction;
    private RMF_RadialMenu menu;

    public Transform elements;

    private string[] button_title;

    private GameObject target;

    public Text center_label;

    public Sprite i2;
    public Sprite i3;
    public Sprite i4;
    public Sprite i5;
    public Sprite i6;
    public Sprite i7;
    public Sprite i8;
    public Sprite i9;
    public Sprite i10;

    private int number_of_elements = 0;

    private void Start()
    {
        this.player = transform.root.gameObject;
        this.radialMenu = transform.GetChild(0).gameObject;
        this.menu = this.radialMenu.GetComponent<RMF_RadialMenu>();
        this.interaction = transform.root.GetComponent<NetworkPlayerInteraction>();
    }

    private void show_menu(GameObject target) {

        //pobris od prejsnjega za vsak slucaj ceprav bi moral bit ze prazno
        menu.elements.Clear();
        foreach (Transform child in elements)
        {
            GameObject.Destroy(child.gameObject);
        }
        center_label.text = "";



        //Debug.Log("Opening menu! - interaction with a player");
        radialMenu.SetActive(true);
        this.target = target;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void show_player_interaction_menu(GameObject target_player) {
        

        show_menu(target_player);
        NetworkPlayerStats target_stats = target_player.GetComponent<NetworkPlayerStats>();

        if (target_stats.downed)
        {
            this.number_of_elements = 5;
            menu.angleOffset = (360f / this.number_of_elements);
            center_label.text = "Downed Player";

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

            setup_button(btn_0, menu.angleOffset*0);
            setup_button(btn_1, menu.angleOffset*1);
            setup_button(btn_2, menu.angleOffset*2);
            setup_button(btn_3, menu.angleOffset*3);
            setup_button(btn_4, menu.angleOffset*4);



            //menu.textLabel.text = "Downed Player";

            Button button = btn_0.transform.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { player_interaction_button_execution(); });

            button = btn_1.transform.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { player_interaction_button_mock(); });

            button = btn_2.transform.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { player_interaction_button_pickup(); });

            button = btn_3.transform.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { player_interaction_button_steal(); });

            button = btn_4.transform.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { player_interaction_button_tieUp(); });



            
        }
        else {



            //interaction_player_guild_invite
            //interaction_player_team_invite
            this.number_of_elements = 2;
            menu.angleOffset = (360f / this.number_of_elements);
            center_label.text = "Player";

            GameObject btn_0_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_guild_invite");
            GameObject btn_1_r = Resources.Load<GameObject>("radial_menu_elements/interaction_player_team_invite");
            GameObject btn_0 = GameObject.Instantiate(btn_0_r);
            GameObject btn_1 = GameObject.Instantiate(btn_1_r);
            menu.elements.Clear();
            setup_button(btn_0, menu.angleOffset*0);
            setup_button(btn_1, menu.angleOffset*1);
            //menu.textLabel.text = "Downed Player";

            Button button = btn_0.transform.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { player_interaction_button_guild_invite(); });

            button = btn_1.transform.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { player_interaction_button_team_invite(); });
        }
        menu.reDraw();

    }

    private void setup_button(GameObject btn,float offset) {
        btn.transform.SetParent(this.elements);
        btn.transform.localPosition = Vector3.zero;
        btn.transform.localRotation = Quaternion.identity;
        btn.transform.GetChild(0).GetComponent<Button>().GetComponent<Image>().sprite = getBtnSpriteFromIndex(this.number_of_elements);
        RMF_RadialMenuElement r = btn.GetComponent<RMF_RadialMenuElement>();
        r.angleOffset = offset;// - menu.angleOffset/2;
        
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
                return this.i10;
        }
    }

    internal void show_ArmorStand_interaction_menu(GameObject stand)
    {
        show_menu(stand);
        /*
        this.button_title[1] = "Swap";
        this.button_title[1] = "Take All";
        this.button_title[2] = "Give All";
        this.button_title[3] = "Helmet";
        this.button_title[4] = "Chest";
        this.button_title[5] = "Gloves";
        this.button_title[6] = "Greaves";
        this.button_title[7] = "Boots";
        this.button_title[8] = "Main Weapon";
        this.button_title[9] = "Secondary Weapon";
        this.button_title[10] = "Shield";
        this.button_title[11] = "Ranged Weapon";
        */

        show_menu(stand);

        this.number_of_elements = 12;
        menu.angleOffset = (360f / this.number_of_elements);
        center_label.text = "Armor Stand";


        GameObject btn_0 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_swap"));
        GameObject btn_1 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_take_all"));
        GameObject btn_2 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_give_all"));
        GameObject btn_3 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_helmet"));
        GameObject btn_4 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_chest"));
        GameObject btn_5 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_hands"));
        GameObject btn_6 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_legs"));
        GameObject btn_7 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_feet"));

        GameObject btn_8 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_weapon0"));
        GameObject btn_9 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_weapon1"));
        GameObject btn_10 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_shield"));
        GameObject btn_11 = GameObject.Instantiate(Resources.Load<GameObject>("radial_menu_elements/interaction_armor_stand_ranged"));

        menu.elements.Clear();

        setup_button(btn_0, menu.angleOffset * 0);
        setup_button(btn_1, menu.angleOffset * 1);
        setup_button(btn_2, menu.angleOffset * 2);
        setup_button(btn_3, menu.angleOffset * 3);
        setup_button(btn_4, menu.angleOffset * 4);
        setup_button(btn_5, menu.angleOffset * 5);
        setup_button(btn_6, menu.angleOffset * 6);
        setup_button(btn_7, menu.angleOffset * 7);
        setup_button(btn_8, menu.angleOffset * 8);
        setup_button(btn_9, menu.angleOffset * 9);
        setup_button(btn_10, menu.angleOffset * 10);
        setup_button(btn_11, menu.angleOffset * 11);

        Button button = btn_0.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_swap(); });

        button = btn_1.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_take_all(); });

        button = btn_2.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_give_all(); });

        button = btn_3.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_helmet(); });

        button = btn_4.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_chest(); });

        button = btn_5.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_hands(); });

        button = btn_6.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_legs(); });

        button = btn_7.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_feet(); });

        button = btn_8.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_weapon0(); });

        button = btn_9.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_weapon1(); });

        button = btn_10.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_shield(); });

        button = btn_11.transform.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { armor_stand_interaction_button_ranged(); });


        menu.reDraw();
    }



    public void hide_radial_menu() {//pobrise vse kar smo dodal pa take fore
       // Debug.Log("Hiding radial menu.");
        menu.elements.Clear();
        foreach (Transform child in elements)
        {
            GameObject.Destroy(child.gameObject);
        }
        center_label.text = "";
        this.radialMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void player_interaction_button_execution()
    {
        //Debug.Log("button - execution - " + this.other.name);
        interaction.local_player_interaction_execution_request(this.target);
    }

    public void player_interaction_button_tieUp()
    {
        interaction.local_player_interaction_tieup_request(this.target);
        //Debug.Log("button - tieUp - " + this.other.name);
    }

    public void player_interaction_button_steal() {
        //Debug.Log("button - steal - " + this.other.name);
        interaction.local_player_interaction_steal_request(this.target);
    }

    public void player_interaction_button_pickup()
    {
        //Debug.Log("button - pickUp - " + this.other.name);
        interaction.local_player_interaction_pickup_request(this.target);
    }

    public void player_interaction_button_mock()
    {
        //Debug.Log("button - mock - " + this.other.name);
        interaction.local_player_interaction_guild_invite_request(this.target);
    }

    public void player_interaction_button_guild_invite()
    {
        //Debug.Log("button - mock - " + this.other.name);
        interaction.local_player_interaction_guild_invite_request(this.target);
    }

    public void player_interaction_button_team_invite()
    {
        //Debug.Log("button - mock - " + this.other.name);
        interaction.local_player_interaction_team_invite_request(this.target);
    }

    private void armor_stand_interaction_button_ranged()
    {
        interaction.local_armor_stand_interaction_ranged_request(this.target);
    }

    private void armor_stand_interaction_button_shield()
    {
        interaction.local_armor_stand_interaction_shield_request(this.target);
    }

    private void armor_stand_interaction_button_weapon1()
    {
        interaction.local_armor_stand_interaction_weapon1_request(this.target);
    }

    private void armor_stand_interaction_button_weapon0()
    {
        interaction.local_armor_stand_interaction_weapon0_request(this.target);
    }

    private void armor_stand_interaction_button_feet()
    {
        interaction.local_armor_stand_interaction_feet_request(this.target);

    }

    private void armor_stand_interaction_button_legs()
    {
        interaction.local_armor_stand_interaction_legs_request(this.target);

    }

    private void armor_stand_interaction_button_hands()
    {
        interaction.local_armor_stand_interaction_hands_request(this.target);

    }

    private void armor_stand_interaction_button_chest()
    {
        interaction.local_armor_stand_interaction_chest_request(this.target);

    }

    private void armor_stand_interaction_button_helmet()
    {
        interaction.local_armor_stand_interaction_helmet_request(this.target);

    }

    private void armor_stand_interaction_button_give_all()
    {
        interaction.local_armor_stand_interaction_give_all_request(this.target);

    }

    private void armor_stand_interaction_button_take_all()
    {
        interaction.local_armor_stand_interaction_take_all_request(this.target);

    }

    private void armor_stand_interaction_button_swap()
    {
        interaction.local_armor_stand_interaction_swap_request(this.target);
    }
}
