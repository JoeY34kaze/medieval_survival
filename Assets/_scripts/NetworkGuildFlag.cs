using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using Random = UnityEngine.Random;
using System.IO;

public class NetworkGuildFlag : NetworkLandClaimObjectBehavior
{

    [SerializeField] private Renderer flag_rend;
    public Texture2D flag_texture;

    public float interactable_distance = 5f;

    public string path_flag_image = "\\Medieval Survival\\images\\company_flag.png";
    public bool try_flag=false;
    public readonly static float influence_radius=25f;
    public System.DateTime placed_time;//DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond);

    public List<uint> authorized_players;
    public uint owner = 0;

    public List<NetworkPlaceable> placeables_for_upkeep;

    private float upkeep_rate=0.5f; //% per tick

    public Item wood;
    public Item stone;
    public Item iron;
    public Item gold;

    private void Start()
    {
        //this.flag_texture = get_random_texture();
        if (!this.networkObject.IsServer)
            authorized_players = new List<uint>();
    }

    internal void init() {//init, ker moramo v networkplaceable postimat parametre, preden lahko klicemo to metodo. sicer bi bila na networkstart()
        if (networkObject.IsServer) {
            this.placed_time = System.DateTime.Now;
            networkObject.SendRpc(RPC_SEND_DATE_TIME_PLACED, Receivers.Others, this.placed_time.Year, this.placed_time.Month, this.placed_time.Day, this.placed_time.Hour, this.placed_time.Minute, this.placed_time.Second, this.placed_time.Millisecond);
            this.authorized_players = new List<uint>();
            this.authorized_players.Add(GetComponent<NetworkPlaceable>().get_creator());
            networkObject.SendRpc(RPC_UPDATE_AUTHORIZED_LIST, Receivers.Others, this.authorized_players.ObjectToByteArray());
            this.placeables_for_upkeep = new List<NetworkPlaceable>();
            refresh_all_placeables_in_range();

            networkObject.onDestroy += Clear_upkeep_placeables(); 
        }
    }

    private NetWorker.BaseNetworkEvent Clear_upkeep_placeables()
    {
        foreach (NetworkPlaceable p in this.placeables_for_upkeep)
            p.upkeep_flag = null;
        return null;
    }

    private void refresh_all_placeables_in_range() {
        this.placeables_for_upkeep.Clear();
        foreach (NetworkPlaceable p in GameObject.FindObjectsOfType<NetworkPlaceable>()) {
            if (Vector2.Distance(new Vector2(p.transform.position.x, p.transform.position.z), new Vector2(transform.position.x, transform.position.z))<NetworkGuildFlag.influence_radius) {
                this.placeables_for_upkeep.Add(p);
                p.upkeep_flag = this;
            }
        }
    }

    internal void add_placeable_for_upkeep(NetworkPlaceable p) {
        if (!this.placeables_for_upkeep.Contains(p))
            this.placeables_for_upkeep.Add(p);
    }

    internal bool is_player_authorized(uint networkId)
    {
        return this.authorized_players.Contains(networkId);
    }


    private IEnumerator load_flag_from_file()
    {

        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture("file://" + System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents") + path_flag_image);                  // "download" the first file from disk
        yield return www.SendWebRequest();                                                               // Wait unill its loaded
        Texture2D user_texture = ((UnityEngine.Networking.DownloadHandlerTexture)www.downloadHandler).texture;
        //user_texture.Resize(1024, 1024);
        // put the downloaded image file into the new Texture2D
        this.flag_rend.material.mainTexture = user_texture;           // put the new image into the current material as defuse material for testing.
        this.flag_texture = user_texture;
        local_send_flag_texture_to_server();
    }

    internal bool pay_upkeep_for(NetworkPlaceable p)
    {
        //upkeep 
        //vzel bomo prvo vrednost iz arraya recepta. recept za placeable iteme bo zmer biu samo wood - kolicina. kaj se placa je odvisn od tiera objekta.
        //t0 -wood
        //w1 - stone -> iron
        if (p.p.item.recepie.ingredient_quantities.Length < 1) return true;
        int cost = p.p.item.recepie.ingredient_quantities[0];
        cost = (int)(cost * upkeep_rate);

        //upkeep resourcov
        switch (p.p.tier) {
            case 0:
                if (!remove_from_container(this.wood, cost)) return false;
                break;
            case 1:
                if (!remove_from_container(this.stone, cost)) return false;
                break;
            case 2:
                if (!remove_from_container(this.iron, cost)) return false;
                break;
            case 3:
                if (!remove_from_container(this.iron, 2*cost)) return false;
                break;
            case 4:
                if (!remove_from_container(this.iron, 4*cost)) return false;
                break;
            default:
                break;
        }
        //upkeep golda
        if (!remove_from_container(this.gold, p.p.tier * cost / 5)) return false;
        return true;
    }

    /// <summary>
    /// returns false if not enough resources
    /// </summary>
    /// <param name="i"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    private bool remove_from_container(Item i, int amount) {
        return GetComponent<NetworkContainer>().Remove(i, amount);
        
    }

    public Texture2D get_random_texture()
    {
        int width = 10;
        int height = 10;

        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                texture.SetPixel(j, height - 1 - i, Color.red * Random.Range(0f, 1.0f));
            }
        }
        texture.Apply();
        return texture;
    }
    public override void SendDateTimePlaced(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            int year = args.GetNext<int>();
            int month = args.GetNext<int>();
            int day = args.GetNext<int>();
            int h = args.GetNext<int>();
            int m = args.GetNext<int>();
            int s = args.GetNext<int>();
            int ms = args.GetNext<int>();
            this.placed_time = new System.DateTime(year, month, day, h, m, s, ms);
        }
    }

    //poslje server, klice se na clientih da poupdejtatjo. to je mogoce narobe ker je security issue za ESP. ce bo treba se da popravt da dela drugace, ampak mislim da ni panike

        //tole je ubistvu toggle¨!!!!
    public override void Authorization_request(RpcArgs args)
    {
        if (networkObject.IsServer)

            if (is_player_authorized(args.Info.SendingPlayer.NetworkId))
            {
                this.authorized_players.Remove(args.Info.SendingPlayer.NetworkId);
                networkObject.SendRpc(RPC_UPDATE_AUTHORIZED_LIST, Receivers.Others, this.authorized_players.ObjectToByteArray());
            }
             else if (is_player_allowed_to_authorize(args.Info.SendingPlayer.NetworkId))
            {

                    this.authorized_players.Add(args.Info.SendingPlayer.NetworkId);
                    networkObject.SendRpc(RPC_UPDATE_AUTHORIZED_LIST, Receivers.Others, this.authorized_players.ObjectToByteArray());
                
            }
    }

    private bool is_player_allowed_to_authorize(uint networkId)
    {
        return Vector3.Distance(FindByid(networkId).transform.position, transform.position) < this.interactable_distance;
    }

    public override void Remove_authorization_request(RpcArgs args)
    {
        throw new NotImplementedException();

    }

    public override void Clear_all_authorized_request(RpcArgs args)
    {
        if (networkObject.IsServer)
            if (is_player_authorized(args.Info.SendingPlayer.NetworkId))
            {
                this.authorized_players.Clear();
                networkObject.SendRpc(RPC_UPDATE_AUTHORIZED_LIST, Receivers.Others, this.authorized_players.ObjectToByteArray());
            }
    }

    public override void SendFlagTextureToServer(RpcArgs args)
    {
        if (networkObject.IsServer) {
            if (is_user_allowed_to_change_flag_image(args.Info.SendingPlayer.NetworkId)) {
                Byte[] raw_data_byte = args.GetNext<byte[]>();
                this.flag_texture.LoadRawTextureData(raw_data_byte.ByteArrayToObject<byte[]>());//lol tole ubistvu dvakrat zakodiran lol. eh
                this.flag_rend.material.mainTexture = this.flag_texture;
                networkObject.SendRpc(RPC_UPDATE_FLAG_TEXTURE_ON_CLIENTS, Receivers.Others, this.flag_texture.GetRawTextureData());
            }
        }

    }

    private bool is_user_allowed_to_change_flag_image(uint network_id)
    {
        return this.authorized_players.Contains(network_id);
    }

    public override void UpdateFlagTextureOnClients(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.flag_texture.LoadRawTextureData(args.GetNext<byte[]>());//lol tole ubistvu dvakrat zakodiran lol. eh
            this.flag_rend.material.mainTexture = this.flag_texture;
        }
    }

    public override void UpdateAuthorizedList(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.authorized_players = args.GetNext<byte[]>().ByteArrayToObject<List<uint>>();
        }
    }


    private void local_send_flag_texture_to_server() {
        networkObject.SendRpc(RPC_SEND_FLAG_TEXTURE_TO_SERVER, Receivers.Server, this.flag_texture.GetRawTextureData());
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana povsod
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        return null;
    }

    internal void local_flag_toggle_authorized_request()
    {
        networkObject.SendRpc(RPC_AUTHORIZATION_REQUEST, Receivers.Server);
    }

    internal void local_flag_upload_image_request()
    {
        if(is_user_allowed_to_change_flag_image(networkObject.MyPlayerId))
            StartCoroutine(load_flag_from_file());
    }

    internal void local_clear_all_request()
    {
        if (is_player_authorized(networkObject.MyPlayerId))
            networkObject.SendRpc(RPC_CLEAR_ALL_AUTHORIZED_REQUEST, Receivers.Server);
    }

    public static NetworkGuildFlag get_dominant_guild_flag_in_range(Vector3 pos)
    {
        NetworkGuildFlag[] all_flags = GameObject.FindObjectsOfType<NetworkGuildFlag>();

        NetworkGuildFlag dominant = null;
        DateTime min_value = System.DateTime.MaxValue;
        for (int i = 0; i < all_flags.Length; i++)
        {
            NetworkGuildFlag f = all_flags[i];
            Vector2 player_pos = new Vector2(pos.x, pos.z);
            Vector2 flag_pos = new Vector2(f.transform.position.x, f.transform.position.z);
            if (Vector2.Distance(player_pos, flag_pos) < NetworkGuildFlag.influence_radius && (DateTime.Compare(f.placed_time, min_value) < 0))
            {
                min_value = f.placed_time;
                dominant = f;
            }
        }
        return dominant;

    }
}
