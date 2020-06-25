using UnityEngine;
using UnityEngine.UI;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using System.Collections.Generic;
using System.Collections;

public class NetworkPlayerStats : NetworkPlayerStatsBehavior
{
    public bool test = false;

    public string playerName="Janez Kranjski";

    public bool inDodge = false;
    public bool downed = false;
    public bool dead = false;
    public float max_health = 255;
    public float health = 255;//for debug purposes, its not being called from any other script that i made, its public just so that i can see it easier in inspector
    public Image healthBar;

    public Text player_displayed_name;
    public Text inventory_guild_name;

    public float head_damage_multiplier = 1.5f;
    public float torso_damage_multiplier = 1.0f;
    public float limb_damage_multiplier = 0.75f;

    public float block_damage_reduction = 0.025f;  //to bomo pobral z itema



    public float death_timer = 10f;

    internal float fire1_cooldown = 0.6f;

    private NetworkPlayerInventory npi;

    internal uint[] team; //array networkId-jev team memberjev. server vedno hrani to vrednost za vse playerje. drugi dobijo samo update od serverja

    private List<uint> already_processed_inviters;
    private bool team_invite_pending = false;

    public string name_guild="no guild yet";

    public string tag_guild="no tag yet";

    public Color color_guild=Color.red;
    public byte[] image_guild;



    public GameObject serverSide_guildManager;
    public panel_guild_handler panelGuildMemberHandler;
    private float original_capsule_collider_height;

    private NetworkPlayerStats executionTarget;

    private NetworkPlayerCombatHandler combatStateHandler;


    private float usual_character_controller_height;
    private Vector3 usual_character_controller_center;

    private IEnumerator death_timer_coroutine;


    private float ping_timer_helper=0f;


    [Range(0,5000)]
    [SerializeField] public int simulated_ms_delay;
    [Range(0, 1)]
    [SerializeField] public float simulated_packet_loss;
    [SerializeField] public bool update_network_throttling;
    private ulong prev_band_out;
    private ulong prev_band_in;

    private void Start()
    {
        
        this.npi = GetComponent<NetworkPlayerInventory>();
        combatStateHandler = GetComponent<NetworkPlayerCombatHandler>();

    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        // TODO:  Your initialization code that relies on network setup for this object goes here
        //networkObject.SendRpc(RPC_UPDATE_ALL_PLAYER_ID, Receivers.Server);
        //this.server_id = NetworkManager.Instance.Networker.Me.NetworkId; -- SAMO ZA DEBUGGING CE BO TREBA POGLEDAT V INSPEKTORJU ID AL PA KEJ
        //this.playerName = "Janez Kranjski";
        //updateDisplayName();
        if (networkObject.IsServer) NetworkGuildManager.Instance.OnStartup();

        if (networkObject.IsOwner)
        {
            UILogic.set_local_player_gameObject(gameObject);
            healthBar = GameObject.Find("health_fg").GetComponent<Image>();
            transform.Find("canvas_player_overhead").gameObject.SetActive(false);
            FloatingTextController.Initialize();
            reticle_hit_controller.Initialize();

            NetworkManager.Instance.Networker.onPingPong += (ping, sender) =>
            {
                MainThreadManager.Run(() =>
                {
                    //Debug.Log("sender: "+sender+" ping: "+ (int)ping +"  timestamp:"+ NetworkManager.Instance.Networker.Time.Timestep + "  bandwidth out: " + NetworkManager.Instance.Networker.BandwidthOut + " | bandwidth IN: " + NetworkManager.Instance.Networker.BandwidthIn +" || bandwidth/s- OUT: "+( NetworkManager.Instance.Networker.BandwidthOut-this.prev_band_out)+" | IN: "+ (NetworkManager.Instance.Networker.BandwidthIn-this.prev_band_in));
                    this.prev_band_in = NetworkManager.Instance.Networker.BandwidthIn;
                    this.prev_band_out = NetworkManager.Instance.Networker.BandwidthOut;

                    UILogic.Instance.setLatencyText(ping);
                });
            };
        }
        


        //----------------------------SERVER SAVES SCRIPT DATA WHEN PLAYER DISCONNECTS ----------------------------------------------------------------------------------------
        if (networkObject.IsServer && !networkObject.IsOwner)//&& !networkObject.IsOwner mislm da je brezpredmeten ker ko se player dc-ja vzame server ownership...pustu bom tle ker zaenrkat dela
        {
            NetworkManager.Instance.Networker.playerDisconnected += (networkingPlayer_that_has_disconnected, disconnecting_networker) =>
            {
                if(!networkingPlayer_that_has_disconnected.IsHost)
                    MainThreadManager.Run(() =>
                    {
                        save_player_on_disconnect(networkingPlayer_that_has_disconnected, disconnecting_networker);
                    });
            };
        }//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

        if (!networkObject.IsServer)
        {
            //server se itak nemore disconnectat sam od sebe
            NetworkManager.Instance.Networker.disconnected += OnLocalClientDisconnected;
           
        }
        if(networkObject.IsServer && !networkObject.IsOwner) {
            load_player_data_on_connected();
            send_other_players_data_to_newly_connected_player(networkObject.Owner);
        }

    }

    /// <summary>
    /// klice se z NetworkGuildManager ko se vse vzpostavi. pohandla vse kar je v zvezi z guildi
    /// </summary>
    internal void startup_guild_manager_handler()
    {
        ///klice se na serverju. pogleda ce pripada ksnemu guildu sicer naredi novega
        NetworkGuildManager.Guild g = NetworkGuildManager.Instance.GetGuildFromSteamId(666);
        if (g == null) {
            g = NetworkGuildManager.Instance.CreateGuild(networkObject.Owner.NetworkId,this.playerName+"'s Guild","","red",null);
        }
        NetworkGuildManager.Instance.sendUserInfoResponseTo(networkObject.Owner);
    }


    //od DRUGIH not k playerju
    private void send_other_players_data_to_newly_connected_player(NetworkingPlayer owner)
    {
        //ubistvu moramo dobit vse skripte na playerju, spestat najboljs da v en rpc (ker je potem flow sinhron, sicer leti 5 rpcjev na yolo k rabjo bit u dolocenem vrstnem redu) in poslat playerju
        //skript je 7

        foreach (NetworkPlayerStats stats in GameObject.FindObjectsOfType<NetworkPlayerStats>()) {
            if (stats.networkObject.Owner.Equals(networkObject.Owner))
            {
                continue;
            }
            else {
                stats.send_this_player_data_to_newly_connected_player( owner);
            }
        }
    }

    private void send_this_player_data_to_newly_connected_player( NetworkingPlayer new_player)
    {
        //skript je 7 indolocene stvari so ze pohandlane, nekatere pa manjkajo tko da gremo cez vse


        //interaction inma nc
        //movement je treba vsaj pozicijo poslat, ker sce se player ne premakne odkat se je client connectov ga nebo vidu pol se pa teleportira k njemu. omae wa mou shindeiru
        NetworkPlayerMovement mov = GetComponent<NetworkPlayerMovement>();
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        bool crouched = mov.isCrouched;
        //---------------Stats 
        string displayName = this.playerName;
        bool downed = this.downed;
        bool dead = this.dead;
        float health = this.health;
        //---------------------INVENTORY 0
        NetworkPlayerInventory inv = GetComponent<NetworkPlayerInventory>();

        Predmet head = inv.head;
        Predmet chest = inv.chest;
        Predmet hands = inv.hands;
        Predmet legs = inv.legs;
        Predmet feet = inv.feet; //na drug stran rpcja je treba klicat onLoaddoutChanged();

        //--------------------- NEUTRALSTATEHANDLER 1
        NetworkPlayerNeutralStateHandler neut = GetComponent<NetworkPlayerNeutralStateHandler>();
        Predmet tool = neut.activeTool;
        //---------------------COMBAT 2
        //tle se najde tud trenutni weapon
        NetworkPlayerCombatHandler c = GetComponent<NetworkPlayerCombatHandler>();
        byte combat_State = c.Combat_mode;
        bool blocking = c.Blocking;
        Predmet equipped_shield = c.currently_equipped_shield;
        Predmet equipped_weapon = c.currently_equipped_weapon;
        byte direction = c.weapon_direction;
        bool is_readying = c.is_readying_attack;
        bool ready_atk = c.ready_attack;
        bool exec_atk = c.executing_attack;
        //--------------------- ANIMATION 3
        //animacije bom ubistvu DIREKT preslikov trenutno stanje kot je. ce se da forsirat state machine bi blo awesome.  skripta ne hrani cist nic tko da me zanima izkljucno samo Animator
        Animator an = GetComponent<Animator>();

        int an_combat = an.GetInteger("combat_mode");
        //walking vertical
        //ready_attack trigger pomoje skippamo? you cant rly get trigger
        int an_dir = an.GetInteger("attack_direction");
        bool an_shield = an.GetBool("shield_equipped");
        bool an_blocking = an.GetBool("combat_blocking");
        bool an_crouched = an.GetBool("crouched");
        bool an_grounded = an.GetBool("grounded");
        int an_weapon_anim_cl = an.GetInteger("weapon_animation_class");


        PlayerSynchronizationContainer Player_Data_Full = new PlayerSynchronizationContainer(
             pos,
             rot,
             crouched,
             displayName,
             downed,
             dead,
             health,
             head,
             chest,
             hands,
             legs,
             feet,
             tool,
             combat_State,
             blocking,
             equipped_shield,
             equipped_weapon,
             direction,
             is_readying,
             ready_atk,
             exec_atk,
             an_combat,
             an_dir,
             an_shield,
             an_blocking,
             an_crouched,
             an_grounded,
             an_weapon_anim_cl
        );

        networkObject.SendRpc(new_player, RPC_SERVER_SEND_ALL_THIS_TO_NEW_PLAYER, Player_Data_Full.ObjectToByteArray());

    }

    private void update_non_owner_gameObject_with_data_on_startup(PlayerSynchronizationContainer data) {
        Debug.Log("Started update of player with owner  networkid " + networkObject.Owner.NetworkId);



        Debug.Log("Updating movement for " + networkObject.Owner.NetworkId);
        NetworkPlayerMovement mov = GetComponent<NetworkPlayerMovement>();
        transform.position=new Vector3(data.pos_x,data.pos_y,data.pos_z);//Vector3 in Quaternion nosta serializable... nc od unity ni serializable pac
        transform.rotation = new Quaternion(data.rot_x, data.rot_y, data.rot_z,data.rot_w);
        mov.isCrouched=data.crouched;

        mov.OnRemotePlayerDataSet();

        Debug.Log("Movement updated for " + networkObject.Owner.NetworkId);

        //---------------Stats 
        Debug.Log("Updating stats for " + networkObject.Owner.NetworkId);
        this.playerName = data.displayName;
        this.downed = data.downed;
        this.dead=data.dead;
        this.health=data.health;

        OnRemotePlayerDataSet();
        

        //---------------------INVENTORY 0
        Debug.Log("Updating inventory for " + networkObject.Owner.NetworkId);
        NetworkPlayerInventory inv = GetComponent<NetworkPlayerInventory>();

        inv.head=data.head;
        inv.chest=data.chest;
        inv.hands=data.hands;
        inv.legs=data.legs;
        inv.feet=data.feet; //na drug stran rpcja je treba klicat onLoaddoutChanged();
        inv.OnRemotePlayerDataSet();

        //--------------------- NEUTRALSTATEHANDLER 1
        Debug.Log("Updating neutralStateHandler for " + networkObject.Owner.NetworkId);
        NetworkPlayerNeutralStateHandler neut = GetComponent<NetworkPlayerNeutralStateHandler>();
        neut.activeTool=data.tool;

        neut.OnRemotePlayerDataSet(data.equipped_weapon,data.equipped_shield);// izbiro toola rabmo nastimat po tem ko dobimo shield! pohandla tud izbiro weapona tko da nerabmo tega u combathandlerju!
       

        //---------------------COMBAT 2
        Debug.Log("Updating combat for " + networkObject.Owner.NetworkId);
        //tle se najde tud trenutni weapon
        NetworkPlayerCombatHandler c = GetComponent<NetworkPlayerCombatHandler>();
        c.Combat_mode=data.combat_State;
        c.Blocking=data.blocking;
        
        c.weapon_direction = data.direction;
        c.is_readying_attack = data.is_readying;
        c.ready_attack=data.ready_atk;
        c.executing_attack=data.exec_atk;
        c.OnRemotePlayerDataSet();
        //--------------------- ANIMATION 3
        //animacije bom ubistvu DIREKT preslikov trenutno stanje kot je. ce se da forsirat state machine bi blo awesome.  skripta ne hrani cist nic tko da me zanima izkljucno samo Animator
        Debug.Log("Updating animator for " + networkObject.Owner.NetworkId);
        Animator an = GetComponent<Animator>();

        //za animator pomoje bi blo treba kr u skriptah handlat stvari al kko....
        //tole bo se konkretna jeba...

        an.SetInteger("combat_mode",data.an_combat);
        an.SetInteger("attack_direction", data.an_dir);
        an.SetBool("shield_equipped", data.an_shield);
        an.SetBool("combat_blocking", data.an_blocking);
        an.SetBool("crouched", data.an_crouched);
        an.SetInteger("weapon_animation_class", data.an_weapon_anim_cl);


        //pohandlat je treba animacije zdj nekak da player k se je sconnectov gort nekak pravilno vid druge playerje ce so trenutno nasred animacijskega postopka
        //reset
        an.SetBool("synchronization_ready_attack", false);
        an.SetBool("synchronization_executing_attack", false);
        an.SetBool("synchronization_combat_blocking", false);
        an.SetBool("synchronization_downed", false);


        if (data.downed || data.dead)
        {
            if (data.downed)
                an.SetBool("synchronization_downed", true);
        }
        else 
        {
            if (c.is_readying_attack)
                GetComponent<NetworkPlayerAnimationLogic>().handle_readying_of_attack((byte)data.an_dir);
            else if (c.ready_attack)
                an.SetBool("synchronization_ready_attack", true);
            else if (c.executing_attack)
                an.SetBool("synchronization_executing_attack", true);
            else if (c.Blocking)
                an.SetBool("synchronization_combat_blocking", true);
        }
        


        //kar je kode za animator je treba pohandlat tukej zaenkrat

        //bool an_grounded = an.GetBool("grounded");

        Debug.Log("REMOTE PLAYER UPDATE COMPLETE FOR " + networkObject.Owner.NetworkId);
    }

    private void OnRemotePlayerDataSet()
    {
        updateDisplayName();
        //ce je downed nj bi animator handlov??
        if (this.dead)
            local_setDrawingPlayer(false);//i think this is enough
    }

    private void load_player_data_on_connected() {
        uint steamId = 666;
        Debug.Log("Checking if Player with steamid of " + steamId + " has any saved data.");
        if (!PlayerManager.load_player_from_saved_data(steamId, gameObject))
        {
            //nimamo podatkov, rect moramo playerju da nj nam poslje svoj UMADNA. tko je zaenkrat.
            
            
        //ZA KASNEJSO IMPLEMENTACIJO CE BO SERVER NADZIROV DNA PLAYERJEV! TRENUTNO CLIENT POSLJE DATA SERVERJU IN SERVER PROPAGIRA NAPREJ CE JE DATA OK
            
            //GetComponent<NetworkUMADnaHandler>().requestDNAFromPlayer();
        }
    }

    private void save_player_on_disconnect(NetworkingPlayer disconnecting_networkingPlayer, NetWorker disconnecting_networker) {
        //------------------------------NetworkPlayerMovement.cs ------------------------------------------------------------------------------------------------
        Debug.Log("Player Disconnected! Saving NetworkPlayerMovement.cs for player  netId: " + disconnecting_networkingPlayer.NetworkId);
        if (PlayerManager.Save(GetComponent<NetworkPlayerMovement>()))
        {
            Debug.Log("Successfully saved player movement.");
        }
        //------------------------------NetworkPlayerInteraction.cs ------------------------------------------------------------------------------------------------
        Debug.Log("Saving NetworkPlayerInteraction.cs for player with  netId: " + disconnecting_networkingPlayer.NetworkId);
        if (PlayerManager.Save(GetComponent<NetworkPlayerInteraction>()))
        {
            Debug.Log("Successfully saved player Interaction.");
        }
        //------------------------------NetworkPlayerInventory.cs ------------------------------------------------------------------------------------------------
        Debug.Log("Saving NetworkPlayerInventory.cs for player with  netId: " + disconnecting_networkingPlayer.NetworkId);
        if (PlayerManager.Save(GetComponent<NetworkPlayerInventory>()))
        {
            Debug.Log("Successfully saved player NetworkPlayerInventory.");
        }
        //------------------------------NetworkPlayerAnimationLogic.cs ------------------------------------------------------------------------------------------------
        Debug.Log("Saving NetworkPlayerAnimationLogic.cs for player with  netId: " + disconnecting_networkingPlayer.NetworkId);
        if (PlayerManager.Save(GetComponent<NetworkPlayerAnimationLogic>()))
        {
            Debug.Log("Successfully saved player NetworkPlayerAnimationLogic.");
        }
        //------------------------------NetworkPlayerNeutralStateHandler.cs ------------------------------------------------------------------------------------------------
        Debug.Log("Saving NetworkPlayerNeutralStateHandler.cs for player with  netId: " + disconnecting_networkingPlayer.NetworkId);
        if (PlayerManager.Save(GetComponent<NetworkPlayerNeutralStateHandler>()))
        {
            Debug.Log("Successfully saved player NetworkPlayerNeutralStateHandler.");
        }
        //------------------------------NetworkPlayerCombat.cs ------------------------------------------------------------------------------------------------
        Debug.Log("Saving NetworkPlayerCombatHandler.cs for player with  netId: " + disconnecting_networkingPlayer.NetworkId);
        if (PlayerManager.Save(GetComponent<NetworkPlayerCombatHandler>()))
        {
            Debug.Log("Successfully saved player NetworkPlayerCombatHandler.");
        }
        //------------------------------NetworkPlayerStats.cs ------------------------------------------------------------------------------------------------
        Debug.Log("Saving NetworkPlayerStats.cs for player with  netId: " + disconnecting_networkingPlayer.NetworkId);
        if (PlayerManager.Save(this))
        {
            Debug.Log("Successfully saved player NetworkPlayerStats.");
        }
        pre_disconnect_cleanup();

        //tle i guess unicmo playerja...

        networkObject.Destroy();
    }

    /// <summary>
    /// KO SE DATA NALOZI MORAMO NEKAK PAMETNO POSKRBET DA SE STVARI APPLAYAjo
    /// </summary>
    internal void OnPlayerDataLoaded()
    {
        Debug.Log("Applying Stats.");
        //UpdateTeam rpc     nevem kko bo pohandlat team zaenkrat.
        if (!networkObject.IsServer) return;
        updateDisplayName();
        //health
        if (this.dead)        
            handle_death_player();
        else
            networkObject.SendRpc(RPC_SET_HEALTH,Receivers.All, this.health,"null");
    }



    private void pre_disconnect_cleanup()
    {
        if (networkObject.IsServer && this.death_timer_coroutine != null)//just in case
        {
            StopCoroutine(this.death_timer_coroutine);
            this.death_timer_coroutine = null;
        }


    }

    internal void handle_collision_with_siege_projectile()
    {
        handle_death_player();
    }



    public void Update()
    {
        if (this.update_network_throttling)
        {
            this.update_network_throttling = false;
            NetworkManager.Instance.Networker.LatencySimulation = this.simulated_ms_delay;
            NetworkManager.Instance.Networker.PacketLossSimulation = this.simulated_packet_loss;
        }

        if (test) {
            test = false;
            networkObject.SendRpc(RPC_SET_HEALTH, Receivers.All, 0f, "coll_0");
        }

        if (networkObject == null)
        {
            // Debug.LogError("networkObject is null.");
            return;
        }


        #region sending ping
        if (networkObject.IsOwner){
            this.ping_timer_helper += Time.deltaTime;
            if (this.ping_timer_helper > 1f)
            {
                this.ping_timer_helper = 0;
                NetworkManager.Instance.Networker.Ping();
                UILogic.Instance.setBandwidthOut(NetworkManager.Instance.Networker.BandwidthOut);
                UILogic.Instance.setBandwidthIn(NetworkManager.Instance.Networker.BandwidthIn);
                UILogic.Instance.setTimeStamp(NetworkManager.Instance.Networker.Time.Timestep);
                
            }
        }
        #endregion

        if (networkObject.IsServer && Input.GetKeyDown(KeyCode.X))
        {
            // Debug.Log("Spawning uma");
            spawn_UMA_body(transform.position, get_UMA_to_string(), 0);

        }

        if (Input.GetButtonDown("Interact") && networkObject.IsOwner)
        {
            Debug.Log(this.dead);
        }


        /*  if (this.test) {
              this.health = 0;
              handle_0_hp();
          }*/
       
    }



    public void OnSubmitModifiedGuildDataClick() {
        NetworkGuildManager.Instance.OnModifyGuildConfirmClick();
    }


    /*
HOW DAMAGE WORKS RIGHT NOW:
na serverju se detektira hit. trenutno edina skripta ki to dela je Weapon_Collider_handler, ki poklice tole metodo. ta metoda izracuna nov health od tega k je bil napaden. to vrednost poslje
napadenmu playerju da si poupdejta health. ta player pol ko si je updejtov health poslje nov rpc vsem drugim da nj si nastavijo njegov health na njegov health. tud server(i can see how this is bad ampak za prototip me ne skrbi.)
*/

    public uint[] getTeam() {
        return this.team;
    }

    #region TAKING DAMAGE

    internal bool is_valid_server_block(NetworkPlayerStats attacker)
    {
        if (!networkObject.IsServer) return false;
        bool blocked = false;
        //this je defender
        Animator anim = GetComponent<Animator>();
        bool is_blocking_with_shield = anim.GetBool("shield_equipped");
        if (is_blocking_with_shield)
        {
            blocked = true;

        }
        else
        {
            float other_direction = attacker.gameObject.GetComponent<Animator>().GetFloat("attack_direction");
            float this_direction = anim.GetFloat("attack_direction");
            blocked = this_direction == other_direction;
        }

        if (blocked)
        {
            //ubistvu samo posljemo na ta netowrkobject in vsi nrdijo sound effect tle
            networkObject.SendRpc(RPC_ON_BLOCKED, Receivers.AllProximity);
        }
        return blocked;
    }





    /// <summary>
    /// this je passive player
    /// </summary>
    /// <param name="weapon"></param>
    /// <param name="tag_passive"></param>
    /// <param name="passive_player_server_network_id"></param>
    /// <param name="agressor_server_network_id"></param>
    public void take_weapon_damage_server_authority(Item weapon,string tag_passive, uint agressor_server_network_id)
    {
        
        //tag je za tag colliderja. coll_0 = headshot, coll_1 = body/torso, coll2=arms/legs
        //networkObject.SendRpc(RPC_UPDATE_ALL_PLAYER_ID, Receivers.Server);
        if (networkObject.IsServer)
        {
            if (this.dead) return;
            if (npi == null) npi = GetComponent<NetworkPlayerInventory>();
            float prev_hp = this.health;
            float dmg;
            if (weapon == null)
            {//unarmed combat. posebej napisat vrednosti. enakob o za block
                dmg = 5;
            }
            else {//poberemo vrednosti iz weapona
                dmg = weapon.damage;
            }
            
            //-----------------------------------------DAMAGE MODIFIERS----------------------------------------------------
            float current_block_damage_reduction = 1.0f;
            if (tag_passive.Equals("block_player"))
            {
                current_block_damage_reduction = block_damage_reduction;

                //tukaj manjka koda za shield. treba poiskat playyerja pa ugotovit kter shield ima v roki za block
            }


            float locational_damage_reduction = 0;
            if (tag_passive.Equals("coll_0")) locational_damage_reduction = head_damage_multiplier;
            else if (tag_passive.Equals("coll_1")) locational_damage_reduction = torso_damage_multiplier;
            else if(tag_passive.Equals("coll_2")) locational_damage_reduction = limb_damage_multiplier;

            //-----------armor values
            float armor_damage_reduction_modifier = 0;
            Predmet temp;
            if (tag_passive.Equals("coll_0"))
            {
                temp = npi.getHeadItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.getItem().damage_reduction;

            }
            else if (tag_passive.Equals("coll_1"))
            {
                temp = npi.getChestItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.getItem().damage_reduction;

                temp = npi.getHandsItem();
                if (temp != null)
                    armor_damage_reduction_modifier += temp.getItem().damage_reduction;

            }
            else if (tag_passive.Equals("coll_2")) {
                temp = npi.getLegsItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.getItem().damage_reduction;

                temp = npi.getFeetItem();
                if (temp != null)
                    armor_damage_reduction_modifier += temp.getItem().damage_reduction;
            }

            armor_damage_reduction_modifier = 1 - armor_damage_reduction_modifier;
            float all_modifiers = locational_damage_reduction * current_block_damage_reduction * armor_damage_reduction_modifier;
            //-------------------------------------------------------------------------------------------------------------
            float final_damage_taken = dmg * all_modifiers;
            float new_hp= this.health - final_damage_taken;// - health bomo poslal po rpcju - tud host bo sam seb poslov tko da nima veze ce kdo spreminja network.
            if (new_hp < 0) new_hp = 0;
            //healthBar.fillAmount = (float)this.health / (float)this.max_health;

            lock (NetworkManager.Instance.Networker.Players)
            {
                int count = 0;//v koliziji sta udelezena dva igralca, poiskat moramo oba. tukej je lahko problem ce klicemo to metodo pri koliziji z ne-igralcem, za agresorja bo slo vedno cez vse igralce.
                NetworkManager.Instance.Networker.IteratePlayers((player) =>
                {
                    if (player.NetworkId == networkObject.Owner.NetworkId) //passive target
                    {
                        //Debug.Log("Victim found! "+ passive_player_server_network_id);
                        networkObject.SendRpc(RPC_SET_HEALTH,Receivers.All, new_hp, tag_passive);
                        count++;

                        if (prev_hp == 0 && final_damage_taken > 0) {
                            //death
                            //poseben sound effect za tak tip smrti?
                            SFXManager.OnPlayerDeath(transform,true);
                            handle_death_player();
                        }
                    }

                    //agressor za izrisanje damage-a
                    if (player.NetworkId == agressor_server_network_id)
                    {
                        //Debug.Log("Agressor player found! " + agressor_server_network_id);
                        networkObject.SendRpc(player, RPC_RECEIVE_NOTIFICATION_FOR_DAMAGE_DEALT, final_damage_taken, tag_passive);
                        count++;
                    }
                    if (count == 2) return;

                });

            }
        }
    }

    internal void SetGuildUpdated(string tag)
    {
       this.player_displayed_name.text ="["+tag + "] " + this.playerName;
    }

    public void take_environmental_damage_server_authority(Item item, string tag)
    {

        //tag je za tag colliderja. coll_0 = headshot, coll_1 = body/torso, coll2=arms/legs
        if (networkObject.IsServer)
        {
            if (this.dead) return;
            if (npi == null) npi = GetComponent<NetworkPlayerInventory>();
            float prev_hp = this.health;
            float dmg = item.damage;


            //-----------------------------------------DAMAGE MODIFIERS----------------------------------------------------
            Debug.Log(tag);
            float locational_damage_reduction = 0;
            if (tag.Equals("coll_0")) locational_damage_reduction = head_damage_multiplier;
            else if (tag.Equals("coll_1")) locational_damage_reduction = torso_damage_multiplier;
            else if (tag.Equals("coll_2")) locational_damage_reduction = limb_damage_multiplier;

            //-----------armor values
            float armor_damage_reduction_modifier = 0;
            Predmet temp;
            if (tag.Equals("coll_0"))
            {
                temp = npi.getHeadItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.getItem().damage_reduction;

            }
            else if (tag.Equals("coll_1"))
            {
                temp = npi.getChestItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.getItem().damage_reduction;

                temp = npi.getHandsItem();
                if (temp != null)
                    armor_damage_reduction_modifier += temp.getItem().damage_reduction;

            }
            else if (tag.Equals("coll_2"))
            {
                temp = npi.getLegsItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.getItem().damage_reduction;

                temp = npi.getFeetItem();
                if (temp != null)
                    armor_damage_reduction_modifier += temp.getItem().damage_reduction;
            }

            armor_damage_reduction_modifier = 1 - armor_damage_reduction_modifier;
            float all_modifiers = locational_damage_reduction * armor_damage_reduction_modifier;
            //-------------------------------------------------------------------------------------------------------------
            float final_damage_taken = dmg * all_modifiers;
            float new_hp = this.health - final_damage_taken;// - health bomo poslal po rpcju - tud host bo sam seb poslov tko da nima veze ce kdo spreminja network.
            if (new_hp < 0) new_hp = 0;
            //healthBar.fillAmount = (float)this.health / (float)this.max_health;

            lock (NetworkManager.Instance.Networker.Players)
            {
                NetworkManager.Instance.Networker.IteratePlayers((player) =>
                {
                    if (player.NetworkId == networkObject.Owner.NetworkId) //passive target
                    {
                        //Debug.Log("Victim found! "+ passive_player_server_network_id);
                        networkObject.SendRpc(RPC_SET_HEALTH, Receivers.All, new_hp, tag);
                        if (prev_hp == 0 && final_damage_taken > 0)
                        {
                            //death
                            handle_death_player();
                        }
                    }
                });

            }
        }
    }

    internal bool localDisconnectRequest()
    {
        if (networkObject.IsOwner && !NetworkManager.Instance.Networker.Me.IsHost)
        {
            NetworkManager.Instance.Networker.Disconnect(false);
            return true;
        }
        else if (NetworkManager.Instance.Networker.Me.IsHost) {
            Debug.LogWarning("Host tried disconnecting..");
            NetworkManager.Instance.Disconnect();
            return true;
        }
        else
        {
            Debug.LogError("tried disconnecting from a networkplayer that isnt owned by me.");
            return false;
            
        }
    }

    #endregion

    internal uint Get_server_id()
    {
        return networkObject.Owner.NetworkId;
    }

    internal bool am_i_local_client()
    {
        return networkObject.IsOwner && !networkObject.IsServer;
    }


    private void handle_death_player()//samo na serverju
    {
        if (!networkObject.IsServer) return;

        // Debug.Log("Spawning body");
        if(GetComponent<NetworkPlayerInventory>().backpackSpot.GetComponentInChildren<NetworkBackpack>()!=null)
            GetComponent<NetworkPlayerInventory>().backpackSpot.GetComponentInChildren<NetworkBackpack>().local_server_BackpackUnequip();

        spawn_UMA_body(transform.position, get_UMA_to_string(), networkObject.Owner.NetworkId);//poslje rpc da nrdi uma body in disabla renderer za playerja v enem
                                                                           // server mora vsem sporocit da nj nehajo renderat playerja k je lihkar umru ker ga je vizualno zamenjov ragdoll
        networkObject.SendRpc(RPC_ON_PLAYER_DEATH, Receivers.All);
    }

    internal bool server_side_respawn_request(RpcArgs args, Vector3 bed_position)
    {
        if (!networkObject.IsServer) return false;
        if (networkObject.Owner.NetworkId != args.Info.SendingPlayer.NetworkId) return false;
        if (check_for_validity_of_respawn_request())//delno se preveri ze v NetworkPlayerBed
            networkObject.SendRpc(RPC_RESPAWN_SIGNAL, Receivers.All, bed_position+Vector3.up);
        else return false;
        return true;
    }

    private bool check_for_validity_of_respawn_request()
    {
        //tle bo kao za prevert ce se player sme respawnat al je slucajn kej pohackov. timer nj bi se chekirov v NetworkPlayerBed.cs
        if (!this.dead)
            return false;

        return true;
    }


    private void updateDisplayName()
    {
        Debug.Log("updating display name ="+ Get_server_id() + " - " + this.playerName + " [ " + this.tag_guild + " ]");

        this.player_displayed_name.text = Get_server_id() + " - " + this.playerName + " [ " + this.tag_guild + " ]";
    }


    private void spawn_UMA_body(Vector3 pos, string data, uint player_id)//nevem kam nj bi drgac dau. lh bi naredu svojo skripto ampak je tud to mal retardiran ker rabs pol networking zrihtat...
    {
        if (!networkObject.IsServer) return;
        Network_bodyBehavior b = NetworkManager.Instance.InstantiateNetwork_body(0, pos);
        b.gameObject.GetComponent<Network_body>().set_data_for_init(data, player_id);
    }

    private string get_UMA_to_string()
    {
       // Debug.Log("not implemented yet");
        return "empty so far";
    }

    public void set_player_health(float amount,uint id) {
        if (!networkObject.IsServer) return;
       // Debug.Log("server :set player health");
        lock (NetworkManager.Instance.Networker.Players)
        {
            NetworkManager.Instance.Networker.IteratePlayers((player) =>
            {
                if (player.NetworkId == id) //passive target
                {
                   // Debug.Log("server :set player health - player found");
                    //networkObject.SendRpc(player, RPC_SET_HEALTH_PASSIVE_TARGET, amount, "revive");
                    networkObject.SendRpc(RPC_SET_HEALTH, Receivers.All, amount, "revive");
                    return;
                }
            });

        }
        //Debug.Log("server :set player health - player not found");
        //networkObject.SendRpc(player, RPC_SET_HEALTH_PASSIVE_TARGET, this.health, tag_passive);
    }


    private void handle_0_hp() {//sprozi tko na ownerju, kot na clientih
        this.downed = true;
        GetComponent<NetworkPlayerAnimationLogic>().handle_downed_start();
        if(GetComponent<NetworkPlayerInventory>().backpackSpot.GetComponentInChildren<NetworkBackpack>())
            GetComponent<NetworkPlayerInventory>().backpackSpot.GetComponentInChildren<NetworkBackpack>().local_server_BackpackUnequip();

        //da ne lebdi v zraku rabmo popravt collider
        CharacterController cc = GetComponent<CharacterController>();
        this.usual_character_controller_height = cc.height;
        this.usual_character_controller_center = cc.center;
        cc.height = 0;
        cc.center = new Vector3(this.usual_character_controller_center.x, this.usual_character_controller_center.y * 1.2f, this.usual_character_controller_center.z);

        if (networkObject.IsServer)
        {
            combatStateHandler.set_shield_collider(false);
            if (this.death_timer_coroutine != null) Debug.LogError("Death Coroutine is busy!! this shouldnt be happening");
            this.death_timer_coroutine = downed_timer(this.death_timer);
            StartCoroutine(this.death_timer_coroutine);
        }
    }

    private IEnumerator downed_timer(float t) {
        yield return new WaitForSecondsRealtime(t);
        if (downed)
        {
            Debug.Log("Player"+networkObject.Owner.NetworkId+" has been on the ground for " + t + " seconds. He will die now.");
            handle_death_player();
        }
    }

    public void handle_player_pickup() {
        this.downed = false;
        GetComponent<NetworkPlayerAnimationLogic>().handle_player_revived();// z tlele k smo smo dobil lahko samo pobiranje igralca. execution bomo klical z drugje in takrat damo na false
        CharacterController cc = GetComponent<CharacterController>();
        cc.height = this.usual_character_controller_height;
        cc.center = this.usual_character_controller_center;
        if (networkObject.IsServer && this.death_timer_coroutine != null)
        {
            StopCoroutine(this.death_timer_coroutine);
            this.death_timer_coroutine = null;
        }
        if (networkObject.IsServer) {
            combatStateHandler.set_shield_collider(true);
        }
    }




    /// <summary>
    /// server poklice da se nastav health na skripti. klice se na vsah skriptah, tud an serverju
    /// </summary>
    /// <param name="args"></param>
    public override void setHealth(RpcArgs args)
    {
        if (!args.Info.SendingPlayer.IsHost) 
            return;
        

        float hp = args.GetNext<float>();
        float prev_hp = this.health;
        bool downed = false;
        if (this.health > 0 && hp == 0)
            downed = true;
        this.health = hp;



        if (downed) handle_0_hp();



        string tag = args.GetNext<string>();
        if (tag.Equals("coll_0") || tag.Equals("coll_1") || tag.Equals("coll_2"))
            SFXManager.OnPlayerHit(transform);
        this.healthBar.fillAmount = this.health / (this.max_health);
        UILogic.TeamPanel.refreshHp(networkObject.NetworkId, this.healthBar.fillAmount);
        if(networkObject.IsOwner)Exploration_and_Battle.Instance.onHostileAction();
        
        //}
    }

    public override void ReceiveNotificationForDamageDealt(RpcArgs args)//tole funkcijo dobi owner agresor objekta in izrise na ekran da je naredu damage, rpc poslje server v metodi take_damage_server_authority
    {


        float dmg = args.GetNext<float>();
        string tag = args.GetNext<string>();
       // Debug.Log("I Did Damage! "+tag);
        FloatingTextController.CreateFloatingText(dmg+"", Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane)),tag);
        reticle_hit_controller.CreateReticleHit(tag); //cod2 reticle hit style like
        Exploration_and_Battle.Instance.onHostileAction();

        GetComponent<NetworkPlayerAnimationLogic>().on_weapon_or_tool_collision(false);//tole ne dela in nevem zakaj ne... ?
    }


    public override void respawnSignal(RpcArgs args)//poslje server vsem, tud sebi
    {
        if (!args.Info.SendingPlayer.IsHost) {
            Debug.LogError("Player, that is not host tried sending respawn signal!!");
            return;
        }
        //nastav spremenljivke povsod

        if (this.usual_character_controller_height == 0) {
            //nastav na neke base vrednosti..
            this.usual_character_controller_height = 1.7f;//vzeto iz prefaba
            this.usual_character_controller_center = new Vector3(0.0038f, 0.84f, -0.065f);//vzeto iz prefaba
        }

        CharacterController cc = GetComponent<CharacterController>();
        cc.height = this.usual_character_controller_height;
        cc.center = this.usual_character_controller_center;

        this.downed = false;
        this.dead = false;

        transform.position = args.GetNext<Vector3>();
        local_setDrawingPlayer(true);
        if(networkObject.IsOwner)UILogic.Instance.closeDeathScreen();

        if (networkObject.IsServer)//server nastima vsem health
            set_player_health(max_health, Get_server_id());

    }

    /// <summary>
    /// metoda k jo dobijo vsi
    /// </summary>
    /// <param name="args"></param>
    public override void OnPlayerDeath(RpcArgs args)//vsi dobijo
    {
        if (args.Info.SendingPlayer.IsHost)
        {
            local_setDrawingPlayer(false);//v drugi metodi zato ker se klice se z vsaj ene druge metode

            //ce "umre" mormo tud resetirat za animacijo da ne lezi vec na tleh. to se izvede na vseh clientih in serverju
            
            this.downed = false;
            this.dead = true; //to bi moral lockat combat pa tak
            this.health = 0;
            Debug.Log("player died! - downed: " + this.downed + " dead: " + this.dead);
            GetComponent<NetworkPlayerCombatHandler>().handle_player_death();//disabla shield pa weapon
            GetComponent<NetworkPlayerAnimationLogic>().handle_player_death();
            if(networkObject.IsOwner)UILogic.Instance.showDeathScreen();

            if (networkObject.IsServer && this.death_timer_coroutine!=null)
            {
                StopCoroutine(this.death_timer_coroutine);
                this.death_timer_coroutine = null;
            }
        }
    }

    private void local_setDrawingPlayer(bool b) {
        transform.Find("UMARenderer").gameObject.SetActive(b);
    }

    /// <summary>
    /// poklice server da poslje nmovo stanje vsem team memberjim. tud sam sebi ce  je v teamu. new_team je lahko null
    /// </summary>
    /// <param name="args"></param>
    public override void UpdateTeam(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;

        String updatedMemebers = args.GetNext<string>();
        this.team = stringToTeam(updatedMemebers);
        GetComponentInChildren<local_team_panel_handler>().refreshAll(this.team);
    }


    private uint[] stringToTeam(string s) {
        if (s == null) return null;

        string[] ss = s.Split('|');
        uint[] rez = new uint[ss.Length - 1];//zacne se z "" zato en slot sfali
        for (int i = 1; i < ss.Length; i++)
        {//zacne z 1 ker je ss[0] = ""
            int k = -1;
            Int32.TryParse(ss[i], out k);
            rez[i - 1] = (uint)k;
        }
        return rez;
    }

    private String teamToString(uint[] t) {
        if (t == null) return "";

        string s = "";
        for (int i = 0; i < t.Length; i++)
        {
                s = s + "|" + t[i];

        }
        //Debug.Log(s);
        return s;
    }

    internal void localTeamInviteRequest(uint other) {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_TEAM_INVITE_REQUEST, Receivers.Server, other);
       
    }

    private void serverSide_sendNegativeTeamResponse(uint other) {
        lock (NetworkManager.Instance.Networker.Players)
        {

            NetworkManager.Instance.Networker.IteratePlayers((player) =>
            {
                if (player.NetworkId == other) //passive target
                {
                    serverSide_sendNegativeTeamResponse(player);
                    return;
                }
            });

        }
    }
    private void serverSide_sendNegativeTeamResponse(NetworkingPlayer p)
    {
        if (networkObject.IsServer)
            FindByid(p.NetworkId).GetComponent<NetworkPlayerStats>().negative_team_dejanski_rpc(p);
    }

    private void negative_team_dejanski_rpc(NetworkingPlayer p)
    {
        if (networkObject.IsServer)
            networkObject.SendRpc(p, RPC_TEAM_INVITE_NEGATIVE_RESPONSE);
    }
    public override void teamInviteNegativeResponse(RpcArgs args)
    {
        if (networkObject.IsOwner && args.Info.SendingPlayer.NetworkId == 0) {
            //nekak uporabniku prkazat da je failal
            Debug.LogError("tole bo treba dopolnit pri implementaciji chata. v chat naj izpise da je request za team failov.");
        }
    }

    /// <summary>
    /// dobi server, poslje player k hoce invitat druzga playerja. id druzga playerja je u args
    /// </summary>
    /// <param name="args"></param>
    public override void teamInviteRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId==networkObject.Owner.NetworkId) {
            uint other = args.GetNext<uint>();

            NetworkPlayerStats other_player_stats = FindByid(other).GetComponent<NetworkPlayerStats>();
            //ce je clovk ze u teamu skipej?????
            bool fail = false;
            if (other_player_stats.team_invite_pending == false)
            {
                if (other_player_stats.team != null)
                {
                    if (other_player_stats.team.Length > 0)
                    {
                        Debug.Log("PLAYER team response negative");

                        serverSide_send_full_team_notification(other);

                        fail = true;
                    }
                }
            }
            else {
                fail = true;
            }
            if (fail) serverSide_sendNegativeTeamResponse(args.Info.SendingPlayer);


            if (!isMyTeamMember(other)) {
                //request invite to team

                lock (NetworkManager.Instance.Networker.Players)
                {

                    NetworkManager.Instance.Networker.IteratePlayers((player) =>
                    {
                        if (player.NetworkId == other) //passive target
                        {
                            other_player_stats.team_invite_pending = true;
                            StartCoroutine(teamInviteLock(other_player_stats));//mislen je da lahko dobi samo en team request naenkrat. ce jih dobi vec nevem kaj se nrdi honestly.
                            other_player_stats.serverSide_TeamInviteRequestToOther(player, args.Info.SendingPlayer.NetworkId);
                            return;
                        }
                    });

                }



            }
        }
        
    }

    private void serverSide_send_full_team_notification(uint other)
    {
        if (networkObject.IsServer)
            FindByid(other).GetComponent<NetworkPlayerStats>().send_full_team_notification_to_owner(Get_server_id());

    }

    private void send_full_team_notification_to_owner(uint other)
    {
        if (networkObject.IsServer)
            networkObject.SendRpc(RPC_TEAM_INVITE_ALREADY_IN_PARTY_NOTIFICATION, Receivers.Owner, other);
    }

    public override void teamInviteAlreadyInPartyNotification(RpcArgs args)
    {
        if (networkObject.IsOwner && args.Info.SendingPlayer.NetworkId == 0)
            Debug.LogError("player tried inviting you to team, but you are already in a party.");
    }


    IEnumerator teamInviteLock(NetworkPlayerStats s)
    {
        
            yield return new WaitForSecondsRealtime(60);
            s.team_invite_pending = false;
    }


    /// <summary>
    /// metoda nrjena zato ker ce hocmo klicat playerja X in ce hocmo da se oglas owner, mormo klicat z skripte, katere owner je player X
    /// </summary>
    void serverSide_TeamInviteRequestToOther(NetworkingPlayer player, uint sender) {
        if(networkObject.IsServer)
            networkObject.SendRpc(player, RPC_TEAM_INVITE_REQUEST_TO_OTHER, sender); //mormo poslat id zravn ker ne posilja ta player ampak server
    }

    private bool isMyTeamMember(uint other) {
        if (this.team == null) return false;
        if (this.team.Length == 1) return false;
        foreach (uint i in this.team)
            if (i == other) return true;

        return false;
    }

    /// <summary>
    /// poslje server ownerju tega k ga hocmo invitat
    /// </summary>
    /// <param name="args"></param>
    public override void teamInviteRequestToOther(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0 && networkObject.IsOwner) {
            uint other = args.GetNext<uint>();
            GameObject other_gameobject = FindByid(other);

            if (this.team !=null)
                if (this.team.Length > 0) return;//smo ze u teamu


            ///izris eno panelo kjer te vprasa ce se hocs joinat. podatke dobimo iz other_gameobject
            UILogic.DecisionsHandler.draw_team_invite_decision(other_gameobject);

            //response posljemo kot RPC_TEAM_INVITE_OTHER_RESPONSE,otherplayerNetworkId,bool -> LocalTeamRequestResponse(uint id, bool resp) {
        }
    }


    /// <summary>
    /// metoda se klice, ko player klikne accept/decline na paneli, ki jo dobi k  ga en invita u team. posle odgovor serverju kaj si je zmislu
    /// </summary>
    /// <param name="id"></param>
    /// <param name="resp"></param>
    internal void LocalTeamRequestResponse(uint id, bool resp)
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_TEAM_INVITE_OTHER_RESPONSE, Receivers.Server, id, resp);
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log("interactable.findplayerById");
        //Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
        //    Debug.Log(p.GetComponent<NetworkPlayerStats>().server_id);
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }

    /// <summary>
    /// poslje un player k je invitan odgovor serverju. server pohendla stvar zdj ko ve vse kaksna je stvar. na koncu poslje novo stanje vsem
    /// </summary>
    /// <param name="args"></param>
    public override void teamInviteOtherResponse(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            uint other = args.GetNext<uint>();//originalni player
            bool decision = args.GetNext<bool>();

            this.team_invite_pending = false;

            if (!check_legitimacy_of_response(other, args.Info.SendingPlayer.NetworkId)) { serverSide_sendNegativeTeamResponse(other); }

            if (decision)
            {
                //sestavi nov team za ta objekt. poglej za playerja k je u originalu poslov invitew, in dodaj tega playerja v njegov team. posl update vsem, tud serverju.
                uint[] new_team = FindByid(other).GetComponent<NetworkPlayerStats>().team;//server mora zmer hrant to vrednost

                if (new_team == null || new_team.Length < 2)
                {
                    new_team = new uint[2];
                    new_team[0] = other;
                    new_team[1] = args.Info.SendingPlayer.NetworkId;//mora bit isto kot this.server_id ce sm sigurn..
                }
                else
                {
                    uint[] temp_team = new uint[new_team.Length + 1];
                    for (int i = 0; i < new_team.Length; i++)
                        temp_team[i] = new_team[i];
                    temp_team[new_team.Length] = args.Info.SendingPlayer.NetworkId;
                    new_team = temp_team;
                }

                //posl networkupdate vsem k so u teamu + server
                serverSide_send_team_update(new_team, new_team, false, other);//tle sta parametra enaka, pri leavanju skupine sta razlicna
               

            }
            else {//player declined or timed out.
                serverSide_sendNegativeTeamResponse(other);
            }
        }
    }
    /// <summary>
    /// poslje trenutno stanje this.team na ownerjevo instanco
    /// </summary>
    /// <param name="player"></param>
    /// <param name="s"></param>
    private void serverSide_updateTeamhelper() {
        if(networkObject.IsServer)
            networkObject.SendRpc(RPC_UPDATE_TEAM,Receivers.Owner, teamToString(this.team));
    }


    //preveri ali je rpc response, ki je prsu od networkId legitimen, tj- ali odgovarja na nek pending request, ali se hoce ilegalno utaknit v nek team.
    private bool check_legitimacy_of_response(uint other, uint networkId)
    {
        Debug.LogError("NOT IMPLEMENTED!");
        return true;
    }

    /// <summary>
    /// ne primerja prot this.team, ampak server gleda komu poslat rpc za update teama
    /// </summary>
    /// <param name="new_team"></param>
    /// <param name="networkId"></param>
    /// <returns></returns>
    private bool isTeamMemberForNetworkUpdate(uint[] new_team, uint networkId)
    {
        foreach (uint i in new_team)
            if (i == networkId)
                return true;
        return false;
    }


    //-----------------------------LEAVE BUTTON-------------
    internal void local_tryToLeaveTeam()
    {
        if (networkObject.IsOwner) {
            networkObject.SendRpc(RPC_TEAM_LEAVE_REQUEST, Receivers.Server);
        }
    }

    public override void teamLeaveRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            if (this.team != null)
            {
                uint[] new_team;
                if (this.team.Length < 3) new_team = null;
                else//vsaj 3je elementi so not
                    new_team = remove_from_uint(this.team, args.Info.SendingPlayer.NetworkId);
                serverSide_send_team_update(new_team, this.team, true, args.Info.SendingPlayer.NetworkId);
            }
            else {
                //poglej da je res vrzen iz vsah vn in posl sinhronizacijo u network ce si kje naredu spremembo. to se nebi smel nikol izvest sicer..
                uint[] x = new uint[1];
                x[0] = args.Info.SendingPlayer.NetworkId;
                serverSide_send_team_update(null, x, true, args.Info.SendingPlayer.NetworkId);//posl senderju zahtevo da nj neha bit retard pa nj poupdejta
            }
        }
    }

    private uint[] remove_from_uint(uint[] team, uint id)
    {
        uint[] n = new uint[team.Length - 1];
        int c= 0;
        foreach (uint u in team)
            if (u != id)
                n[c++] = u;
        return n;
    }

    private void serverSide_send_team_update(uint[] new_team, uint[] players_to_update, bool deleting, uint requester) {
        int count = 0;
        lock (NetworkManager.Instance.Networker.Players)
        {

            NetworkManager.Instance.Networker.IteratePlayers((player) =>
            {
                if (isTeamMemberForNetworkUpdate(players_to_update, player.NetworkId)) //team memberji. vsakemu memberju nastav kdo so njegovi team memberji, tud na serverjevi skripti
                {
                    NetworkPlayerStats nps = FindByid(player.NetworkId).GetComponent<NetworkPlayerStats>();


                    if (!deleting)
                    {
                        nps.team = new_team;//da zrihta na serverju
                        nps.serverSide_updateTeamhelper();//da zrihta na clientu
                    }
                    else {
                        if (player.NetworkId != requester)
                        {
                            nps.team = new_team;//da zrihta na serverju
                            nps.serverSide_updateTeamhelper();//da zrihta na clientu

                        }
                        else {//mora poslat null sicer se zmer vidi team kar je ostal od njega
                            nps.team = null;//da zrihta na serverju
                            nps.serverSide_updateTeamhelper();//da zrihta na clientu
                        }
                    }
                    count++;
                    if (count >= players_to_update.Length) return;//ce je server v teamu bo zal iteriral cez vse playerje, sj jih nebo dost so i dont give a fuck
                }
            });

        }
    }

    public override void GuildUpdate(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            this.name_guild = args.GetNext<string>();
            this.tag_guild = args.GetNext<string>();
            this.color_guild = args.GetNext<Color>();
            this.image_guild = args.GetNext<byte[]>();

            // vsi nastavjo
            updateDisplayName();
        }
    }

    public override void RefreshHealth(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
            this.health = args.GetNext<float>();
    }



    private void OnLocalClientDisconnected(NetWorker sender)
    {
        Debug.LogError("LOCAL CLIENT HAS BEEN DISCONENCTED!!");

        MainThreadManager.Run(() => { UILogic.Instance.show_disconnect_info(); }) ;
    }
    public void kill() {
        if (networkObject.IsServer) {
            networkObject.Destroy();
        }
    }


    internal void localPlayerExecutionRequest(uint server_id_agresorja)
    {
        networkObject.SendRpc(RPC_EXECUTION_REQUEST, Receivers.Server, server_id_agresorja, 1f);//1f je cas animacije. naceloma bi mogl dat animator.getcurrentAnimation.getLengthOfCurrentAnimation al pa nekej nevem kaj je..
    }

    public override void ExecutionRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;
        //nek security check i guess
        GameObject passive = gameObject;
        GameObject agressor = FindByid(args.GetNext<uint>());
        float time_delay = args.GetNext<float>();

        if (Vector3.Distance(passive.transform.position, agressor.transform.position) < 2f && this.downed) {

            StartCoroutine(ExecutionDelayed(time_delay));
        }
    }
    private IEnumerator ExecutionDelayed(float t) {
        yield return new WaitForSeconds(t);
        handle_death_player();
    }

    public override void OnBlocked(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            SFXManager.OnBlock(transform,GetComponent<Animator>().GetBool("shield_equipped"));
        }
    }

    public void local_respawn_without_bed_request() {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_RESPAWN_WITHOUT_BED_REQUEST, Receivers.Server);

    }
    public override void RespawnWithoutBedRequest(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId && networkObject.IsServer) {
            Location l = Locations.GetNearestRespawnLocation(transform.position);
            if (l!=null)
                networkObject.SendRpc(RPC_RESPAWN_SIGNAL, Receivers.All, l.transform.position);
            else
                networkObject.SendRpc(RPC_RESPAWN_SIGNAL, Receivers.All, new Vector3 (15419, 413, 15784));
        }
            
    }

    public override void ServerSendAllThisToNewPlayer(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            Debug.Log("Received data from server to setup other clients and server player objects");
            byte[] b_arr = args.GetNext<byte[]>();
            PlayerSynchronizationContainer dat = b_arr.ByteArrayToObject<PlayerSynchronizationContainer>();
            update_non_owner_gameObject_with_data_on_startup(dat);
        }
    }
}
