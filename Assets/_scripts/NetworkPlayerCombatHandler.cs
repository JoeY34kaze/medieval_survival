using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System;

public class NetworkPlayerCombatHandler : NetworkPlayerCombatBehavior
{
    public byte combat_mode = 0; //0 = no combat, 1= combat  podobno kot ma star citizen drugacne komande ko si pes al pa v vozilu
    public bool blocking = false;
    public bool Blocking { set { this.blocking = value; } }
    public byte Combat_mode
    {
        get { return this.combat_mode; }
        set { this.combat_mode = value; }
    }
    private panel_bar_handler bar_handler;
    public bool in_attack_animation = false;
    private Animator animator;
    private player_local_locks player_local_locks;
    private NetworkPlayerStats stats;

    private NetworkPlayerInventory networkPlayerInventory;
    public GameObject radial_menu;
    public GameObject[] combat_sound_effects;

    public Transform weapon_slot;
    public Transform shield_slot;

    public int currently_equipped_shield;

    private int GetChildIndexOfShieldFromId(int n)
    {
        foreach (Transform c in shield_slot)
            if (c.GetComponent<identifier_helper>().id == n)
                return c.transform.GetSiblingIndex();

        throw new Exception("Cannot find sibling id for shield from item id!");
    }

    private void disable_all_shields()
    {
        foreach(Transform c in shield_slot) {
            if (c.gameObject.activeSelf) c.gameObject.SetActive(false);
        }
    }

    private NetworkPlayerNeutralStateHandler neutralStateHandler;

    public int[] equipped_weapons;//weaponi k so u loadoutu od playerja

    public int index_of_currently_selected_weapon_from_equipped_weapons = 0;

    //---------------------------------------------------------------------------------------DELEGATES------------------------------------------------------------
    public delegate void On_Current_Weapon_Variable_Change_Delegate(int newVal);
    public event On_Current_Weapon_Variable_Change_Delegate Current_weapon_change_event;
    private void On_Current_weapon_changed(int newVal)
    {
        Debug.Log(this.index_of_currently_selected_weapon_from_equipped_weapons +" - > WEAPON HAS BEEN CHANGED! index is now :" + newVal);
        animator.SetInteger("weapon_animation_class", getWeaponClassForAnimator(equipped_weapons[newVal])); //tole je podvojen pri refreshu weaponov ker ce poberes al pa dropas item zaobide tega delegata. optimizacija strukture kode ksnej ce se bo dalo
        refresh_weapon(newVal);
    }

    private int getWeaponClassForAnimator(int weapon_id)//tole bo treba updejtat i guess.
    {
        if (weapon_id < 2) return 0;
        return Mapper.instance.getItemById(weapon_id).weapon_animation_class;
    }

    //------------------------------------------------------------------------------------------NETWORKING-----------------------------------------------------------

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player_local_locks = GetComponent<player_local_locks>();
        stats = GetComponent<NetworkPlayerStats>();
        networkPlayerInventory = GetComponent<NetworkPlayerInventory>();
        //this.radial_menu = transform.GetComponentInChildren<RMF_RadialMenu>().gameObject; -treba dat v start ker sicer crkne k ni se vse nrjen
        this.bar_handler = GetComponentInChildren<panel_bar_handler>();
        this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
        initialize_weapons();
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        this.Current_weapon_change_event += On_Current_weapon_changed; //registriramo delegata ceprav ga ubistvu nerabmo vec ker koda ni vec tolk zapletena

        //poslji request da nj updejta characterja tko kot je na serverju
    }

    private void initialize_weapons() {
        this.equipped_weapons = new int[5];//unarmed,unarmed block,wep0,wep1,ranged
        this.equipped_weapons[0] = 0;
        this.equipped_weapons[1] = 1;
        this.equipped_weapons[2] = 0;
        this.equipped_weapons[3] = 0;
        this.currently_equipped_shield = 1;
    }

    private bool is_allowed_to_attack_local() {
        if (stats.downed || stats.dead)
        {
            return false; //Ce je downan da nemora vec napadat pa take fore. to je precej loše ker je na clientu. ksnej bo treba prenest to logiko na server ker tole zjebe ze cheatengine
        }

        if (networkPlayerInventory.panel_inventory.activeSelf) return false; //odprt inventorij
        if (stats.guild_modification_panel.activeSelf) return false;
        if (this.radial_menu.activeSelf) return false;

        return true;
    }

    private void Update()
    {
        if (networkObject == null) {
            Debug.LogError("networkObject is null. - najbrz zato ker se se connecta gor.");
            return; }
        if (!networkObject.IsOwner)
        {
            handle_animations_from_rpcs();
            return;
        }

        if (!is_allowed_to_attack_local()) return;

        check_and_handle_combat();//keyboard input glede combata
    }

    private void check_and_handle_combat()
    {
        check_and_handle_combat_mode();
        check_for_weapon_switch();

        if (in_attack_animation)
        {
            if (animator.GetCurrentAnimatorStateInfo(1).IsName("combat_layer.in_combat_idle") || animator.GetCurrentAnimatorStateInfo(1).IsName("combat_layer.1h_sword_idle"))
            {
                in_attack_animation = false;
            }
        }
        if (this.combat_mode == 1)
        {
            if (Input.GetButtonDown("Fire1") && !this.in_attack_animation && !this.blocking)
            {
                execute_main_attack();
            }
            else if (Input.GetButtonDown("Fire2"))
            {//blocking-------------------------------------------------------------
                if (current_shield_can_perform_block())
                {
                    this.blocking = true;
                    block_activate();
                }
            }
            else if (Input.GetButtonUp("Fire2"))
            {
                this.blocking = false;
                block_deactivate();

            }


            if (animator.GetBool("combat_blocking") != this.blocking)
            {
                animator.SetBool("combat_blocking", this.blocking);
                sendSecondaryAttackRpc(this.blocking);
            }//---------------------------------------------------------------------


            //treba nekej nrdit da pofejkas attack. torej iz main attack animacije bo treba skocit v nevtralno ce da med attackom block recimo
            if (this.in_attack_animation && Input.GetButtonDown("Fire2"))
            {
                this.in_attack_animation = false;
                animator.SetTrigger("feign");
                networkObject.SendRpc(RPC_NETWORK_FEIGN, Receivers.OthersProximity);
            }
        }

    }
    /// <summary>
    /// ce bojo meli shieldi durability al pa kej tazga
    /// </summary>
    /// <returns></returns>
    private bool current_shield_can_perform_block()
    {
       // Debug.Log("Trying to perform block!");
        return true;
    }

    private void check_for_weapon_switch()
    {
        if (!networkObject.IsOwner) return;
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && !this.blocking && !this.in_attack_animation) // forward - menja weapone. unarmed fist - unarmed block se skippa vmes - weapon0 - weapon1 - ranged
        {
         //   Debug.Log("client : sending weapon change request");
            networkObject.SendRpc(RPC_CHANGE_CURRENT_WEAPON_REQUEST, Receivers.Server);
        }
    }

    public void handle_player_death() {
        Debug.Log("handling player death");
        disable_all_possible_equipped_weapons();
        disable_all_shields();
        if (networkObject.IsOwner && this.combat_mode==1)
        {
            networkObject.SendRpc(RPC_CHANGE_COMBAT_MODE_REQUEST, Receivers.Server);
        }
    }

    public void handle_player_downed() {
        Debug.Log("handling player downed");
        if (networkObject.IsOwner && this.combat_mode == 1)
        {
            networkObject.SendRpc(RPC_CHANGE_COMBAT_MODE_REQUEST, Receivers.Server);
        }
    }

    internal void setCurrentWeaponToFirstNotEmpty()
    {
        update_equipped_weapons();

       // Debug.Log("server: checking to see it player is unarmed");
        if (this.equipped_weapons[index_of_currently_selected_weapon_from_equipped_weapons] > 1) return;
       // Debug.Log("server: player is unarmed. setting current weapon to first not empty");
        //nastimej na prvga k ni empty
        int first = 0;
        if (this.equipped_weapons[2] != 0) { first = 2; }
        else if(this.equipped_weapons[3] != 0) { first = 3; }
        else if(this.equipped_weapons[4] != 0) { first = 4; }
        //posl rpc clientu kaj je njegov nov index
        this.index_of_currently_selected_weapon_from_equipped_weapons = first;
        networkObject.SendRpc(RPC_CHANGE_CURRENT_WEAPON_RESPONSE, Receivers.All, first);
    }

    public void update_equipped_weapons()//tole se klice takoj ko se zgodi networkUpdate za loadout na others, ter pred networkUpdate na serverju
    {
        if (this.equipped_weapons.Length == 0)
            initialize_weapons();

        //Debug.Log("updating equipped weapons");
        // this.equipped_weapons[0] = 0;//unarmed
        // this.equipped_weapons[1] = 1;//unarmed block

        this.equipped_weapons[2] = networkPlayerInventory.GetWeapon0();
        this.equipped_weapons[3] = networkPlayerInventory.GetWeapon1();
        this.equipped_weapons[4] = networkPlayerInventory.GetRanged();
        this.currently_equipped_shield = networkPlayerInventory.GetShield();

        refresh_in_hand();

    }

    private void refresh_in_hand() {
        //Debug.Log("refreshing all in hand");
        //deaktiviramo vse in aktiviramo kar ima v rokah.
        refresh_weapon(this.index_of_currently_selected_weapon_from_equipped_weapons);
        refresh_shield(this.currently_equipped_shield);
    }
    private void refresh_weapon(int newVal) {
        disable_all_possible_equipped_weapons();

        getChildWeaponById(equipped_weapons[newVal]).SetActive(true);
       

        animator.SetInteger("weapon_animation_class", getWeaponClassForAnimator(equipped_weapons[newVal]));

    }

    private GameObject getChildWeaponById(int weap_id)
    {
        for (int i = 0; i < this.weapon_slot.childCount; i++) {
            if (this.weapon_slot.GetChild(i).GetComponent<identifier_helper>().id == weap_id)
                return this.weapon_slot.GetChild(i).gameObject;
        }
        return null;
    }

    private void refresh_shield(int shield_id) {

        disable_all_shields();

        foreach (Transform c in shield_slot)
        {

            if (c.GetComponent<identifier_helper>().id == shield_id)
            {
                c.gameObject.SetActive(true);
                break;
            }
        }
    }


    private int getSiblingIndexOfFirstActiveChild_Weapon()
    {
        int k = -1;

        for (int j = 0; j < weapon_slot.childCount; j++)
        {
            if (weapon_slot.GetChild(j).gameObject.activeSelf)
            {
                return j;
            }
        }
        return k;
    }

    private void block_activate()
    {
        if (this.currently_equipped_shield == 1)
        {//unarmed block or block with weapon
            Debug.Log("Blocking with unarmed / weapon");
        }
        else {
            //perform block with shield
            Debug.Log("Blocking with shield!");
        } 

    }

    private void block_deactivate()
    {
        if (this.currently_equipped_shield == 1)
        {//unarmed block or block with weapon
            Debug.Log("Stopped blocking with unarmed / weapon");
        }
        else
        {
            //perform block with shield
            Debug.Log("Stopped blocking with shield!");
        }

    }

    private void check_and_handle_combat_mode()
    {
        if (Input.GetButtonDown("Change combat mode") && is_allowed_to_attack_local())
        {
            Debug.Log("client: sending change combat mode request");
            networkObject.SendRpc(RPC_CHANGE_COMBAT_MODE_REQUEST, Receivers.Server);
        }

    }

    private void place_shield_on_back()
    {
        Debug.Log("Treba implementirat da se da shield na hrbet.");
    }


    private void disable_all_possible_equipped_weapons()
    {
        for(int i=0;i<weapon_slot.childCount;i++)
            weapon_slot.GetChild(i).gameObject.SetActive(false);
        
    }

    private void handle_animations_from_rpcs()//sprozi se samo za remote playerje da jim pohendla parametre v animatorju
    {

        animator.SetBool("combat_blocking", this.blocking);
        if (animator.GetInteger("combat_mode") != (int)Combat_mode)
        {
            animator.SetInteger("combat_mode", (int)Combat_mode);
            //disablat weapon k ma u rok.
            if((int)Combat_mode==0)
                disable_all_possible_equipped_weapons();
        }
    }





    IEnumerator StartFire1Lock(float timeout)
    {
        player_local_locks.fire1_available = false;
        yield return new WaitForSeconds(timeout);
        player_local_locks.fire1_available = true;
    }

    private void setup_Fire1_lock()
    {
        StartCoroutine(StartFire1Lock(stats.fire1_cooldown));
    }

    public void execute_main_attack_from_remote()
    {//spodnjega nesmemo izvedt po rpcju ker pridemo sicer v loop
        animator.SetTrigger("attack_1");
        play_main_attack_sound_effect();
    }
    private void execute_main_attack()
    {
        if (!player_local_locks.fire1_available) return;
        else setup_Fire1_lock();
        //tukej bi dau kodo da menja med levo pa desno roko med attackom 0 - obe, 1 = leva, 2 = desna;
        animator.SetTrigger("attack_1");
        in_attack_animation = true;
        sendMainAttackRpc();
        play_main_attack_sound_effect();
    }

    private void play_main_attack_sound_effect()
    {
        //poisc kter je taprav sound effect za predvajat, zaenkrat je samo edn
        GameObject clip = GameObject.Instantiate(combat_sound_effects[0]);
        clip.transform.parent = transform;
    }


    private void reset_all_combat_related_animator_parameters()
    {
        animator.SetBool("combat_blocking", false);
        this.blocking = false;
        //fire1 se mora itak resetirat ker je trigger


    }

    /// <summary>
    /// Na serverju se aktivira collider na weaponu s ktermu napadamo. to metodo naj bi klical animation event.
    /// </summary>
    /// <param name="index_roke">-ubistvu je samo true pa false. 1 se enabla desna roka. -1 se disabla desna roka. </param>
    public void activate_weapon_collider_server(int index_roke)
    {
        if (!networkObject.IsServer) return;
        bool active_l = false;
        bool active_r = false;
        //if (index_roke == 0 || index_roke == 2) active_l = true;
        if (index_roke == 1 || index_roke == 2) active_r = true;
        Debug.Log("Activating colliders " + index_roke + " " + active_l + active_r);

        int ind = getSiblingIndexOfFirstActiveChild_Weapon();
        if (this.weapon_slot.GetChild(ind).GetComponent<Collider>().enabled != active_r)
        {//change right colider
            this.weapon_slot.GetChild(ind).GetComponent<Weapon_collider_handler>().set_offensive_colliders(active_r);
        }
    }

    public void sendMainAttackRpc()
    {
        networkObject.SendRpc(RPC_NETWORK_FIRE1, Receivers.OthersProximity);
    }

    public void sendSecondaryAttackRpc(bool b)
    {
        networkObject.SendRpc(RPC_NETWORK_FIRE2, Receivers.OthersProximity, b);
    }


    private void draw_current_weapon()
    {
        //Debug.Log("drawing current weapon");
        disable_all_possible_equipped_weapons();
        int id2 = this.equipped_weapons[index_of_currently_selected_weapon_from_equipped_weapons];
        foreach (Transform c in weapon_slot) {
            if (c.GetComponent<identifier_helper>().id == id2)
            {
                c.gameObject.SetActive(true);
                return;
            }
        }
    }

    //--------------------------RPC's

    public override void NetworkFire1(RpcArgs args)
    {
        if(args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
            execute_main_attack_from_remote();
    }

    public override void NetworkFire2(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            bool blocking = args.GetNext<bool>();
            this.blocking = blocking;
        }
    }

    public override void NetworkFeign(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
            animator.SetTrigger("feign");
    }

    /// <summary>
    /// weapon1
    /// weapon2
    /// previous weapon1
    /// previous weapon2
    /// ce je previous weapon == -1 pomen da clearej vse weapone. to se nrdi samo na zacetku ko gre prvic u combat state just in case
    /// </summary>
    /// <param name="args"></param>
    public override void ChangeCurrentWeaponRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) return;
       // Debug.Log("server: got weapon change request. sending response IF all is legit");

        //djmo scrollat samo prek weaponov k niso unarmed. nocmo trikat misko premaknt ker je povsod unarmed
        if (this.blocking || this.in_attack_animation) return;

        if (this.equipped_weapons[this.index_of_currently_selected_weapon_from_equipped_weapons] == 0) this.index_of_currently_selected_weapon_from_equipped_weapons = 0;//ce je kterkoli drug indeks kot 0 enak 0 ga nastav na 0
        int next_index = (this.index_of_currently_selected_weapon_from_equipped_weapons + 1) % 5;
        if (this.index_of_currently_selected_weapon_from_equipped_weapons == 0)
        {
            if (this.equipped_weapons[2] != 0) next_index = 2;//wep0
            else
            {
                if (this.equipped_weapons[3] != 0) next_index = 3;//wep1
                else
                {
                    if (this.equipped_weapons[4] != 0) next_index = 4;//ranged
                    else next_index = 0;//vsi so unarmed
                }
            }
        }

        this.index_of_currently_selected_weapon_from_equipped_weapons = next_index;//tole rabmo sporocit vsem, tud serverju ker je server tud player lahko
        networkObject.SendRpc(RPC_CHANGE_CURRENT_WEAPON_RESPONSE, Receivers.All, this.index_of_currently_selected_weapon_from_equipped_weapons);
    }

    public override void ChangeCurrentWeaponResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
          //  Debug.Log("Client & server : got weapon change response.");
            if(!networkObject.IsServer)this.index_of_currently_selected_weapon_from_equipped_weapons = args.GetNext<int>(); // event pohendla vse kar se rab nrdit ob spremembi
            animator.SetInteger("weapon_animation_class", getWeaponClassForAnimator(equipped_weapons[this.index_of_currently_selected_weapon_from_equipped_weapons]));
            draw_current_weapon();
        }
    }


    public override void ChangeCombatModeRequest(RpcArgs args)
    {
        if (!networkObject.IsServer || args.Info.SendingPlayer.NetworkId != networkObject.Owner.NetworkId) return;
       // Debug.Log("server : got change combat mode request");
        if (!this.in_attack_animation) {
            int prev = this.combat_mode;
            int next = 0;
            if (prev == 0) next = 1;

            networkObject.SendRpc(RPC_CHANGE_COMBAT_MODE_RESPONSE, Receivers.All, next);
        }
    }

    public override void ChangeCombatModeResponse(RpcArgs args)//ALL GET IT, EVEN SERVER
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;
        
        int new_mode = args.GetNext<int>();

        this.combat_mode = (byte)new_mode;
       // Debug.Log("got change combat mode response : "+this.combat_mode + " "+new_mode);

        if (new_mode == 0)
        {
            
            animator.SetInteger("combat_mode", 0);


            reset_all_combat_related_animator_parameters();//legacy
            place_shield_on_back();
            //disable_all_possible_equipped_weapons();
            neutralStateHandler.NeutralStateSetup();
        }
        else {
            
            animator.SetInteger("combat_mode", 1);
            animator.SetInteger("weapon_animation_class", getWeaponClassForAnimator(equipped_weapons[this.index_of_currently_selected_weapon_from_equipped_weapons]));

            neutralStateHandler.CombatStateSetup();

            draw_current_weapon();
        }

    }


    public override void SendAll(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            this.combat_mode = (byte)args.GetNext<int>();
            this.blocking = args.GetNext<bool>();
            this.index_of_currently_selected_weapon_from_equipped_weapons = args.GetNext<int>();
            refresh_in_hand();
        }
    }

    internal void ServerSendAll(NetworkingPlayer p)
    {
        if (networkObject.IsServer)
        {
            networkObject.SendRpc(p, RPC_SEND_ALL, (int)this.combat_mode, this.blocking, this.index_of_currently_selected_weapon_from_equipped_weapons);
        }
    }
}
