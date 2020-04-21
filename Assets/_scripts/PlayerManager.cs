using BeardedManStudios.Forge.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    #region SINGLETON PATTERN
    public static PlayerManager _instance;
    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PlayerManager>();

                if (_instance == null)
                {
                    GameObject container = new GameObject("PlayerManager");
                    _instance = container.AddComponent<PlayerManager>();
                }
            }

            return _instance;
        }
    }
    #endregion



    /// <summary>
    /// hrani stanja playerjev. ko se dc-ja se zapise stanje, ko se reconnecta se mu poslje stanje da povoz lokalne stvari
    /// </summary>
    /// 

    private static List<PlayerState> _plstts;
    private static List<PlayerState> playerStates {
        get { 
            if (_plstts == null)
                _plstts = new List<PlayerState>();
            return _plstts;
                    }
    }


    /*  -----------------------------------------------------------------------------------------------------------------------------------------------
        |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||   INITIALIZATION OVER |||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        ----------------------------------------------------------------------------------------------------------------------------------------------- */

    #region SAVING SCRIPTS
    internal static bool Save(NetworkPlayerMovement s) {
        try
        {
            uint net_id = s.networkObject.Owner.NetworkId;
            uint steamId = 666;
            PlayerState ps = PlayerManager.get_playerStateForPlayer(steamId);
            if (ps == null)
            {
                ps = PlayerManager.CreateNewPlayerState(net_id, steamId);
            }

            ps.current_gravity_velocity = s.current_gravity_velocity;
            ps.position = s.transform.position;
            ps.rotation = s.transform.rotation;
            
            return true;
        }
        catch (Exception e) {
            Debug.LogError("Exception when saving player" + s.GetType().Name + " " + e.Message);
            return false;
        }

    }


    internal static bool Save(NetworkPlayerInteraction s)
    {
        try
        {
            Debug.LogWarning("Nothing to save in Interaction.");
            return true;
        }
        catch (Exception e) {
            Debug.LogError("Exception when saving player"+s.GetType().Name+  " "+ e.Message);
            return false;
        }
    }

    internal static bool Save(NetworkPlayerAnimationLogic s)
    {
        try
        {
            uint net_id = s.networkObject.Owner.NetworkId;
            uint steamId = 666;
            PlayerState ps = PlayerManager.get_playerStateForPlayer(steamId);
            Debug.LogWarning("Not implemented yet.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception when saving player" + s.GetType().Name + " " + e.Message);
            return false;
        }
    }

    internal static bool Save(NetworkPlayerCombatHandler s )
    {
        try
        {
            Debug.LogWarning("Not saving anything in combat handler.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception when saving player" + s.GetType().Name + " " + e.Message);
            return false;
        }
    }

    internal static bool Save(NetworkPlayerInventory s)
    {
        try
        {
            uint net_id = s.networkObject.Owner.NetworkId;
            uint steamId = 666;
            PlayerState ps = PlayerManager.get_playerStateForPlayer(steamId);
            //List<PredmetRecepie> craftingQueue;   kaj nrdit z crafting queue?- skenslat vse i guess pa pol shrant pomoje
            s.cancelAllCrafting_server();
            ps.predmeti_personal = s.predmeti_personal;
            ps.predmeti_hotbar = s.predmeti_hotbar;
            ps.head=s.head;
            ps.chest=s.chest;
            ps.hands=s.hands;
            ps.legs=s.legs;
            ps.feet=s.feet;
            //public Predmet backpack;   ????????? zaenkrat pade na tla
            return true;

        }
        catch (Exception e)
        {
            Debug.LogError("Exception when saving player" + s.GetType().Name + " " + e.Message);
            return false;
        }
    }

    internal static bool Save(NetworkPlayerNeutralStateHandler s)
    {
        try
        {
            Debug.LogWarning("Not saving anything in animation logic.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception when saving player" + s.GetType().Name + " " + e.Message);
            return false;
        }
    }

    internal static bool Save(NetworkPlayerStats s)
    {
        try
        {
            uint net_id = s.networkObject.Owner.NetworkId;
            uint steamId = 666;
            PlayerState ps = PlayerManager.get_playerStateForPlayer(steamId);

            ps.playerName = s.playerName;
            if (s.downed || s.dead) {
                ps.dead = true;
                ps.health = 0;
            } else
                ps.health = s.health;
            ps.player_displayed_name = s.player_displayed_name.text;//tole verjetn nastavt drugje - runtime mashup
            ps.team=s.getTeam();

            //---------------------------------------------------- GUILD NAJ BI NACELOMA SOU U GUILD MANAGER
            /*
            public string name_guild = "no guild yet";
            public string tag_guild = "no tag yet";
            public Color color_guild = Color.red;
            public byte[] image_guild;
            */
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception when saving player" + s.GetType().Name + " " + e.Message);
            return false;
        }
    }

    internal static void load_player_from_saved_data(uint steamId, GameObject player_gameObject)
    {
        NetworkPlayerStats nps = player_gameObject.GetComponent<NetworkPlayerStats>();

        uint net_id = nps.networkObject.Owner.NetworkId;

        PlayerState ps = PlayerManager.get_playerStateForPlayer(steamId);
        if (ps == null) { Debug.Log("No player data found for steamid "+steamId+"."); return; }

        Debug.Log("Saved data found for player " + steamId);
        //-------------------------------------MOVEMENT-------------------------
        Debug.Log("Loading Movement");
        NetworkPlayerMovement m = player_gameObject.GetComponent<NetworkPlayerMovement>();
        m.current_gravity_velocity=ps.current_gravity_velocity; 
        player_gameObject.transform.position = ps.position;
        player_gameObject.transform.rotation=ps.rotation; 
        m.OnPlayerDataLoaded();


        //-----------------------------------Inventory---------------------------
        NetworkPlayerInventory npi = player_gameObject.GetComponent<NetworkPlayerInventory>();
        Debug.Log("Loading Inventory");
        npi.predmeti_personal=ps.predmeti_personal;
        npi.predmeti_hotbar=ps.predmeti_hotbar;
        npi.head=ps.head;
        npi.chest=ps.chest;
        npi.hands=ps.hands; 
        npi.legs=ps.legs;
        npi.feet=ps.feet;
        npi.OnPlayerDataLoaded();

        //------------------------------------STATS------------------------------
        Debug.Log("Loading Stats");
        nps.playerName = ps.playerName;
        nps.dead = ps.dead;
        nps.health = ps.health;
        nps.player_displayed_name.text = ps.player_displayed_name;
        nps.team = ps.team;
        nps.OnPlayerDataLoaded();
        //-----------------------------------------------------------------------
        Debug.Log("No other scripts contain anything that needs loaded. Complete!");

    }

    #endregion


    private static PlayerState CreateNewPlayerState(uint netId, uint steamId)
    {
        PlayerState p = new PlayerState(netId, steamId);
        PlayerManager.playerStates.Add(p);
        return p;
    }

    /// <summary>
    /// get PlayreState based on steamId. returns null if none.
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    public static PlayerState get_playerStateForPlayer(uint steamId) {
        //na 
        //TODO -> sorted list pa nekak pametno dostopat do njega ??

        foreach(PlayerState ps in PlayerManager.playerStates)
            if (ps.steamId == steamId)
                return ps;
        return null;
    }


    /// <summary>
    /// Podatkovni tip, ki hrani vse podatke, ki se updejtajo playerju, ki se sconnecta na server
    /// </summary>
    public class PlayerState {
        //general
        public uint previousNetworkId;
        public uint steamId;

        //-----------------------NETWORKPLAYERMOVEMENT.cs -----------------
        public Vector3 current_gravity_velocity { get; internal set; }
        public Vector3 position { get; internal set; }
        public Quaternion rotation { get; internal set; }

    //-----------------------NETWORKPLAYERINVENTORY.CS--------------------------------------------
        public Predmet[] predmeti_personal { get; internal set; }
        public Predmet[] predmeti_hotbar { get; internal set; }
        public Predmet head { get; internal set; }
        public Predmet chest { get; internal set; }
        public Predmet hands { get; internal set; }
        public Predmet legs { get; internal set; }
        public Predmet feet { get; internal set; }

    //-----------------------NETWORKPLAYERSTATS.CS--------------------------------------------


        public string playerName { get; internal set; }
        public bool dead { get; internal set; }
        public float health { get; internal set; }
        public string player_displayed_name { get; internal set; }
        public uint[] team { get; internal set; }

    //------------------------------------   CONSTRUCTOR ---------------------------------------

        public PlayerState(uint previousNetworkId, uint steamId) {
            this.previousNetworkId = previousNetworkId;
            this.steamId = steamId;
        }



        public bool saveToDisk() {
            Debug.LogError("Not implemented yet..");
            return false;
        }


    }
}
