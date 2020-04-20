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
            Debug.LogWarning("Not implemented yet.");
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
            Debug.LogWarning("Not implemented yet.");
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
            Debug.LogWarning("Not implemented yet.");
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
            Debug.LogWarning("Not implemented yet.");
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
            Debug.LogWarning("Not implemented yet.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception when saving player" + s.GetType().Name + " " + e.Message);
            return false;
        }
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

        //-------------------------------------------------------------------
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
