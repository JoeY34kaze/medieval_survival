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

    public bool in_attack_animation = false;
    private Animator animator;
    private player_local_locks player_local_locks;
    private NetworkPlayerStats stats;

    private NetworkPlayerInventory networkPlayerInventory;
    //private Mapper mapper;
    public GameObject[] combat_sound_effects;

    public Transform weapon_slot;
    public Transform shield_slot;

    private int equipped_shield_dont_access_this = 1;//zmer bo id shielda, ce ni u loadoutu shielda bo meu unarmed block
    public int currently_equipped_shield {
        get { return this.equipped_shield_dont_access_this; }
        set {
            if (this.equipped_shield_dont_access_this == value) return;
            else {
                equipped_shield_dont_access_this = value;
                if (Current_shield_changed_event != null)
                    Current_shield_changed_event(this.equipped_shield_dont_access_this);
            }
        }
    }
    public delegate void On_Current_Shield_Variable_Change_Delegate(int n);
    public event On_Current_Shield_Variable_Change_Delegate Current_shield_changed_event;
    private void On_Current_shield_changed(int n) {

        Debug.Log("Shield HAS BEEN CHANGED! " + n);
        if (combat_mode == 0) return;
        if (networkObject.IsOwner)
        {
            if (n != 1)
            {
                if (this.shield_slot.GetChild(1).gameObject.activeSelf)//ce slucajn drzimo block
                {
                    this.shield_slot.GetChild(1).gameObject.SetActive(false);
                }
                this.shield_slot.GetChild(GetChildIndexOfShieldFromId(n)).gameObject.SetActive(true);
                this.shield_slot.GetChild(GetChildIndexOfShieldFromId(n)).gameObject.GetComponent<Collider>().enabled = false;
            }
            else
            {
                disable_all_shields();
            }
            networkObject.SendRpc(RPC_CHANGE_CURRENT_SHIELD, Receivers.OthersProximity, n);
        }
        //animator?
        //izrisavanje
        //rpcji
    }

    private int GetChildIndexOfShieldFromId(int n)
    {
        if (n == 3) {//id je iron shield, vrni pozicijo na roki k je.
            return 2;
        }
        return 1;
    }

    private void disable_all_shields()
    {
        foreach(Transform c in shield_slot) {
            if (c.gameObject.activeSelf) c.gameObject.SetActive(false);
        }
    }

    public int[] equipped_weapons;//weaponi k so u loadoutu od playerja

    public uint[] possible_melee_weapons = new uint[2];
    private int previous_weapon;
    private int currently_equipped_weapon = 0;
    public int index_of_currently_selected_weapon_from_equipped_weapons
    {
        get { return currently_equipped_weapon; }
        set
        {
            if (currently_equipped_weapon == value) return;
            this.previous_weapon = index_of_currently_selected_weapon_from_equipped_weapons;
            currently_equipped_weapon = value;
            if (Current_weapon_change_event != null)
                Current_weapon_change_event(currently_equipped_weapon);
        }
    }
    //---------------------------------------------------------------------------------------DELEGATES--------------------------------------------------------------- they dont do shit???
    public delegate void On_Current_Weapon_Variable_Change_Delegate(int newVal);
    public event On_Current_Weapon_Variable_Change_Delegate Current_weapon_change_event;
    private void On_Current_weapon_changed(int newVal)
    {
        Debug.Log(this.previous_weapon + "WEAPON HAS BEEN CHANGED! " + newVal);
        animator.SetInteger("weapon_animation_class", getWeaponClassForAnimator(equipped_weapons[newVal]));
        if (combat_mode == 0) return;
        if (networkObject.IsOwner)
        {
            this.weapon_slot.GetChild(equipped_weapons[previous_weapon]).gameObject.SetActive(false);
            this.weapon_slot.GetChild(equipped_weapons[newVal]).gameObject.SetActive(true);
            this.weapon_slot.GetChild(equipped_weapons[newVal]).gameObject.GetComponent<Collider>().enabled = false;

            networkObject.SendRpc(RPC_CHANGE_CURRENT_WEAPON, Receivers.OthersProximity, equipped_weapons[newVal], equipped_weapons[this.previous_weapon]);
            Debug.Log("owner poslal rpc da clienti updejtajo njegov trenutni weapon");
        }
    }

    private int getWeaponClassForAnimator(int v)//tole bo treba updejtat i guess.
    {
        if (this.equipped_weapons[v] < 2) return 0;
        return Mapper.instance.getItemById(this.equipped_weapons[v]).weapon_animation_class;
    }

    //------------------------------------------------------------------------------------------NETWORKING-----------------------------------------------------------

    private void Start()
    {
        this.equipped_weapons = new int[5];//unarmed,unarmed block,wep0,wep1,ranged
        this.equipped_weapons[0] = 0;
        this.equipped_weapons[1] = 1;
        this.equipped_weapons[2] = 0;
        this.equipped_weapons[3] = 0;
        this.currently_equipped_shield = 1;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player_local_locks = GetComponent<player_local_locks>();
        stats = GetComponent<NetworkPlayerStats>();
        networkPlayerInventory = GetComponent<NetworkPlayerInventory>();
    }
    protected override void NetworkStart()
    {
        base.NetworkStart();
        this.Current_weapon_change_event += On_Current_weapon_changed; //registriramo delegata
        this.Current_shield_changed_event += On_Current_shield_changed;
    }

    private void Update()
    {
        if (networkObject == null) {
            Debug.LogError("networkObject is null.");
            return; }
        if (!networkObject.IsOwner)
        {
            //Debug.Log("Got combat mode " + networkObject.combatmode);
            this.combat_mode = networkObject.combatmode;//to bo dat v rpc

            handle_animations_from_rpcs();
            return;
        }
        if (networkPlayerInventory.panel_inventory.activeSelf) return;

        check_and_handle_combat();//keyboard input glede combata
        networkObject.combatmode = this.combat_mode; //ce bo treba zmanjsevat bandwidth lahko tole zamenjamo z rpc ampak je treba nrdit buffered rpc al pa nekejker se sicer lahko zgodi da bi biu en u combat mode in pride do playerja in bi ga ta player vidu da ni u combat modu. that causes problems. ce bomo sploh mel te mode al nevm
    }

    internal void setCurrentWeaponToFirstNotEmpty()//overrides what is currently selected
    {
        for (int i = 0; i < 5; i++) if (equipped_weapons[i] > 1) this.index_of_currently_selected_weapon_from_equipped_weapons = i;
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
        Debug.Log("Trying to perform block!");
        return true;
    }

    private void check_for_weapon_switch()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && !this.blocking) // forward - menja weapone. unarmed fist - unarmed block se skippa vmes - weapon0 - weapon1 - ranged
        {

            update_equipped_weapons();//tole mislm da je dodolj pohendlan ze u networkPlayerInventory


            //djmo scrollat samo prek weaponov k niso unarmed. nocmo trikat misko premaknt ker je povsod unarmed
            if (this.equipped_weapons[this.index_of_currently_selected_weapon_from_equipped_weapons] == 0) this.index_of_currently_selected_weapon_from_equipped_weapons = 0;
            int next_index= (this.index_of_currently_selected_weapon_from_equipped_weapons+1)%5;
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

            this.index_of_currently_selected_weapon_from_equipped_weapons = next_index;
        }
    }

    public void update_equipped_weapons()
    {
        // this.equipped_weapons[0] = 0;//unarmed
        // this.equipped_weapons[1] = 1;//unarmed block
        this.equipped_weapons[2] = networkPlayerInventory.GetWeapon0();
        this.equipped_weapons[3] = networkPlayerInventory.GetWeapon1();
        this.equipped_weapons[4] = networkPlayerInventory.GetRanged();

        int prev_shield = this.currently_equipped_shield;
        this.currently_equipped_shield=networkPlayerInventory.GetShield();

        if (prev_shield != this.currently_equipped_shield) {//prslo je do spremembe shielda
            disable_all_shields();
            if (this.currently_equipped_shield != 1)
            {
                shield_slot.GetChild(GetChildIndexOfShieldFromId(this.currently_equipped_shield)).gameObject.SetActive(true);//ga takoj izrise
                networkObject.SendRpc(RPC_CHANGE_CURRENT_SHIELD, Receivers.OthersProximity, this.currently_equipped_shield);
            }
        }



        //ce je trenutno equipan item, ki ni v equipped weapons ga moramo deaktivirat.
        int index_prev = getSiblingIndexOfFirstActiveChild_Weapon();
        if (index_prev == -1) {
            //do nothing
        }
        else if (this.equipped_weapons[this.index_of_currently_selected_weapon_from_equipped_weapons] != index_prev)
        {
            this.index_of_currently_selected_weapon_from_equipped_weapons = 0;
            if (index_prev > -1)
            {
                this.weapon_slot.GetChild(index_prev).gameObject.SetActive(false);//tole bo treba najbrz spravt tud v rpc al pa nekej
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
        if (Input.GetButtonDown("Change combat mode"))
        {
            if (Combat_mode == 0)
            {
                Combat_mode = 1;
                if (networkObject.IsOwner)
                {

                    enable_current_weapon();
                    enable_current_shield();
                }
            }
            else
            {
                Combat_mode = 0;//tole ksnej stlacmo v delegata da loh klicemo evente
                reset_all_combat_related_animator_parameters();
                disable_all_possible_equipped_weapons();
                place_shield_on_back();
            }
        }

        if (animator.GetInteger("combat_mode") != (int)Combat_mode) animator.SetInteger("combat_mode", (int)Combat_mode);

    }

    private void place_shield_on_back()
    {
        Debug.Log("Treba implementirat da se da shield na hrbet.");
    }

    private void enable_current_shield()
    {
        if (!networkObject.IsOwner) return;
        if (this.currently_equipped_shield == 1) return;
        this.shield_slot.GetChild(GetChildIndexOfShieldFromId(this.currently_equipped_shield)).gameObject.SetActive(true);
        networkObject.SendRpc(RPC_CHANGE_CURRENT_SHIELD, Receivers.OthersProximity, this.currently_equipped_shield);
    }

    private void disable_all_possible_equipped_weapons()
    {
        foreach (Transform c in weapon_slot)
            c.gameObject.SetActive(false);
    }

    /// <summary>
    /// tole je da se weapon enabla takoj ko gre player v combat mode
    /// </summary>
    private void enable_current_weapon()
    {
        if (!networkObject.IsOwner) return;

        weapon_slot.GetChild(this.equipped_weapons[index_of_currently_selected_weapon_from_equipped_weapons]).gameObject.SetActive(true);

        networkObject.SendRpc(RPC_CHANGE_CURRENT_WEAPON, Receivers.OthersProximity, this.equipped_weapons[index_of_currently_selected_weapon_from_equipped_weapons], -1);
    }


    private void handle_animations_from_rpcs()//sprozi se samo za remote playerje da jim pohendla parametre v animatorju
    {

        animator.SetBool("combat_blocking", this.blocking);
        if (animator.GetInteger("combat_mode") != (int)Combat_mode) animator.SetInteger("combat_mode", (int)Combat_mode);
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

        if (this.weapon_slot.GetChild(this.equipped_weapons[this.index_of_currently_selected_weapon_from_equipped_weapons]).GetComponent<Collider>().enabled != active_r)
        {//change right colider
            this.weapon_slot.GetChild(this.equipped_weapons[this.index_of_currently_selected_weapon_from_equipped_weapons]).GetComponent<Weapon_collider_handler>().set_offensive_colliders(active_r);
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




    //--------------------------RPC's

    public override void NetworkFire1(RpcArgs args)
    {
        execute_main_attack_from_remote();
    }

    public override void NetworkFire2(RpcArgs args)
    {
        bool blocking = args.GetNext<bool>();
        this.blocking = blocking;
    }

    public override void NetworkFeign(RpcArgs args)
    {
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
    public override void ChangeCurrentWeapon(RpcArgs args)
    {
        if (networkObject.IsOwner) return;
        int new_weapon_id1 = args.GetNext<int>();
        int prev_weapon_id1 = args.GetNext<int>();

        animator.SetInteger("current_weapon", new_weapon_id1);
        if (prev_weapon_id1 == -1)
        {

                for (int i = 0; i < this.weapon_slot.childCount; i++)
                {
                    if (new_weapon_id1 != i) this.weapon_slot.GetChild(i).gameObject.SetActive(false);
                    else
                    {
                        this.weapon_slot.GetChild(i).gameObject.SetActive(true);
                        this.weapon_slot.GetChild(i).gameObject.GetComponent<Collider>().enabled = false;
                    }
                }
            
        }
        else
            this.weapon_slot.GetChild(prev_weapon_id1).gameObject.SetActive(false);
            this.weapon_slot.GetChild(new_weapon_id1).gameObject.SetActive(true);
            this.weapon_slot.GetChild(new_weapon_id1).gameObject.GetComponent<Collider>().enabled = false;
    }

    public override void ChangeCurrentShield(RpcArgs args)
    {
        if (networkObject.IsOwner) return;
        int new_id = args.GetNext<int>();

        if (new_id == 1)
        {
            disable_all_shields();
        }
        else {
            disable_all_shields();//kr tk
            int ch = GetChildIndexOfShieldFromId(new_id);
            shield_slot.GetChild(ch).gameObject.SetActive(true);
            shield_slot.GetChild(ch).GetComponent<Collider>().enabled = true;//nj kar skos detektira? ce ma player na hrbtu recimo bi blo najbrz fajn ce bi blokiral puscice
        }
    }
}
