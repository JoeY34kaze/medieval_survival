using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System;

public class NetworkPlayerCombatHandler : NetworkPlayerCombatBehavior
{
    private byte combat_mode = 0; //0 = no combat, 1= combat  
    private bool blocking = false;
    public bool Blocking {
        get { return this.blocking; }
        set { this.blocking = value; }
    }
    public byte Combat_mode
    {
        get { return this.combat_mode; }
        set { this.combat_mode = value; }
    }


    private NetworkPlayerStats stats;

    public Transform weapon_slot;
    public Transform shield_slot;

    [HideInInspector] internal Predmet currently_equipped_shield = null;
    [HideInInspector] internal Predmet currently_equipped_weapon = null;

    private NetworkPlayerAnimationLogic animator;

    public byte weapon_direction = 0;
    public bool is_readying_attack = false;
    public bool ready_attack = false;
    public bool executing_attack = false;

    public bool locally_buffered_execute_attack_request = false;

    private void disable_all_shields()
    {
        foreach (Transform c in shield_slot) {
            if (c.gameObject.activeSelf) c.gameObject.SetActive(false);
        }
    }

    public bool is_in_action_that_merits_movement_speed_slow() {
        return this.is_readying_attack || this.ready_attack || this.executing_attack || this.Blocking;
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

        stats = GetComponent<NetworkPlayerStats>();
        //this.radial_menu = transform.GetComponentInChildren<RMF_RadialMenu>().gameObject; -treba dat v start ker sicer crkne k ni se vse nrjen
        this.neutralStateHandler = GetComponent<NetworkPlayerNeutralStateHandler>();
    }

    private void Update()
    {
        if (networkObject == null) {
            Debug.LogWarning("networkObject is null. - najbrz zato ker se se connecta gor.");
            return; }




        if (!networkObject.IsOwner || !is_allowed_to_attack_local())
        {
            return;
        }
        checkAttackDirection();


        if (Input.GetButtonDown("Fire2")) {

            Debug.Log("fire2");
        }

        //input glede menjave orozja pa tega se izvaja v neutralStatehandlerju
        if (this.combat_mode == 0) ResetAllCombatParameters();
        else if (this.combat_mode == 1)
        {
            if (hasWeaponSelected())
            {
                if (Input.GetButtonDown("Fire1") && !this.executing_attack && !this.is_readying_attack && !this.ready_attack && !this.blocking)
                {
                    locally_buffered_execute_attack_request = false;

                    networkObject.SendRpc(RPC_START_ATTACK_REQUEST, Receivers.Server, this.weapon_direction);
                }
                else if (Input.GetButtonDown("Fire2"))
                {
                    //block
                    networkObject.SendRpc(RPC_START_BLOCK_REQUEST, Receivers.Server, this.weapon_direction);
                }
                else if (Input.GetButtonUp("Fire2"))
                {
                    //nehov blokirat
                    networkObject.SendRpc(RPC_STOP_BLOCK_REQ, Receivers.Server);
                }
                if (Input.GetButtonUp("Fire1"))
                {
                    if (ready_attack)
                        networkObject.SendRpc(RPC_EXECUTE_ATTACK_REQUEST, Receivers.Server);
                    else
                        this.locally_buffered_execute_attack_request = true;
                }
            }
            else if (this.currently_equipped_shield != null)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    //block
                    networkObject.SendRpc(RPC_START_BLOCK_REQUEST, Receivers.Server, this.weapon_direction);
                }
                else if (Input.GetButtonUp("Fire2"))
                {
                    //nehov blokirat
                    networkObject.SendRpc(RPC_STOP_BLOCK_REQ, Receivers.Server);
                }
            }
        }

    }

    private void ResetAllCombatParameters()
    {

    combat_mode=0;
    blocking = false;
    is_readying_attack = false;
    ready_attack = false;
    executing_attack = false;
    locally_buffered_execute_attack_request = false;
}
    internal void ResetAllParametersRelatedToAttack() {
        is_readying_attack = false;
        ready_attack = false;
        executing_attack = false;
        locally_buffered_execute_attack_request = false;
    }

    private void checkAttackDirection() {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        float xx = x * x;
        float yy = y * y;
        // Debug.Log(x + " " + y);

        if (y > 0 && xx < yy) { if (this.weapon_direction != 0) { this.weapon_direction = 0; UILogic.Instance.OnWeaponDirectionChanged(0); } }
        else if (y < 0 && xx < yy) { if (this.weapon_direction != 2) { this.weapon_direction = 2; UILogic.Instance.OnWeaponDirectionChanged(2); } }
        else if (x > 0 && xx > yy) { if (this.weapon_direction != 1) { this.weapon_direction = 1; UILogic.Instance.OnWeaponDirectionChanged(1); } }
        else if (x < 0 && xx > yy) { if (this.weapon_direction != 3) { this.weapon_direction = 3; UILogic.Instance.OnWeaponDirectionChanged(3); } }

    }


    internal Predmet GetCurrentlyActiveWeapon()
    {
        if (this.currently_equipped_weapon != null)
        {
            if (this.currently_equipped_weapon.getItem().type == Item.Type.weapon) return this.currently_equipped_weapon;

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
            if (this.currently_equipped_weapon.getItem().type == Item.Type.ranged) return this.currently_equipped_weapon;
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
    public bool is_allowed_to_attack_local() {
        if (stats.downed || stats.dead)
        {
            return false; //Ce je downan da nemora vec napadat pa take fore. to je precej loše ker je na clientu. ksnej bo treba prenest to logiko na server ker tole zjebe ze cheatengine
        }

        if (UILogic.Instance.hasOpenWindow) return false; //odprt inventorij
        if (UILogic.Instance.isRadialMenuOpen()) return false;

        return true;
    }



    /// <summary>
    /// klice animatiopn event ki je na koncu vsake attack animacije
    /// </summary>
    public void handleEndOfAttackAnimation() {
        if (this.is_readying_attack || this.ready_attack || this.executing_attack)
        {
            animator.setFeign();
        }
        this.ready_attack = false;
        this.is_readying_attack = false;
        this.executing_attack = false;

        animator.reset_swing_IK();
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

    public void OnAttackReady() {
        this.is_readying_attack = false;
        animator.reset_readying_attack();
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
                if (c.GetComponent<Weapon_collider_handler>().item.id == this.currently_equipped_weapon.getItem().id)
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
            }
        }

        foreach (Transform c in shield_slot)
        {
            if (this.currently_equipped_shield != null)
            {
                if (c.GetComponent<identifier_helper>().id == this.currently_equipped_shield.getItem().id)
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







    private void play_main_attack_sound_effect()
    {
        //poisc kter je taprav sound effect za predvajat, zaenkrat je samo edn
        SFXManager.OnWeaponAttackSFX(transform,this.currently_equipped_weapon.getItem());
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
    /// 
    /// ALSO!!!! te metode ne slice samo server ampak tud lokalni player!!! zarad IK
    /// </summary>
    public void activate_weapon_collider_server(int b)
    {
        if (networkObject.IsServer || networkObject.IsOwner)
        {
            bool active = false;
            if (b > 0) active = true;

            foreach (Transform child in weapon_slot)
            {
                if(this.currently_equipped_weapon!=null)
                    if (child.GetComponent<Weapon_collider_handler>().item.id == this.currently_equipped_weapon.getItem().id)
                    {
                        child.GetComponent<Weapon_collider_handler>().set_offensive_colliders(active);
                    }
                else
                    child.GetComponent<Weapon_collider_handler>().set_offensive_colliders(false);//zamenjov weapon/tool rabmo disablat
            }
        }
    }

    public void set_weapon_blocking_collider_server(bool b)
    {
        if (!networkObject.IsServer) return;
        if (this.currently_equipped_weapon == null) return;
        Debug.Log("Activating blocking colliders  " + b);

        foreach (Transform child in weapon_slot)
        {
            if (child.GetComponent<Weapon_collider_handler>().item.id == this.currently_equipped_weapon.getItem().id)
                child.GetComponent<Weapon_collider_handler>().set_defensive_colliders(b);

        }
    }
    ///klice se potem, ki ze zamenjamo item z hotbara.
    public void ChangeCombatMode(Item i)
    {

        if (!networkObject.IsServer) return;
        // Debug.Log("server : got change combat mode request");
        int next = 0;
        if (i != null)
        {
            if (i.type == Item.Type.weapon || i.type == Item.Type.ranged || i.type == Item.Type.shield || i.type == Item.Type.tool)
                next = 1;
        }
        networkObject.SendRpc(RPC_CHANGE_COMBAT_MODE_RESPONSE, Receivers.All, next);

    }

    internal void OnRemotePlayerDataSet()
    {
        if (currently_equipped_weapon != null)
            ChangeCombatMode(currently_equipped_weapon.getItem());
    }

    private void setCombatStateLocally(byte new_mode)
    {
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

    internal void SetShield(Predmet s) {
        this.currently_equipped_shield = s;
        set_shield_collider(true);
        animator.onShieldChanged(s);
    }

    internal void set_shield_collider(bool v)
    {
        for (int i = 0; i < this.shield_slot.childCount; i++) { 

        this.shield_slot.GetChild(i).gameObject.GetComponent<Collider>().enabled = v;
    }
}

    //--------------------------RPC's




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

   
    public override void SendAll(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            this.combat_mode = (byte)args.GetNext<int>();
            this.blocking = args.GetNext<bool>();
            this.currently_equipped_weapon = args.GetNext<byte[]>().ByteArrayToObject<Predmet>();
            this.currently_equipped_shield = args.GetNext<byte[]>().ByteArrayToObject<Predmet>();
            update_equipped_weapons();
            this.weapon_direction = args.GetNext<byte>();
            this.is_readying_attack = args.GetNext<bool>();
        }
    }

    internal void ServerSendAll(NetworkingPlayer p)
    {
        if (networkObject.IsServer)
        {

            //int id_wep = (this.currently_equipped_weapon.item == null) ? -1 : this.currently_equipped_weapon.item.id;

            networkObject.SendRpc(p, RPC_SEND_ALL, (int)this.combat_mode, this.blocking, this.currently_equipped_weapon.ObjectToByteArray(),  this.currently_equipped_shield.ObjectToByteArray(), this.weapon_direction, this.is_readying_attack, this.ready_attack,this.executing_attack);
        }
    }


    public override void start_attack_request(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            byte dir = args.GetNext<byte>();
            if (dir > 3) dir = 3;
            if (!this.executing_attack && !ready_attack && !is_readying_attack && !blocking) { //ce je v zacetnem stanju
                if (this.currently_equipped_weapon != null) {
                    networkObject.SendRpc(RPC_START_ATTACK_RESPONSE, Receivers.All, true, dir);
                }
            }
        }
    }

    public override void start_attack_response(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.ready_attack = false;
            this.executing_attack = false;
            this.locally_buffered_execute_attack_request = false;
            this.is_readying_attack = args.GetNext<bool>();
            if(this.is_readying_attack)
                animator.handle_readying_of_attack(args.GetNext<byte>());
        }
    }

    /// <summary>
    /// animation event ki mora bit naliman na animaciji
    /// </summary>
    public void on_attack_ready() {
        if (networkObject.IsOwner || networkObject.IsServer) {
            this.is_readying_attack = false;
            this.ready_attack = true;

            if (networkObject.IsOwner && this.locally_buffered_execute_attack_request)
                networkObject.SendRpc(RPC_EXECUTE_ATTACK_REQUEST, Receivers.Server);
        }
    }

    public override void execute_attack_request(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            if (this.ready_attack) { //vedno true razen ce so e paketi zgubil?? ali pa ce hacka
                networkObject.SendRpc(RPC_EXECUTE_ATTACK_RESPONSE, Receivers.All, true);
            }
        }
    }

    public override void execute_attack_response(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.ready_attack = false;
            this.executing_attack = true;
            animator.handle_execution_of_attack();
        }
    }


    public override void start_block_request(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            if (!this.blocking && !this.executing_attack)
            {//starts blocking
                if (currently_equipped_shield == null)
                {
                    set_weapon_blocking_collider_server(true);
                }
                networkObject.SendRpc(RPC_START_BLOCK_RESPONSE, Receivers.All, args.GetNext<byte>());

            }

        }
    }

    public override void start_block_response(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            byte dir = args.GetNext<byte>();
            this.blocking = true;

            //reestiramo vsi kar ima veze z attackom ker je ali prešel z attackanja na block ali pa z blokiranja v nevtralsnot.
            this.locally_buffered_execute_attack_request = false;
            this.is_readying_attack = false;
            this.ready_attack = false;
            this.executing_attack = false;

            animator.setCombatBlocking(blocking, dir);
        }
    }

    public override void stop_block_req(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            networkObject.SendRpc(RPC_STOP_BLOCK_RESP, Receivers.All);
        }
    }

    public override void stop_block_resp(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.blocking = false;
            animator.setCombatBlocking(false, 0);
        }
    }

    internal void RefreshCombatState(Predmet p)
    {
        if(p!=null)
            if (p.getItem().type != Item.Type.tool) p = null;//da je spodnja metoda bolj pregledna odrezemo stran vse kar ni tool, ker p nas zanima samo ce je tool

        //ZA COMBAT MODE - precej neefektivno ker pri menjavi itema na baru se klice dvakrat rpc...........
        if (this.currently_equipped_weapon != null)
            this.ChangeCombatMode(this.currently_equipped_weapon.getItem());
        else if (this.currently_equipped_shield != null)
            this.ChangeCombatMode(this.currently_equipped_shield.getItem());
        else if (p != null) 
           this.ChangeCombatMode(p.getItem());
        else//nimamo nc equipan
            this.ChangeCombatMode(null);
    }
}

