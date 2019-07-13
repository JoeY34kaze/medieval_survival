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
    private List<PlayerState> _playerStates;
    // Start is called before the first frame update
    void Start()
    {
        this._playerStates = new List<PlayerState>();
    }

    public void SavePlayerState(PlayerState ps)
    {
        Debug.Log("Saving player state for player : " + ps.previousNetworkId);
        this._playerStates.Add(ps);
    }

    /// <summary>
    /// vrne podatke o playerju k so zapisan not. networkId je sam za testirat, ksnej se zamenja z steamId playerja ko bo vgrajen steamworksAPI
    /// </summary>
    /// <param name="networkId"></param>
    /// <returns></returns>
    public PlayerState PopPlayerState(uint networkId) {
        if(_playerStates!=null)
        if (_playerStates.Count > 0) {
            PlayerState r = _playerStates[0];
            _playerStates.Remove(r);
            Debug.Log("Popping the first player state - updating with data from player : " + r.previousNetworkId);
            return r;
        }
        return null;
    }

    /// <summary>
    /// Podatkovni tip, ki hrani vse podatke, ki jih mora player prejet takrat, ko se sconnecta na server. ta podatkovni tip se tudi zakodira rocno v forge network RPC in poslje novo prihajajocemu playerju
    /// </summary>
    public class PlayerState {
        //general
        public uint previousNetworkId;

        public Vector3 position;
        public Quaternion rotation;
        //stats
        public string playerName;
        public bool dead;
        public float health;

        //inventory ----------------------- poslal se bo samo id (int) nevem kaj nrdit z durability pa takim foram se..
        public Item[] items;
        public Item head;
        public Item chest;
        public Item hands;
        public Item legs;
        public Item feet;
        public Item ranged;
        public Item weapon_0;
        public Item weapon_1;
        public Item shield;
        public Item backpack;
        //umaData

        //guild ??
        public NetworkGuildManager.Guild guild;
        //team
        //public uint[] team;
        //??
        public PlayerState(uint previousNetworkId) {
            this.previousNetworkId = previousNetworkId;
        }


    }
}
