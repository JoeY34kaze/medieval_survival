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
    public byte[] flag_image_bytes;

    public Texture2D flag_tex;
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


    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();

            




            Debug.Log(flag_rend.material.mainTexture);
            Debug.Log(flag_rend.material.mainTexture.GetType());


            

            
            paint_flag(get_random_texture().EncodeToPNG());
            ///user_texture.Resize(512, 512);
            //user_texture.Apply();
            Debug.Log(this.flag_tex.format + " | " + this.flag_tex.graphicsFormat);
            // put the downloaded image file into the new Texture2D
           // this.flag_texture = user_texture;
            //this.flag_rend.material.mainTexture = this.flag_texture;


            networkObject.SendRpc(RPC_UPDATE_FLAG_TEXTURE_ON_CLIENTS, Receivers.Others, get_bytes_from_image(), 512, 512);
        }
        else {
            networkObject.SendRpc(RPC_CLIENT_ON_CONNECT_REQUEST, Receivers.Server);
        }
    }

    private Texture2D get_new_blank_texture()
    {
        return new Texture2D(512, 512, TextureFormat.ARGB32, false);
    }

    private void paint_flag(byte[] tex) {
        Texture2D n = get_new_blank_texture();
      //  n.LoadImage(arr);
        
        this.flag_tex = get_new_blank_texture();
        this.flag_tex.LoadImage(tex);
        this.flag_tex.Apply();
        this.flag_rend.material.mainTexture = this.flag_tex;
    }

    private byte[] get_bytes_from_image() {
        return this.flag_tex.EncodeToPNG();
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
            networkObject.onDestroy += (sender) => { Clear_upkeep_placeables(); };



        }
    }




    public Texture2D get_random_texture()
    {
        int width = 512;
        int height = 512;

        Texture2D texture = get_new_blank_texture();
        texture.filterMode = FilterMode.Point;
        float k = 0;
        for (int i = 0; i < width; i++)
        {
             if(i%10 ==0)k = Random.Range(0.0f, 1.0f);
            for (int j = 0; j < height; j++)
            {
                texture.SetPixel(j, height - 1 - i, Color.red * k);
            }
        }
        texture.Apply();
        return texture;
    }

    #region TEXTURE


    private IEnumerator load_flag_from_file()
    {
        if (!File.Exists(System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents") + path_flag_image))
        {
            Debug.Log("flag not found!");

        }
        else
        {


            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture("file://" + System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents") + path_flag_image);                  // "download" the first file from disk
            yield return www.SendWebRequest();                                                               // Wait unill its loaded
            Texture2D user_texture = ((UnityEngine.Networking.DownloadHandlerTexture)www.downloadHandler).texture;

            if (user_texture.height != 512 || user_texture.width != 512)
            {
                Debug.LogWarning("External texture is not 512 by 512!");
            }
            else
            {


                paint_flag(user_texture.EncodeToPNG());

                Debug.Log(user_texture.format + " | " + user_texture.graphicsFormat);
                // put the downloaded image file into the new Texture2D

                local_send_flag_texture_to_server();
            }
        }
    }


    private void local_send_flag_texture_to_server()
    {
      //  MainThreadManager.Instance.Execute(
       //     () => {
                networkObject.SendRpc(RPC_SEND_FLAG_TEXTURE_TO_SERVER, Receivers.Server, get_bytes_from_image(), 512, 512);
       //     }
       //     );
    }

    public override void SendFlagTextureToServer(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            if (is_user_allowed_to_change_flag_image(args.Info.SendingPlayer.NetworkId))
            {

                byte[] img = args.GetNext<byte[]>();
                int x = args.GetNext<int>();
                int y = args.GetNext<int>();
                paint_flag(img);
                networkObject.SendRpc(RPC_UPDATE_FLAG_TEXTURE_ON_CLIENTS, Receivers.Others, img, x, y);
            }
        }

    }

    private bool is_user_allowed_to_change_flag_image(uint network_id)
    {
        return this.authorized_players.Contains(network_id);
    }

    public override void UpdateFlagTextureOnClients(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost)
        {
            byte[] img = args.GetNext<byte[]>();
            paint_flag(img);
        }
    }
    internal void local_flag_toggle_authorized_request()
    {
        networkObject.SendRpc(RPC_AUTHORIZATION_REQUEST, Receivers.Server);
    }

    internal void local_flag_upload_image_request()
    {
        if (is_user_allowed_to_change_flag_image(networkObject.MyPlayerId))
            StartCoroutine(load_flag_from_file());
    }

    public override void client_on_connect_request(RpcArgs args)
    {
        if (networkObject.IsServer)
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_UPDATE_FLAG_TEXTURE_ON_CLIENTS, this.flag_image_bytes, 512, 512);
    }
    /* ----- resizing

    public enum ImageFilterMode : int
    {
        Nearest = 0,
        Biliner = 1,
        Average = 2
    }
    public static Texture2D ResizeTexture(Texture2D pSource, ImageFilterMode pFilterMode, float pScale)
    {

        //*** Variables
        int i;

        //*** Get All the source pixels
        Color[] aSourceColor = pSource.GetPixels(0);
        Vector2 vSourceSize = new Vector2(pSource.width, pSource.height);

        //*** Calculate New Size
        float xWidth = Mathf.RoundToInt((float)pSource.width * pScale);
        float xHeight = Mathf.RoundToInt((float)pSource.height * pScale);

        //*** Make New
        Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);

        //*** Make destination array
        int xLength = (int)xWidth * (int)xHeight;
        Color[] aColor = new Color[xLength];

        Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

        //*** Loop through destination pixels and process
        Vector2 vCenter = new Vector2();
        for (i = 0; i < xLength; i++)
        {

            //*** Figure out x&y
            float xX = (float)i % xWidth;
            float xY = Mathf.Floor((float)i / xWidth);

            //*** Calculate Center
            vCenter.x = (xX / xWidth) * vSourceSize.x;
            vCenter.y = (xY / xHeight) * vSourceSize.y;

            //*** Do Based on mode
            //*** Nearest neighbour (testing)
            if (pFilterMode == ImageFilterMode.Nearest)
            {

                //*** Nearest neighbour (testing)
                vCenter.x = Mathf.Round(vCenter.x);
                vCenter.y = Mathf.Round(vCenter.y);

                //*** Calculate source index
                int xSourceIndex = (int)((vCenter.y * vSourceSize.x) + vCenter.x);

                //*** Copy Pixel
                aColor[i] = aSourceColor[xSourceIndex];
            }

            //*** Bilinear
            else if (pFilterMode == ImageFilterMode.Biliner)
            {

                //*** Get Ratios
                float xRatioX = vCenter.x - Mathf.Floor(vCenter.x);
                float xRatioY = vCenter.y - Mathf.Floor(vCenter.y);

                //*** Get Pixel index's
                int xIndexTL = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                int xIndexTR = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));
                int xIndexBL = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                int xIndexBR = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));

                //*** Calculate Color
                aColor[i] = Color.Lerp(
                    Color.Lerp(aSourceColor[xIndexTL], aSourceColor[xIndexTR], xRatioX),
                    Color.Lerp(aSourceColor[xIndexBL], aSourceColor[xIndexBR], xRatioX),
                    xRatioY
                );
            }

            //*** Average
            else if (pFilterMode == ImageFilterMode.Average)
            {

                //*** Calculate grid around point
                int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
                int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
                int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
                int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);

                //*** Loop and accumulate
                Vector4 oColorTotal = new Vector4();
                Color oColorTemp = new Color();
                float xGridCount = 0;
                for (int iy = xYFrom; iy < xYTo; iy++)
                {
                    for (int ix = xXFrom; ix < xXTo; ix++)
                    {

                        //*** Get Color
                        oColorTemp += aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];

                        //*** Sum
                        xGridCount++;
                    }
                }

                //*** Average Color
                aColor[i] = oColorTemp / (float)xGridCount;
            }
        }

        //*** Set Pixels
        oNewTex.SetPixels(aColor);
        oNewTex.Apply();

        //*** Return
        return oNewTex;
    }
    */
    #endregion



    #region OTHER


    private void Clear_upkeep_placeables()
    {
        foreach (NetworkPlaceable p in this.placeables_for_upkeep)
            p.upkeep_flag = null;
        
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




    internal bool pay_upkeep_for(NetworkPlaceable p)
    {
        //upkeep 
        //vzel bomo prvo vrednost iz arraya recepta. recept za placeable iteme bo zmer biu samo wood - kolicina. kaj se placa je odvisn od tiera objekta.
        //t0 -wood
        //w1 - stone -> iron
        if (p.p.getItem().recepie.ingredient_quantities.Length < 1) return true;
        int cost = p.p.getItem().recepie.ingredient_quantities[0];
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

    private int[] get_upkeep_for_single_block_per_tick(NetworkPlaceable p) {

        int[] r = new int[4];//wood,stone,iron,gold
        for (int i = 0; i < r.Length; i++) r[i] = 0;
        if (p.p.getItem().recepie.ingredient_quantities.Length == 0) return r;

        int cost = p.p.getItem().recepie.ingredient_quantities[0];
        cost = (int)(cost * upkeep_rate);

        switch (p.p.tier)
        {
            case 0:
                r[0]+= cost;
                break;
            case 1:
                r[1] += cost;
                break;
            case 2:
                r[2] += cost;
                break;
            case 3:
                r[2] += 2*cost;
                break;
            case 4:
                r[2] += 4*cost;
                break;
            default:
                break;
        }
        r[3] += p.p.tier * cost / 5;

        return r;
    }

    internal int[] get_upkeep_for_24h()
    {
        int[] r = new int[4];

        r = get_upkeep_for_single_tick();
        for (int i = 0; i < r.Length; i++) {
            r[i] = r[i] * 24;
        }
        return r;
    }

    private int[] get_upkeep_for_single_tick()
    {
        int[] r = new int[4];//wood, stone, iron, gold
        for (int i = 0; i < r.Length; i++) r[i] = 0;

        foreach (NetworkPlaceable p in this.placeables_for_upkeep) {
                if (p.p.getItem().needs_upkeep)
                {
                    int[] s = get_upkeep_for_single_block_per_tick(p);
                    for (int i = 0; i < r.Length; i++) {
                        r[i] += s[i];
                    }
                }
        }
        return r;
    }

    private bool is_in_range(NetworkGuildFlag f, NetworkPlaceable p)
    {
        throw new NotImplementedException();
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


    public override void UpdateAuthorizedList(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.authorized_players = args.GetNext<byte[]>().ByteArrayToObject<List<uint>>();
        }
    }



    public GameObject FindByid(uint targetNetworkId) //koda kopširana povsod
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        return null;
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
    #endregion
}
