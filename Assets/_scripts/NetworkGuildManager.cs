using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// hrani bolj kot ne vse kar zahtevajo guildi. samo server ima vse guilde, clienti posiljajo zahteve z objektov ce rabjo vidt kter je njihov guild.
/// </summary>
public class NetworkGuildManager : NetworkGuildManagerBehavior
{
    #region FIELDS

    public GameObject localPlayer;


    public Text name_guild;
    public Text tag_guild;
    public Text color_guild;

    public List<Guild> guilds;

    public List<uint[]> valid_invites;

    //private UILogic uiLogic;

    private Guild currentGuild;

    #endregion

    #region SINGLETON PATTERN
    public static NetworkGuildManager _instance;
    public static NetworkGuildManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<NetworkGuildManager>();

                if (_instance == null)
                {
                    NetworkGuildManagerBehavior beh = NetworkManager.Instance.InstantiateNetworkGuildManager();
                    _instance = beh.gameObject.GetComponent<NetworkGuildManager>();
                }
            }

            return _instance;
        }
    }
    #endregion

    #region UNITY_METHODS
    protected override void NetworkStart()
    {
        base.NetworkStart();
        UILogic.Instance.ngm = this;
        if (networkObject.IsServer) {
            //load guilds from database or something...
            //if null
            if(this.guilds==null)this.guilds = new List<Guild>();
            networkObject.TakeOwnership();
            this.valid_invites = new List<uint[]>();
        }

       // localPlayer = FindByid(NetworkManager.Instance.Networker.Me.NetworkId);//?? i guess it should work. mrde bols da player pogleda pa poveze z druge strani..
       // localPlayerGuildInfoUpdateRequest();
        
    }



    #endregion

    #region KICK

    internal void localKickRequest(uint designated_player)
    {
        networkObject.SendRpc(RPC_KICK_GUILD_REQUEST, Receivers.Server, designated_player);
        toggleMemberPanel();
    }

    public override void KickGuildRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            uint requester = args.Info.SendingPlayer.NetworkId;
            uint faggot = args.GetNext<uint>();


            Guild from = getGuildFromNetworkId(requester);

            if (requester == from.guildMaster) {
                if (requester != faggot) {
                    from.removeMember(faggot);
                    sendGuildModifiedResponse(CreateGuild(faggot, FindByid(faggot).GetComponent<NetworkPlayerStats>().playerName + "'s Guild", args.Info.SendingPlayer.NetworkId + "-ST", "ff0000", null));
                    sendUserInfoResponseTo(faggot);
                }
            }
        }
    }

    #endregion

    #region LEAVE
    //poslje lokalni player samo
    internal void leaveGuildRequest()
    {
        networkObject.SendRpc(RPC_LEAVE_GUILD_REQUEST, Receivers.Server);
        toggleMemberPanel();
    }

    public override void LeaveGuildRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            uint requester = args.Info.SendingPlayer.NetworkId;
            Guild from = getGuildFromNetworkId(requester);
            if (from.members.Count <= 1)
            {
                //nemora leavat ce je sam not, to je njegov starter guild al pa nekej
            }
            else {//lahko leava
                from.removeMember(requester);
                //ce je leaval gm rabmo enga druzga dat u gm role
                if (from.guildMaster == requester) {
                    from.guildMaster = from.members[0];
                }
                //posljemo ostalim memberjim guilda updejt? - mislm da ni treba

                //kreiramo nov guild zanjga. njegov starter guild in poslemo njemu updejt
                sendGuildModifiedResponse(CreateGuild(args.Info.SendingPlayer.NetworkId, FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerStats>().playerName + "'s Guild", args.Info.SendingPlayer.NetworkId+"-ST" , "#ff0000", null));
                sendUserInfoResponseTo(args.Info.SendingPlayer);
            }
        }
    }


    #endregion

    #region MODIFY_GUILD

    public void OnButtonModifyClick()
    {
        Debug.Log("Modification CLICKED");
        UILogic.Instance.showGuildModificationPanel(true, this);

    }

    public void OnModifyGuildConfirmClick() {

        Debug.Log("SUBMITTING GUILD CHANGE");

        string name="Invalid";
        string tag="-1";
        Color color=Color.gray;
        byte[] image_byte = new byte[25];

        try
        {
            name = UILogic.Instance.GuildModificationName.text;
        }
        catch (Exception e) { }

        try
        {
            tag = UILogic.Instance.GuildModificationTag.text;
        }
        catch (Exception e) { }
        /*
        try
        {
            name = UILogic.Instance.GuildModificationColor.text;
        }
        catch (Exception e) { }*/


        UILogic.Instance.showGuildModificationPanel(false, this);


        networkObject.SendRpc(RPC_CREATE_OR_MODIFY_GUILD_REQUEST, Receivers.Server, name, tag, color, image_byte);
    }

    /// <summary>
    /// updejta ali ustvari guild. po operaciji poslje updejte vsem objektom po networki, ki rabjo updejt stanja.(vsi playerji za ime recimo)
    /// </summary>
    /// <param name="args"></param>
    public override void CreateOrModifyGuildRequest(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            uint requester = args.Info.SendingPlayer.NetworkId;

            string name = args.GetNext<string>();
            string tag = args.GetNext<string>();
            string c = args.GetNext<string>();
            byte[] image = args.GetNext<byte[]>();


            Guild g = null;
            if ((g = getGuildFromNetworkId(requester)) != null)
            {
                if (g.guildMaster == requester)
                {
                    //update
                    g.name = name;
                    g.tag = tag;
                    g.color = c;
                    g.image = image;
                }
            }
            else
            {
                //create guild
                g = CreateGuild(args.Info.SendingPlayer.NetworkId, FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerStats>().name + "'s Guild", "", "#ff0000", null);
            }

            sendGuildModifiedResponse(g);
        }
        else
            return;
    }

    private void sendGuildModifiedResponse(Guild g)
    {
        if (networkObject.IsServer)
        {
            foreach (uint member in g.members)
            {
                //networkObject.SendRpc(sendingPlayer, RPC_GUILD_MODIFICATION_RESPONSE, asker, g.name, g.tag, g.color, g.image);

                //zdj mormo vse updejtat pa sinhronizirat kar je povezan z tem guildom. zaenkrat dodamo tag zravn imena playerja.
                GameObject pl = FindByid(member);
                if (pl != null)
                {
                    NetworkPlayerStats ms = pl.GetComponent<NetworkPlayerStats>();
                    if (ms != null)
                        ms.SetGuildUpdated(g.tag);
                    else
                    {
                        Debug.LogError("MS IS NULL. COULDNT UPDATE OR CREATE GUILD!");
                        StartCoroutine(sendGuildModifiedResponsePending(member, g));//coroutine se zazene ker ucas nemormo poslat playerju ker se player se kreira medtem ko ga na networku ze sprejme. probably not the best :)
                    }
                }
                else
                {
                    Debug.LogError("PL IS NULL. COULDNT UPDATE OR CREATE GUILD!");
                    StartCoroutine(sendGuildModifiedResponsePending(member, g));
                }
            }
        }

    }

    IEnumerator sendGuildModifiedResponsePending(uint member, Guild g)
    {
        int i = 0;
        if (networkObject.IsServer)
        {
            bool sent = false;
            while (!sent && (i++ < 60))//caka max 4 minute
            {
                GameObject pl = FindByid(member);
                if (pl != null)
                {
                    NetworkPlayerStats ms = pl.GetComponent<NetworkPlayerStats>();
                    if (ms != null)
                    {
                        ms.SetGuildUpdated(g.tag);
                        sent = true;
                    }
                    else
                        Debug.LogError("MS IS NULL. COULDNT UPDATE OR CREATE GUILD!");
                }
                yield return new WaitForSeconds(2);
            }
        }

    }


    #endregion

    #region HELPER_METHODS
    private Color IfValidColorReturnColor(string text, Color color)
    {
        //ce se text prevede v ksno barvo. hex recimo kva jz vem nekej tazga ker ni color pickerja u unity breu da bi importov asset...
        return color;
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        //Debug.Log("interactable.findplayerById");
        //Debug.Log(targetNetworkId);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
            {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
             //    Debug.Log(p.GetComponent<NetworkPlayerStats>().server_id);
                if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
            }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }

    private void AddToValidInvites(uint gm, uint pleb) {
        uint[] inv = new uint[2];
        inv[0] = gm;
        inv[1] = pleb;
        this.valid_invites.Add(inv);
    }

    private bool ConsumeValidInvite(uint gm, uint pleb) {

        foreach (uint[] u in this.valid_invites) {
            if (u[0] == gm && u[1] == pleb) {
                this.valid_invites.Remove(u);
                return true;
            }
        }
        return false;

    }

    #endregion

    #region MEMBER_PANEL

    internal void toggleMemberPanel()
    {

        bool next = !UILogic.PanelGuildHander.gameObject.activeSelf;
        UILogic.PanelGuildHander.Clear();//pobrise prejsne memberje
        UILogic.PanelGuildHander.gameObject.SetActive(!UILogic.PanelGuildHander.gameObject.activeSelf);
        /*if (next)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            localGetMembersRequest();
        }
        else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }*/
    }

    internal void SetMemberPanel(bool v)
    {
        if (!v)
        {
            UILogic.PanelGuildHander.Clear();//pobrise prejsne memberje
        }
        else {
            UILogic.PanelGuildHander.Clear();//pobrise prejsne memberje
            localGetMembersRequest();
        }
    }

    private void localGetMembersRequest()
    {
        networkObject.SendRpc(RPC_GET_GUILD_MEMBERS_REQUEST, Receivers.Server);
    }

    public override void GetGuildMembersRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            Guild g = getGuildFromNetworkId(args.Info.SendingPlayer.NetworkId);
            if(g!=null)
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_GUILD_MEMBERS_UPDATE,g.guildMaster, g.GetMembersToString(), g.name);
        }
    }

    #endregion

    #region GUILD_RELATED_METHODS

    public Guild CreateGuild(uint gm, string name, string tag, string c, byte[] image)
    {
        Guild g = new Guild(gm, name, tag, c, image);
        if (this.guilds == null) this.guilds = new List<Guild>();
        this.guilds.Add(g);
        return g;
    }

    public Guild getGuildFromNetworkId(uint p) {
        if (this.guilds != null)
            if(this.guilds.Count>0)
                foreach (Guild g in this.guilds) {
                    if (g.isMember(p))
                        return g;
                }
        return null;
    }

    private void DestroyGuild(Guild g2)
    {
        this.guilds.Remove(g2);
    }

    /// <summary>
    /// "uint,uint,uint" -> [uint,uint,uint]
    /// </summary>
    /// <param name="members_string"></param>
    /// <returns></returns>
    private uint[] GetMembersFromRPCString(string members_string)
    {
        string[] s = members_string.Split(',');
        uint[] r = new uint[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            r[i] = UInt32.Parse(s[i]);
        }
        return r;
    }

    [Serializable]
    public class Guild
    {
        public int id;
        public string name;
        public string tag;
        public string color;
        public byte[] image;
        public uint guildMaster;
        public List<uint> members;


        public Guild(uint gm, string v1, string v2, string c, byte[] p)
        {
            this.guildMaster = gm;
            this.members = new List<uint>();
            this.members.Add(gm);

            this.name = v1;
            this.tag = v2;
            this.color = c;
            this.image = p;

            //id??
        }

        /// <summary>
        /// baje da vrne csv
        /// </summary>
        /// <returns></returns>
        public string GetMembersToString()
        {
            return String.Join(",", Array.ConvertAll(this.members.ToArray(), element => element.ToString()));
        }


        public void addMember(uint i)
        {
            if (!isMember(i))
                members.Add(i);
        }
        public void removeMember(uint i)//lahko odstranis gm-ja ampak ta metoda se klice samo ce je vec kot 1 player u guildu. treba chekirat prej
        {
            members.Remove(i);
        }

        public bool isMember(uint i)
        {
            foreach (uint j in members) if (j == i)
                    return true;
            return false;
        }
    }

    #endregion

    #region INVITE

    /// <summary>
    /// klice gm, posle serverju da hoce invitat plebejca u guild
    /// </summary>
    /// <param name="plebejec"></param>
    internal void localSendGuildInvite(uint plebejec)
    {
        networkObject.SendRpc(RPC_REQUEST_SEND_GUILD_INVITE, Receivers.Server, plebejec);
    }

    /// <summary>
    /// dobi server od gm-ja, parameter je id playerja k ga gm invita. ce je vse legit se poslje povabilo plebejcu
    /// </summary>
    /// <param name="args"></param>
    public override void RequestSendGuildInvite(RpcArgs args)
    {
        if (networkObject.IsServer) {
            uint pleb = args.GetNext<uint>();
            uint gm = args.Info.SendingPlayer.NetworkId;
            Guild g = getGuildFromNetworkId(gm);
            Guild g2 = getGuildFromNetworkId(pleb);
            if (g != null && (g2==null || g2.members.Count==1)) {//samo guild master lahko invita in samo ce player ni ze u guildu lahko invita
                if (g.guildMaster == gm) {//lahko poslje invite naprej
                    AddToValidInvites(gm, pleb);
                    networkObject.SendRpc(NetworkManager.Instance.Networker.GetPlayerById(pleb), RPC_SEND_GUILD_INVITE_TO_CANDIDATE, gm, g.name);
                }
            }
        }
    }

    /// <summary>
    /// dobi kandidat od serverja. odpre se mu panela, kjer pise kdo ga je invitov kam, lahko sprejme ali zavrne invite.
    /// </summary>
    /// <param name="args"></param>
    public override void SendGuildInviteToCandidate(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            uint gm = args.GetNext<uint>();
            string guild_name = args.GetNext<string>();

            //narisi panelo enako kot za team invite


            UILogic.DecisionsHandler.draw_guild_invite_decision(FindByid(gm).GetComponent<NetworkPlayerStats>().playerName, guild_name, gm, this);

        }
    }
    /// <summary>
    /// sprozi se na plebejcu z skripte panel_guild_invite_handler, ko klikne na gumb ali ko potece timer. poslje response serverju
    /// </summary>
    /// <param name="gm"></param>
    /// <param name="v"></param>
    internal void LocalGuildRequestResponse(uint gm, bool v)
    {
        
        networkObject.SendRpc(RPC_GUILD_INVITE_RESPONSE, Receivers.Server, gm, v);
    }

    /// <summary>
    /// dobi server od plebejca, ce je sprejel in ce je vse legit se ga doda u guild in poslje updejt guilda vsem playerjem
    /// </summary>
    /// <param name="args"></param>
    public override void GuildInviteResponse(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            uint from = args.Info.SendingPlayer.NetworkId;
            uint gm = args.GetNext<uint>();
            bool status = args.GetNext<bool>();
            if (isGuildInvitationResponseValid(gm, from))
            {
                Guild g = getGuildFromNetworkId(gm);
                if (g != null)
                {
                    //naenkrat treba zbrisat in dodat v tem zaporedju, sicer zbrise guild v katerga ga je ubistvu hotu dodat lol
                    Guild g2 = getGuildFromNetworkId(from);
                    DestroyGuild(g2);

                    g.addMember(from);
                    sendGuildModifiedResponse(g);


                }

            }
            else {
                //invalid join response.
            }
        }

    }

    /// <summary>
    /// SECURITY FEATURE - preverit mora ce je valid, magari da se shrani ko gm pposlje invite, da se caka response od dolocenga playerja za tega gm-ja. sicer lahko kr en retard poslje response in ga vrze u guild lol
    /// </summary>
    /// <returns></returns>
    private bool isGuildInvitationResponseValid(uint gm, uint pleb)
    {
        return ConsumeValidInvite(gm, pleb);
    }

    public override void GuildMembersUpdate(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            //if panel open



            uint gm = args.GetNext<uint>();
            string members_string = args.GetNext<string>();
            string gName = args.GetNext<string>();
            UILogic.PanelGuildHander.GuildNameText.text = gName;

            uint[] members = GetMembersFromRPCString(members_string);

            UILogic.PanelGuildHander.initGm(gm, FindByid(gm).GetComponent<NetworkPlayerStats>().playerName, NetworkManager.Instance.Networker.Me.NetworkId == gm);
            foreach (uint ui in members)
            {
                if (ui != gm)
                {
                    UILogic.PanelGuildHander.init(ui, FindByid(ui).GetComponent<NetworkPlayerStats>().playerName, NetworkManager.Instance.Networker.Me.NetworkId == gm);
                }
            }
        }

    }

    internal void RefreshPlayersNetworkId(uint previousNetworkId, uint v)
    {
        Guild g = getGuildFromNetworkId(previousNetworkId);
        for (int i = 0; i < g.members.Count; i++){ 
            if (g.members[i] == previousNetworkId) g.members[i] = v;
            break;
        }

        if (g.guildMaster == previousNetworkId) g.guildMaster = v;

        sendGuildModifiedResponse(g);
    }

    public void localPlayerGuildInfoUpdateRequest() {
        networkObject.SendRpc(RPC_USER_INFO_REQUEST, Receivers.Server);
    }

    public override void UserInfoRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            //vrnt mora podatke userju v ktermu guildu je pa take fore
            Guild g = getGuildFromNetworkId(args.Info.SendingPlayer.NetworkId);
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_USER_INFO_RESPONSE, g.ObjectToByteArray());
        }
    }

    internal void sendUserInfoResponseTo(NetworkingPlayer  p) {
        if (networkObject.IsServer)
        {
            //vrnt mora podatke userju v ktermu guildu je pa take fore
            Guild g = getGuildFromNetworkId(p.NetworkId);
            networkObject.SendRpc(p, RPC_USER_INFO_RESPONSE, g.ObjectToByteArray());
        }
    }

    internal void sendUserInfoResponseTo(uint p)
    {
        if (networkObject.IsServer)
        {
            lock (NetworkManager.Instance.Networker.Players)
            {
                NetworkManager.Instance.Networker.IteratePlayers((player) =>
                {
                    if (player.NetworkId == p) //passive target
                    {
                        //vrnt mora podatke userju v ktermu guildu je pa take fore
                        Guild g = getGuildFromNetworkId(p);
                        networkObject.SendRpc(player, RPC_USER_INFO_RESPONSE, g.ObjectToByteArray());
                    }
                });
            }
        }
    }

    public override void UserInfoResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.currentGuild = args.GetNext<byte[]>().ByteArrayToObject<Guild>();
        }
    }

    #endregion

}
