using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System;

public class NetworkPlayerCombatHandler : NetworkPlayerCombatBehavior
{
    public byte combat_mode = 0; //0 = no combat, 1= combat  
    public bool blocking = false;
    public bool Blocking { set { this.blocking = value; } }
    public byte Combat_mode
    {
        get { return this.combat_mode; }
        set { this.combat_mode = value; }
    }
    private panel_bar_handler bar_handler;
    public bool in_attack_animation = false;
    private player_local_locks player_local_locks;
    private NetworkPlayerStats stats;

    private NetworkPlayerInventory networkPlayerInventory;
    public GameObject radial_menu;
    public GameObject[] combat_sound_effects;

    public Transform weapon_slot;
    public Transform shield_slot;

    public Predmet currently_equipped_shield;
    public Predmet currently_equipped_weapon;//melee in ranged

    private NetworkPlayerAnimationLogic animator;
    private UILogic uiLogic;

    private void disable_all_shields()
    {
        foreach(Transform c in shield_slot) {
            if (c.gameObject.activeSelf) c.gameObject.SetActive(false);
        }
    }

    private NetworkPlayerNeutralStateHandler neutralStateHandler;


    private int getWeaponClassForAnimator(int weapon_id)//tole bo treba updejtat i guess.
    {
        if (weapon_id < 2) return 0;
        return Mapper.instance.getItemById(weapon_id).weapon_animation_class;
    }

    //------------------------------------------------------------------------------------------NETWORKING-----------------------------------------------------------

    private void Awake()
    {
        animator = GetComponent<NetworkPlayerAnimationLogic>();
        player_local_locks = GetComponent<player_local_locks>();
        stats = GetComponent<NetworkPlayerStats>();
        networkPlayerInventory = GetComponent<NetworkPlayerInventory>();
        this.uiLogic = GetComponentInChildren<UILogic>();
        //this.radial_menu = transform.GetComponentInChildren<RMF_RadialMenu>().gameObject; -treba dat v start ker sicer crkne k ni se vse nrjen
        this.bar_handler = GetComponentInChildren<panel_bar_handler>();
        this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
    }

    internal Predmet GetCurrentlyActiveWeapon()
    {
        if (this.currently_equipped_weapon != null)
        {
            if (this.currently_equipped_weapon.item.type == Item.Type.weapon) return this.currently_equipped_weapon;

        }
        return null;

    }

    internal Predmet GetCurrentlyActiveShield()
    {
        return this.currently_equipped_shield;
    }

    internal Predmet GetCurrentlyActiveRanged()
    {
        if (this.currently_equipped_weapon != null)
            if (this.currently_equipped_weapon.item.type == Item.Type.ranged) return this.currently_equipped_weapon;
        return null;
    }


    private void initialize_weapons() {
        this.currently_equipped_weapon = null;
        this.currently_equipped_shield = null;
    }

    /// <summary>
    /// samo lokalno preveri da ne tezi drugim panelam. ce hacka se mora na serverju prevert
    /// </summary>
    /// <returns></returns>
    private bool is_allowed_to_attack_local() {
        if (stats.downed || stats.dead )
        {
            return false; //Ce je downan da nemora vec napadat pa take fore. to je precej loše ker je na clientu. ksnej bo treba prenest to logiko na server ker tole zjebe ze cheatengine
        }

        if (uiLogic.hasOpenWindow) return false; //odprt inventorij
        if (stats.guild_modification_panel.activeSelf) return false;
        if (this.radial_menu.activeSelf) return false;

        return true;
    }

    private void Update()
    {
        if (networkObject == null) {
            Debug.LogError("networkObject is null. - najbrz zato ker se se connecta gor.");
            return; }
        if (!networkObject.IsOwner || !is_allowed_to_attack_local())
        {
            return;
        }

        if (Input.GetButtonDown("Fire2")) {
            Debug.Log("sad");
            Debug.Log("fire2");
        }

        //input glede menjave orozja pa tega se izvaja v neutralStatehandlerju

        if (this.combat_mode == 1)
        {
            if (hasWeaponSelected()) {
                if (Input.GetButtonDown("Fire1") && !this.in_attack_animation)
                {
                    //fire  --------------------------//ce server nrdi tole tukaj potem faila rpc
                    if (!networkObject.IsServer) {
                        if (player_local_locks.fire1_available)
                            setup_Fire1_lock();
                        else
                            return;
                    }//--------------------------------ce server nrdi tole tukaj potem faila rpc
                    
                    networkObject.SendRpc(RPC_NETWORK_FIRE1, Receivers.Server);
                }
                else if (Input.GetButtonDown("Fire2") && !this.in_attack_animation && !this.blocking)
                {
                    //block
                    networkObject.SendRpc(RPC_NETWORK_FIRE2, Receivers.Server);
                }
                else if (Input.GetButtonUp("Fire2")) {
                    //nehov blokirat
                    networkObject.SendRpc(RPC_NETWORK_FIRE2, Receivers.Server);
                }//fejkanje
                if (Input.GetButtonDown("Fire2") && this.in_attack_animation)
                {
                    networkObject.SendRpc(RPC_NETWORK_FEIGN, Receivers.Server);
                }

            }
        }


        
    }

    /// <summary>
    /// klice animatiopn event ki je na koncu vsake attack animacije ter v animaciji, kjer pofejkas attack. v dodgu je tud ta animation event. kjerkoli pademo iz attack animacije uglavnem mora bit ta event
    /// </summary>
    public void handleEndOfAttackAnimation() {
        this.in_attack_animation = false;
    }

    /// <summary>
    /// pogleda kter weapon je trenutno izbran. pogleda samo field. field nastavi pa rpc iz inventorija - bar!
    /// </summary>
    /// <returns></returns>
    private bool hasWeaponSelected()
    {
        return this.currently_equipped_weapon != null;
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

    /// <summary>
    /// tole se klice na podlagi rpc-ja z serverja.
    /// </summary>
    public void handle_player_death() {
        Debug.Log("handling player death");
        foreach (Transform child in weapon_slot) child.gameObject.SetActive(false);//disabla weapone
        foreach (Transform child in shield_slot) child.gameObject.SetActive(false);//disabla shielde
    }

    public void handle_player_downed() {
        Debug.Log("handling player downed");
    }

    /// <summary>
    /// tole se klice takoj ko se zgodi networkUpdate za loadout na others, ter pred networkUpdate na serverju
    /// izrise na podlagi fieldov v tej skripti.
    /// </summary>
    public void update_equipped_weapons()
    {
 
            foreach (Transform c in weapon_slot)
            {
                if (this.currently_equipped_weapon != null)
                {
                    if (c.GetComponent<Weapon_collider_handler>().item.id == this.currently_equipped_weapon.item.id)
                    {
                        c.gameObject.SetActive(true);
                    }
                    else
                    {
                        c.gameObject.SetActive(false);
                    }
                }
                else {
                    c.gameObject.SetActive(false);
                    //treba tud pohendlat animacijo da vrze iz combat state-a. lahko klicemo kr combatstatesetter - ker se to nastavi na vsah playerjih, tud na serverju.
                    setCombatStateLocally(0);
                }
            }
        
            foreach (Transform c in shield_slot)
            {
                if (this.currently_equipped_shield != null)
                {
                    if (c.GetComponent<identifier_helper>().id == this.currently_equipped_shield.item.id)
                    {
                        c.gameObject.SetActive(true);
                    }
                    else
                    {
                        c.gameObject.SetActive(false);
                    }
                }
                else {
                    c.gameObject.SetActive(false);
                }
            }
            
    }


    private void place_shield_on_back()
    {
        //Debug.Log("Treba implementirat da se da shield na hrbet.");
    }


    IEnumerator StartFire1Lock(float timeout)
    {
        if (networkObject.IsServer)
        {
            player_local_locks.fire1_available = false;
            yield return new WaitForSeconds(timeout);
            player_local_locks.fire1_available = true;
        }
    }

    private void setup_Fire1_lock()
    {
        StartCoroutine(StartFire1Lock(stats.fire1_cooldown));
    }



    private void play_main_attack_sound_effect()
    {
        //poisc kter je taprav sound effect za predvajat, zaenkrat je samo edn
        GameObject clip = GameObject.Instantiate(combat_sound_effects[0]);
        clip.transform.parent = transform;
    }

    private int getSiblingIndexOfFirstActiveChild_Weapon()
    {
        int k = -1;
        for (int j = 0; j < weapon_slot.childCount; j++)
            if (weapon_slot.GetChild(j).gameObject.activeSelf)
                return j;
        return k;
    }

    /// <summary>
    /// Na serverju se aktivira collider na weaponu s ktermu napadamo. to metodo naj bi klical animation event.
    /// </summary>
    public void activate_weapon_collider_server(int b)
    {
        if (!networkObject.IsServer) return;
        bool active = false;
        if (b > 0) active = true;
        Debug.Log("Activating colliders  " +  active);

        foreach (Transform child in weapon_slot) {
            if (child.GetComponent<Weapon_collider_handler>().item.id == this.currently_equipped_weapon.item.id) {
                child.GetComponent<Weapon_collider_handler>().set_offensive_colliders(active);
            }
        }
    }


    //--------------------------RPC's

    public override void NetworkFire1(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            if (hasWeaponSelected() && !this.in_attack_animation && player_local_locks.fire1_available) {
                setup_Fire1_lock();
                networkObject.SendRpc(RPC_NETWORK_FIRE1_RESPONSE, Receivers.All);
            }
        }

    }

    /// <summary>
    /// to je ubistvu toggle. menja med blokiranjem
    /// </summary>
    /// <param name="args"></param>
    public override void NetworkFire2(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            if (this.blocking) {//stops blocking
                networkObject.SendRpc(RPC_NETWORK_FIRE2_RESPONSE, Receivers.All, true);
            } else if (!this.in_attack_animation && !this.blocking) {//starts blocking
                networkObject.SendRpc(RPC_NETWORK_FIRE2_RESPONSE, Receivers.All, true);
            }
        }
    }

    public override void NetworkFeign(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            if (this.in_attack_animation) {//pofejkej
                //TODO: rpc za fejkanje response
                networkObject.SendRpc(RPC_NETWORK_FEIGN_RESPONSE, Receivers.All);
            }
        }
    }

    ///klice se potem, ki ze zamenjamo item z hotbara.
    public void ChangeCombatMode(Item i)
    {
        if (!networkObject.IsServer) return;
        // Debug.Log("server : got change combat mode request");
        int next = 0;
        if (i != null)
            if (i.type == Item.Type.weapon || i.type == Item.Type.ranged)
                next = 1;

        networkObject.SendRpc(RPC_CHANGE_COMBAT_MODE_RESPONSE, Receivers.All, next);
        
    }

    /// <summary>
    /// prejmejo vsi od serverja. setter
    /// </summary>
    /// <param name="args"></param>
    public override void ChangeCombatModeResponse(RpcArgs args)//ALL GET IT, EVEN SERVER
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;
        
        int new_mode = args.GetNext<int>();

        setCombatStateLocally((byte)new_mode);
    }

    private void setCombatStateLocally(byte new_mode) {
        this.combat_mode = (byte)new_mode;
        // Debug.Log("got change combat mode response : "+this.combat_mode + " "+new_mode);


        if (new_mode == 0)
        {
            animator.setCombatState((byte)new_mode);
            place_shield_on_back();
            neutralStateHandler.NeutralStateSetup();
        }
        else
        {
            neutralStateHandler.CombatStateSetup();
            update_equipped_weapons();
            animator.setCombatState((byte)new_mode);
        }
    }
 
    public override void SendAll(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            this.combat_mode = (byte)args.GetNext<int>();
            this.blocking = args.GetNext<bool>();
            this.currently_equipped_weapon = Predmet.createNewPredmet(args.GetNext<string>());
            this.currently_equipped_shield = Predmet.createNewPredmet(args.GetNext<string>());
            update_equipped_weapons();
        }
    }

    internal void ServerSendAll(NetworkingPlayer p)
    {
        if (networkObject.IsServer)
        {

            //int id_wep = (this.currently_equipped_weapon.item == null) ? -1 : this.currently_equipped_weapon.item.id;

            networkObject.SendRpc(p, RPC_SEND_ALL, (int)this.combat_mode, this.blocking, (this.currently_equipped_weapon == null) ? "-1" : this.currently_equipped_weapon.toNetworkString(), (this.currently_equipped_shield == null) ? "-1" : this.currently_equipped_shield.toNetworkString());
        }
    }

    public override void NetworkFire1Response(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            if(networkObject.IsServer)
            {//do server stuff
             //coolliderji se aktivirajo na animation eventu (activate_weapon_collider_server())
            }
            //vsi - animacije
            animator.setFire1();//animator in inAttackAnimation
        }

    }

    public override void NetworkFire2Response(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {//do server stuff - collider? - its always on so..
            animator.setCombatBlocking(args.GetNext<bool>());
        }
    }

    /// <summary>
    /// poslje server. pomen d amormo nastavt parametre za pofejkat
    /// </summary>
    /// <param name="args"></param>
    public override void NetworkFeignResponse(RpcArgs args)
    {
        if(args.Info.SendingPlayer.NetworkId==0)
            if (this.in_attack_animation) {
                this.in_attack_animation = false;
                animator.setFeign();
            }
    }
}
