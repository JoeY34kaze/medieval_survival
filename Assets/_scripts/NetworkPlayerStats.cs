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

    //private uint server_id = 5;
    public NetWorker myNetWorker;
    public Text player_displayed_name;
    public Text inventory_guild_name;

    public float head_damage_multiplier = 1.5f;
    public float torso_damage_multiplier = 1.0f;
    public float limb_damage_multiplier = 0.75f;

    public float block_damage_reduction = 0.025f;  //to bomo pobral z itema

    public float fire1_cooldown = 0.7f;

    private NetworkPlayerInventory npi;

    public GameObject[] sound_effects_on_player;

    public decicions_handler_ui decision_handler;
    public local_team_panel_handler team_panel;

    private uint[] team; //array networkId-jev team memberjev. server vedno hrani to vrednost za vse playerje. drugi dobijo samo update od serverja


    private List<uint> already_processed_inviters;
    private bool team_invite_pending = false;

    public Transform soul;

    public string name_guild="no guild yet";
    public string tag_guild="no tag yet";

    internal panel_guild_handler GetPGH()
    {
        return this.panelGuildMemberHandler;
    }

    public Color color_guild=Color.red;
    public byte[] image_guild;

    public GameObject guild_modification_panel;

    public Text guild_name_input;
    public Text guild_tag_input;
    public Text guild_color_input;

    private Queue<NetworkingPlayer> acceptedAndNotUpdatedPlayers;
    private bool AcceptedPlayerHandlingPending = false;
    public GameObject serverSide_guildManager;
    public panel_guild_handler panelGuildMemberHandler;

    private void Start()
    {
        this.npi = GetComponent<NetworkPlayerInventory>();
        acceptedAndNotUpdatedPlayers = new Queue<NetworkingPlayer>();
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        // TODO:  Your initialization code that relies on network setup for this object goes here
        myNetWorker = NetworkManager.Instance.Networker;//GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
        //networkObject.SendRpc(RPC_UPDATE_ALL_PLAYER_ID, Receivers.Server);
        //this.server_id = myNetWorker.Me.NetworkId; -- SAMO ZA DEBUGGING CE BO TREBA POGLEDAT V INSPEKTORJU ID AL PA KEJ
        //this.playerName = "Janez Kranjski";
        //updateDisplayName();
        if (networkObject.IsOwner)
        {

            healthBar = GameObject.Find("health_fg").GetComponent<Image>();
            transform.Find("canvas_player_overhead").gameObject.SetActive(false);
            FloatingTextController.Initialize();
            reticle_hit_controller.Initialize();
            if (networkObject.IsServer) {
                NetworkManager.Instance.Networker.playerAccepted += PlayerAccepted;
                NetworkManager.Instance.Networker.playerDisconnected += OnPlayerDisconnected;
            }
        }
       

        StartCoroutine(RequestUpdateFromEveryoneDelayed(2));//pozene coroutine, ki vsem network objektom, kateri imajo karkoli da se rab rocno sinhronizirat na clientih, ki so se ravnokar povezal, poslje rpc s katerim signalizira, da nj mu poslejo nazaj podatke s katerimi bo nastavu trenutno stanje objekta.
        if (networkObject.IsServer && networkObject.IsOwner) {
            NetworkGuildManagerBehavior beh = NetworkManager.Instance.InstantiateNetworkGuildManager();
            this.serverSide_guildManager = beh.gameObject;
            StartCoroutine(serverPlayerInitDelayer(1));
        }
    }

    public IEnumerator serverPlayerInitDelayer(float t) {
        yield return new WaitForSeconds(t);

        ServerSendOnAcceptedData();
    }

    public void Update()
    {

        if (networkObject == null)
        {
            // Debug.LogError("networkObject is null.");
            return;
        }
        if (myNetWorker == null)
        {
            if (GameObject.Find("NetworkManager(Clone)") != null)
            {
                myNetWorker = GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
            }
        }

        if (networkObject.IsServer && Input.GetKeyDown(KeyCode.X))
        {
            // Debug.Log("Spawning uma");
            spawn_UMA_body(transform.position, get_UMA_to_string(), 0);

        }

        if (Input.GetButtonDown("Interact") && this.dead && networkObject.IsOwner)
        {
            networkObject.SendRpc(RPC_RESPAWN_REQUEST, Receivers.Server);
        }

        if (Input.GetButtonDown("Interact") && networkObject.IsOwner)
        {
            Debug.Log(this.dead);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && networkObject.IsOwner)
        {
            handleEscapePressed();
        }

        if (Input.GetButtonDown("Guild") && networkObject.IsOwner) {
            //zapri inventorij, zapri guildModification, ??

            if (npi.panel_inventory.activeSelf) npi.panel_inventory.SetActive(false);
            if (this.guild_modification_panel.activeSelf) this.guild_modification_panel.SetActive(false);
            GameObject.FindGameObjectWithTag("GuildManager").GetComponent<NetworkGuildManager>().toggleMemberPanel();
        }

        if(networkObject.IsOwner)
            if (this.guild_modification_panel.activeSelf) {
                if (!Cursor.visible) Cursor.visible = true;
                if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;

            }

        /*  if (this.test) {
              this.health = 0;
              handle_0_hp();
          }*/
        if(networkObject.IsServer)
            if (this.acceptedAndNotUpdatedPlayers.Count > 0 && !AcceptedPlayerHandlingPending) {//updejtej playerja k je na vrhu vrste
                this.AcceptedPlayerHandlingPending = true;
                StartCoroutine(HandleAcceptedPlayersData(2f));
            
            }

    }

    public void OnSubmitModifiedGuildDataClick() {
        GameObject.FindGameObjectWithTag("GuildManager").GetComponent<NetworkGuildManager>().OnModifyGuildConfirmClick();
    }


    private void handleEscapePressed()
    {
        //ce je odprto ksno okno ga zapri, sicer prikaz main menu
        if (guild_modification_panel.activeSelf || npi.panel_inventory.activeSelf)
        {
            showGuildModificationPanel(false, null);
            npi.panel_inventory.SetActive(false);//nevem kaj se nrdi z itemi lol..
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else {
            Debug.LogWarning("trying to show main menu but i dont have anything yet");
        }
    }

    /*
HOW DAMAGE WORKS RIGHT NOW:
na serverju se detektira hit. trenutno edina skripta ki to dela je Weapon_Collider_handler, ki poklice tole metodo. ta metoda izracuna nov health od tega k je bil napaden. to vrednost poslje
napadenmu playerju da si poupdejta health. ta player pol ko si je updejtov health poslje nov rpc vsem drugim da nj si nastavijo njegov health na njegov health. tud server(i can see how this is bad ampak za prototip me ne skrbi.)
*/

    public uint[] getTeam() {
        return this.team;
    }

    public void take_weapon_damage_server_authority(Item weapon,string tag_passive, string tag_agressor ,uint passive_player_server_network_id, uint agressor_server_network_id)
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
            Item temp;
            if (tag_passive.Equals("coll_0"))
            {
                temp = npi.getHeadItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.damage_reduction;

            }
            else if (tag_passive.Equals("coll_1"))
            {
                temp = npi.getChestItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.damage_reduction;

                temp = npi.getHandsItem();
                if (temp != null)
                    armor_damage_reduction_modifier += temp.damage_reduction;

            }
            else if (tag_passive.Equals("coll_2")) {
                temp = npi.getLegsItem();
                if (temp != null)
                    armor_damage_reduction_modifier = temp.damage_reduction;

                temp = npi.getFeetItem();
                if (temp != null)
                    armor_damage_reduction_modifier += temp.damage_reduction;
            }

            armor_damage_reduction_modifier = 1 - armor_damage_reduction_modifier;
            float all_modifiers = locational_damage_reduction * current_block_damage_reduction * armor_damage_reduction_modifier;
            //-------------------------------------------------------------------------------------------------------------
            float final_damage_taken = dmg * all_modifiers;
            float new_hp= this.health - final_damage_taken;// - health bomo poslal po rpcju - tud host bo sam seb poslov tko da nima veze ce kdo spreminja network.
            if (new_hp < 0) new_hp = 0;
            //healthBar.fillAmount = (float)this.health / (float)this.max_health;

            lock (myNetWorker.Players)
            {
                int count = 0;//v koliziji sta udelezena dva igralca, poiskat moramo oba. tukej je lahko problem ce klicemo to metodo pri koliziji z ne-igralcem, za agresorja bo slo vedno cez vse igralce.
                myNetWorker.IteratePlayers((player) =>
                {
                    if (player.NetworkId == passive_player_server_network_id) //passive target
                    {
                        //Debug.Log("Victim found! "+ passive_player_server_network_id);
                        networkObject.SendRpc(RPC_SET_HEALTH,Receivers.All, new_hp, tag_passive);
                        count++;

                        if (prev_hp == 0 && final_damage_taken > 0) {
                            //death
                            handle_death_player(passive_player_server_network_id);
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

    internal uint Get_server_id()
    {
        return networkObject.Owner.NetworkId;
    }

    internal void SendGuildUpdate(NetworkGuildManager.Guild g)
    {
        if (networkObject.IsServer)
        {
            this.SendGuildUpdate(g.name, g.tag, g.color, g.image);
        }
    }

    internal void SendGuildUpdate(string name, string tag, Color color, byte[] image)
    {
        if (networkObject.IsServer) {
            if (image == null) image = new byte[25];
            if (tag == null) tag = "asd";
            networkObject.SendRpc(RPC_GUILD_UPDATE, Receivers.All, name, tag, color,image);
        }
    }

    internal void SendGuildUpdateToPlayer(NetworkingPlayer p,string name, string tag, Color color, byte[] image)
    {
        if (networkObject.IsServer)
        {
            if (image == null) image = new byte[25];
            if (tag == null) tag = "asd";
            networkObject.SendRpc(p,RPC_GUILD_UPDATE, name, tag, color, image);
        }
    }

    private void handle_death_player(uint player_id)//samo na serverju
    {
        if (!networkObject.IsServer) return;

        // Debug.Log("Spawning body");
        if(GetComponent<NetworkPlayerInventory>().backpackSpot.GetComponentInChildren<NetworkBackpack>()!=null)
            GetComponent<NetworkPlayerInventory>().backpackSpot.GetComponentInChildren<NetworkBackpack>().local_server_BackpackUnequip();

        spawn_UMA_body(transform.position, get_UMA_to_string(), player_id);//poslje rpc da nrdi uma body in disabla renderer za playerja v enem
                                                                           // server mora vsem sporocit da nj nehajo renderat playerja k je lihkar umru ker ga je vizualno zamenjov ragdoll
        networkObject.SendRpc(RPC_ON_PLAYER_DEATH, Receivers.All);
    }

    private void handle_respawn_player() {
        if (!networkObject.IsServer) return;
        if(check_for_validity_of_respawn_request())
            networkObject.SendRpc(RPC_RESPAWN_SIGNAL, Receivers.All, transform.position);

    }

    private bool check_for_validity_of_respawn_request()
    {
        //tle bo kao za prevert ce se player sme respawnat al je slucajn kej pohackov da se prej respawna, recimo da pohacka timer al pa kej.
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
        lock (myNetWorker.Players)
        {
            myNetWorker.IteratePlayers((player) =>
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


    }

    public void handle_player_pickup() {
        this.downed = false;
        GetComponent<NetworkPlayerAnimationLogic>().handle_player_revived();// z tlele k smo smo dobil lahko samo pobiranje igralca. execution bomo klical z drugje in takrat damo na false

    }




    /// <summary>
    /// server poklice da se nastav health na skripti. klice se na vsah skriptah, tud an serverju
    /// </summary>
    /// <param name="args"></param>
    public override void setHealth(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId!=0) 
            return;
        

        float hp = args.GetNext<float>();
        float prev_hp = this.health;
        bool downed = false;
        if (this.health > 0 && hp == 0)
            downed = true;
        this.health = hp;



        if (downed) handle_0_hp();



        string tag = args.GetNext<string>();
        if (!tag.Equals("block_player") && !tag.Equals("revive")) GameObject.Instantiate(this.sound_effects_on_player[0]);//tag ni od objekta al pa kej. je samo kot parameter da se ve, da smo pobral cloveka
        this.healthBar.fillAmount = this.health / (this.max_health);
        FindByid(NetworkManager.Instance.Networker.Me.NetworkId).GetComponent<NetworkPlayerStats>().team_panel.refreshHp(Get_server_id(), this.healthBar.fillAmount);
        
        //}
    }

    public override void ReceiveNotificationForDamageDealt(RpcArgs args)//tole funkcijo dobi owner agresor objekta in izrise na ekran da je naredu damage, rpc poslje server v metodi take_damage_server_authority
    {

       // if (!networkObject.IsOwner) Debug.Log("I dont know why this prints out. The server is the owner of one of the objects. what the hell?");

        float dmg = args.GetNext<float>();
        string tag = args.GetNext<string>();
       // Debug.Log("I Did Damage! "+tag);
        FloatingTextController.CreateFloatingText(dmg+"", Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane)),tag);
        reticle_hit_controller.CreateReticleHit(tag); //cod2 reticle hit style like
    }

    public override void respawnRequest(RpcArgs args)//poslje client serverju
    {
        if (!networkObject.IsServer) return;
        handle_respawn_player();
    }

    public override void respawnSignal(RpcArgs args)//poslje server vsem, tud sebi
    {
        //nastav spremenljivke povsod
        this.downed = false;
        this.dead = false;

        local_setDrawingPlayer(true);

        if (networkObject.IsServer)//server nastima vsem health
            set_player_health(max_health/2, Get_server_id());

    }

    /// <summary>
    /// metoda k jo dobijo vsi
    /// </summary>
    /// <param name="args"></param>
    public override void OnPlayerDeath(RpcArgs args)//vsi dobijo
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            local_setDrawingPlayer(false);//v drugi metodi zato ker se klice se z vsaj ene druge metode

            //ce "umre" mormo tud resetirat za animacijo da ne lezi vec na tleh. to se izvede na vseh clientih in serverju
            
            this.downed = false;
            this.dead = true; //to bi moral lockat combat pa tak
            this.health = 0;
            Debug.Log("player died! - downed: " + this.downed + " dead: " + this.dead);
            GetComponent<NetworkPlayerCombatHandler>().handle_player_death();//disabla shield pa weapon
            GetComponent<NetworkPlayerAnimationLogic>().handle_player_death();
        }
    }

    private void local_setDrawingPlayer(bool b) {
        transform.Find("UMARenderer").gameObject.SetActive(b);
        //izris prica al karkoli bo ze letel po zraku do tvojga otroka da ga possessa(!b)
        this.soul.gameObject.SetActive(!b);
        GetComponent<Collider>().enabled = b;
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
        lock (myNetWorker.Players)
        {

            myNetWorker.IteratePlayers((player) =>
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

                lock (myNetWorker.Players)
                {
                    
                    myNetWorker.IteratePlayers((player) =>
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
            this.decision_handler.draw_team_invite_decision(other_gameobject);

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
        lock (myNetWorker.Players)
        {

            myNetWorker.IteratePlayers((player) =>
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

            //samo ownerju se nastav ime v inventoriju. drugi nimajo sploh canvasa
            if (networkObject.IsOwner) {this.inventory_guild_name.text = this.name_guild; }

            // vsi nastavjo
            updateDisplayName();
        }
    }

    internal void showGuildModificationPanel(bool b,NetworkGuildManager ngm)
    {
        if (b && ngm!=null) {
            if (ngm.name_guild == null) ngm.name_guild = this.guild_name_input;
            if (ngm.tag_guild == null) ngm.tag_guild = this.guild_tag_input;
            if (ngm.color_guild == null) ngm.color_guild = this.guild_color_input;
        }
        this.guild_modification_panel.SetActive(b);
        if(npi!=null)if(npi.panel_inventory!=null)if(npi.panel_inventory.activeSelf)npi.panel_inventory.SetActive(false);

        GetComponent<NetworkPlayerAnimationLogic>().hookChestRotation = !b;
        GetComponent<NetworkPlayerMovement>().lockMovement = b;
        GetComponent<player_camera_handler>().lockCamera = b;
    }



    /// <summary>
    /// Fired when the player has been officially accepted by the server and now is the time you are able to start sending your messages to this player.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="sender"></param>
    private void PlayerAccepted(NetworkingPlayer player, NetWorker sender)//nevem tocn kaj nam networker pomaga tbh..
    {
        if (networkObject.IsServer)
        {
            Debug.Log("SERVER : player was accepted to server : " + player.NetworkId + " - updating players data.");

            //kaj ce bi mel logiko connectanja in disconnectanja nrjeno tko da ko se prvic sconnecta se ne nrdi skor nc, ko se dc-ja se njegov objekt samo disabla ampak ne zbrise.
            //ko se player reconnecta - se na podlagi njegovga steam id-ja poisce ce ze obstaja njegov objekt v spilu.
            //ce obstaja se njegov trenutni objekt ubije in preveze vso logiko (ownership, camero) na ta najdeni objekt.

            //ownerju povej da naj prevzame ta objekt. drugim verjetno ni treba nic povedat ker se bojo animacije avtomatsko pohendlale zarad akcij ownerja. tko nekak k conan exiles.

            //UnityMainThreadDispatcher.Instance().Enqueue(PushToMainThreadPlayerAcceptedData(player,"JEBAC"));
            this.acceptedAndNotUpdatedPlayers.Enqueue(player);
        }
    }

    /// <summary>
    /// sprozi se, ko se player sconnecta na server. Ker je unity mal prizadet dela to tko da gre pogledat ce kter player caka v vrsti za updejt, vzame enga in klice metodo na njegovem objektu da nj poskrbi za dejanski updejt
    /// </summary>
    /// <param name="time_delay"></param>
    /// <returns></returns>
    public IEnumerator HandleAcceptedPlayersData(float time_delay)
    {
        if (networkObject.IsServer)
        {
            yield return new WaitForSeconds(time_delay);
            Debug.Log("This is executed from the main thread");

            NetworkingPlayer p = this.acceptedAndNotUpdatedPlayers.Dequeue();
            GameObject obj = FindByid(p.NetworkId);
            if (obj != null)
            {
                NetworkPlayerStats stats = obj.GetComponent<NetworkPlayerStats>();
                if (stats != null)
                {
                    stats.ServerSendOnAcceptedData();//this is wherer the amgic happens

                }
                else
                {
                    this.acceptedAndNotUpdatedPlayers.Enqueue(p);//ker je failal pohendlat ga mormo pohendlat ksnej enkat.
                }
            }
            else
            {
                this.acceptedAndNotUpdatedPlayers.Enqueue(p);//ker je failal pohendlat ga mormo pohendlat ksnej enkat.
            }
            AcceptedPlayerHandlingPending = false;
        }
        yield return null;
    }

    /// <summary>
    /// metoda poisce podatke o tem playerju, ki se je ravnokar sconnectal na server. updejta objekt z temi podatki na serverju, server poskrbi za sinhronizacijo po omrezju
    /// </summary>
    /// <param name="name"></param>
    private void ServerSendOnAcceptedData() {
        PlayerManager.PlayerState saved_playerState = PlayerManager.Instance.PopPlayerState(Get_server_id());//hacky. PlayerManager bo treba dat na singleton..


        //serverju bo treba poslat drug data. vsi nesmejo dobit podatkov o inventoriju recimo. samo server ga mora.. bomo podatke k jih rab samo server met zapisal direkt pa je
        networkObject.SendRpc(RPC_RECEIVE_PERSONAL_DATA_ON_CONNECTION,Receivers.All,
            saved_playerState.position,
            saved_playerState.rotation,
            saved_playerState.playerName,
            saved_playerState.dead,
            saved_playerState.health,
            saved_playerState.head.id,
            saved_playerState.chest.id,
            saved_playerState.hands.id,
            saved_playerState.legs.id,
            saved_playerState.feet.id,
            saved_playerState.ranged.id,
            saved_playerState.weapon_0.id,
            saved_playerState.weapon_1.id,
            saved_playerState.shield.id,
            saved_playerState.backpack.id
            );//nevem kaj nrdit z backpackom....

        this.npi.items = saved_playerState.items;


        //ce ne dobimo nobenga guilda z updejta bomo ustvarli novga.


        NetworkGuildManager.Guild playersGuild = null;
        //players_guild = NetworkGuildManager.findPlayersGuild(this.GetSteamworksID());
        if (playersGuild == null) {
            playersGuild = GameObject.FindGameObjectWithTag("GuildManager").GetComponent<NetworkGuildManager>().CreateGuild(networkObject.Owner.NetworkId, networkObject.Owner.NetworkId + "'s clan", networkObject.Owner.NetworkId + "-S", Color.gray, new byte[25]);
            if (playersGuild != null) {
                SendGuildUpdate(playersGuild.name, playersGuild.tag, playersGuild.color, playersGuild.image);
            }
        }
    }

    /// <summary>
    /// sprozi se na vsah clientih za ta objekt. poslje server vsem podatke o tem objektu. ta objekt se je ravnokar REconnectal na server in to je bilo njegovo prejsnje stanje.
    /// </summary>
    /// <param name="args"></param>
    public override void ReceivePersonalDataOnConnection(RpcArgs args)
    {

        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            Debug.Log("Client: Updating player data with server side data.");

            transform.position = args.GetNext<Vector3>();
            transform.rotation = args.GetNext<Quaternion>();
            this.playerName = args.GetNext<string>();
            this.dead = args.GetNext<bool>();
            this.health = args.GetNext<float>();

            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);
            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);
            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);
            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);
            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);

            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);
            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);
            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 1);//WEAPON 1
            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);
            this.npi.SetLoadoutItem(Mapper.instance.getItemById(args.GetNext<int>()), 0);//Backpack??



            updateDisplayName();
            this.npi.refresh_UMA_equipped_gear();
            GetComponent<NetworkPlayerCombatHandler>().update_equipped_weapons();

        }
    }

    /// <summary>
    /// klice owner. zatezi serverju da nj mu poslje podatke za vsak objekt
    /// </summary>
    /// <param name="time_delay"></param>
    /// <returns></returns>
    public IEnumerator RequestUpdateFromEveryoneDelayed(float time_delay) {
        yield return new WaitForSeconds(time_delay);

        //vsem network objektim poslji zahtevo da poslejo podatke
        if (networkObject.IsOwner)
            SendGetALL();
    }

    internal void SendGetALL() {
        networkObject.SendRpc(RPC_GET_ALL, Receivers.Server);
    }
    /// <summary>
    /// dobi server. klice VSE skripte V SCENI synchronizerja da pohendlajo sinhronizacijo novga playerja
    /// </summary>
    /// <param name="args"></param>
    public override void GetAll(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            foreach (NetworkStartupSynchronizer synchronizer in UnityEngine.Object.FindObjectsOfType<NetworkStartupSynchronizer>()){
                synchronizer.SendDataToStartingClient(args.Info.SendingPlayer);
            }
        }
    }

    /// <summary>
    /// tole je narobe se mi zdi. mogl bi poslat vsem ne samo njemu
    /// </summary>
    /// <param name="p"></param>
    public void ServerSendAll(NetworkingPlayer p) {
        if (!networkObject.IsServer) return;

        //kar se tice guilda nrdimo kr guild update.
        SendGuildUpdateToPlayer(p,this.name_guild, this.tag_guild, this.color_guild, this.image_guild);
        //health
        networkObject.SendRpc(p, RPC_REFRESH_HEALTH, this.health);
        //ostalo
        networkObject.SendRpc(p,RPC_SEND_ALL, this.playerName, this.downed, this.dead);
    }

    /// <summary>
    /// player dobi podatke od NetworkPlayerStats skripte ki mu se manjklajo za sinhronizacijo
    /// </summary>
    /// <param name="args"></param>
    public override void SendAll(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            this.playerName=args.GetNext<string>();
            this.downed = args.GetNext<bool>();
            this.dead = args.GetNext<bool>();
            updateDisplayName();
        }
    }

    public override void RefreshHealth(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
            this.health = args.GetNext<float>();
    }


    /// <summary>
    /// Fired when the player has been officially accepted by the server and now is the time you are able to start sending your messages to this player.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="sender"></param>
    private void OnPlayerDisconnected(NetworkingPlayer player, NetWorker sender)//nevem tocn kaj nam networker pomaga tbh..
    {
        if (networkObject.IsServer)
        {
            Debug.Log("SERVER : player was Disconnected : " + player.NetworkId);

            //shrani v PlayerManagerja
            PlayerManager.Instance.SavePlayerState(FindByid(player.NetworkId).GetComponent<NetworkPlayerStats>().GetPlayerState());
        }
    }

    /// <summary>
    /// nrdi objekt playerstate k se shrani v playermanagerja in se poslje ob reconnectu za updejt njegovega stanja
    /// </summary>
    /// <returns></returns>
    public PlayerManager.PlayerState GetPlayerState() {
        PlayerManager.PlayerState ps = new PlayerManager.PlayerState(Get_server_id());

        //nafilamo podatke
        Debug.Log("GetPlayerState()- creating data.");
        ps.position = transform.position;
        ps.rotation = transform.rotation;
        ps.playerName = playerName;
        ps.dead = dead;
        ps.health = health;


        ps.items = npi.items;
        ps.head=npi.getHeadItem();
        ps.chest = npi.getChestItem();
        ps.hands = npi.getHandsItem();
        ps.legs = npi.getLegsItem();
        ps.feet = npi.getFeetItem();
        ps.ranged = npi.getRangedItem();
        ps.weapon_0 = npi.getWeapon_0Item();
        ps.weapon_1 = npi.getWeapon_1Item();
        ps.shield = npi.getShieldItem();
        ps.backpack = npi.getBackpackItem();

        return ps;
    }
}
