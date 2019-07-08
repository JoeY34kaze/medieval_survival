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

    public GameObject localPlayer;
    public Text name_guild;
    public Text tag_guild;
    public Text color_guild;

    public List<Guild> guilds;

    protected override void NetworkStart()
    {
        base.NetworkStart();

        if (networkObject.IsServer) {
            //load guilds from database or something...
            //if null
            if(this.guilds==null)this.guilds = new List<Guild>();
            networkObject.TakeOwnership();
        }
        localPlayer = FindByid(NetworkManager.Instance.Networker.Me.NetworkId);//?? i guess it should work. mrde bols da player pogleda pa poveze z druge strani..
        
        //networkObject.SendRpc(RPC_REQUEST_GUILD_AFFILIATION, Receivers.Server, NetworkManager.Instance.Networker.Me.NetworkId); tega nebo tle. ko se connecta server pogleda ce ze obstaa njemu pripadajoc objekt in ga poupdejta, sicer nrdit nov guild.
    }


    public void OnButtonModifyClick() {
        Debug.Log("Modification CLICKED");
        if(localPlayer==null) localPlayer = FindByid(NetworkManager.Instance.Networker.Me.NetworkId);
        localPlayer.GetComponent<NetworkPlayerStats>().showGuildModificationPanel(true, this);

    }

   /* private void Update()
    {
        
        Debug.Log(NetworkManager.Instance.Networker.Me.NetworkId);
    }*/


    public void OnModifyGuildConfirmClick() {

        Debug.Log("SUBMITTING GUILD CHANGE");

        string name="RETARDOS";
        string tag="RET";
        Color color=Color.blue;
        byte[] image_byte = new byte[25];
        ///image nekak dobit
        ///
        /*
        if (this.name_guild != null)
            if (this.name_guild.text != "")
                name = this.name_guild.text;
        if (this.tag_guild != null)
            if (this.tag_guild.text != "")
                name = this.tag_guild.text;
        if (this.color_guild != null)
            if (this.color_guild.text != "")
                color = IfValidColorReturnColor(this.color_guild.text, color);

        //nekej nrdit za image..

        */

        localPlayer.GetComponent<NetworkPlayerStats>().showGuildModificationPanel(false, this);


        networkObject.SendRpc(RPC_CREATE_OR_MODIFY_GUILD_REQUEST, Receivers.Server, name, tag, color, image_byte);
    }

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
            Color c = args.GetNext<Color>();
            byte[] image = args.GetNext<byte[]>();


            Guild g = null;
            if ((g = findPlayersGuild(requester)) != null)
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
            else {
                //create guild
                g=CreateGuild(args.Info.SendingPlayer.NetworkId, FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerStats>().name + "'s Guild", "", Color.gray, null);
            }

            sendGuildModifiedResponse(g);
        }
        else
            return;
    }

    private void sendGuildModifiedResponse(Guild g)
    {
        if (networkObject.IsServer) {
            foreach (uint member in g.members)
            {
                //networkObject.SendRpc(sendingPlayer, RPC_GUILD_MODIFICATION_RESPONSE, asker, g.name, g.tag, g.color, g.image);

                //zdj mormo vse updejtat pa sinhronizirat kar je povezan z tem guildom. zaenkrat dodamo tag zravn imena playerja.
                GameObject pl = FindByid(member);
                if (pl != null)
                {
                    NetworkPlayerStats ms = pl.GetComponent<NetworkPlayerStats>();
                    if (ms != null)
                        ms.SendGuildUpdate(g.name, g.tag, g.color, g.image);
                    else
                    {
                        Debug.LogError("MS IS NULL. COULDNT UPDATE OR CREATE GUILD!");
                        StartCoroutine(sendGuildModifiedResponsePending(member, g));
                    }
                }
                else {
                    Debug.LogError("PL IS NULL. COULDNT UPDATE OR CREATE GUILD!");
                    StartCoroutine(sendGuildModifiedResponsePending(member, g));
                }
            }
        }
            
    }

    IEnumerator sendGuildModifiedResponsePending(uint member, Guild g)
    {
        if (networkObject.IsServer)
        {
            bool sent = false;
            while (!sent)
            {
                GameObject pl = FindByid(member);
                if (pl != null)
                {
                    NetworkPlayerStats ms = pl.GetComponent<NetworkPlayerStats>();
                    if (ms != null)
                    {
                        ms.SendGuildUpdate(g.name, g.tag, g.color, g.image);
                        sent = true;
                    }
                    else
                        Debug.LogError("MS IS NULL. COULDNT UPDATE OR CREATE GUILD!");
                }
                yield return new WaitForSeconds(2);
            }
        }
        
    }

    /*
    /// <summary>
    /// odgovor od requesta. prvi parameter je server_id od playerja k je poslov request da vemo komu odgovort
    /// </summary>
    /// <param name="args"></param>
    public override void GuildModificationResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            uint asker = args.GetNext<uint>();
            string name = args.GetNext<string>();
            string tag = args.GetNext<string>();
            Color color = args.GetNext<Color>();
            byte[] image = args.GetNext<byte[]>();

            Debug.Log("GuildAffiliationResponse: " + asker + " belongs to " + name+"also i have no idea what to do with this method since update is sent to NetworkPlayerStats on all players from server directly...");

            //localPlayer.GetComponent<NetworkPlayerStats>().updateGuild(name,tag,color,image); - to ze server pohendla
        }
    }*/

    /// <summary>
    /// playerjev objekt poslje poizvedbo ktermu guildu pripada. ne rabi bit owner.
    /// </summary>
    /// <param name="args"></param>
    public override void RequestGuildAffiliation(RpcArgs args)
    {
        if (networkObject.IsServer) {
            //tole lahko requesta sender tud za kak drug item, katerga NetworkId je razlicen od sendarje. zato mormo senderju vrnt nazaj tud networkId katerga je poslal zravn
            uint id_req = args.GetNext<uint>();

            Guild g = findPlayersGuild(id_req);

            if (g == null)
            {
                //create new guild
                Guild k = CreateGuild(args.Info.SendingPlayer.NetworkId, args.Info.SendingPlayer.NetworkId + "'s Starter Guild", "", Color.gray, null);
                g = k;
            }
            sendGuildModifiedResponse(g);
        }
    }


    private Guild CreateGuild(uint gm, string name, string tag, Color c, byte[] image)
    {
        Guild g = new Guild(gm, name, tag, c, image);
        if (this.guilds == null) this.guilds = new List<Guild>();
        this.guilds.Add(g);
        return g;
    }

    private Guild findPlayersGuild(uint p) {
        if (this.guilds != null)
            if(this.guilds.Count>0)
                foreach (Guild g in this.guilds) {
                    if (g.isMember(p))
                        return g;
                }
        return null;
    }

    public class Guild {
        public int id;
        public string name;
        public string tag;
        public Color color;
        public byte[] image;
        public uint guildMaster;
        public List<uint> members;


        public Guild(uint gm, string v1, string v2, Color gray, byte[] p)
        {
            this.guildMaster = gm;
            this.members = new List<uint>();
            this.members.Add(gm);

            this.name = v1;
            this.tag = v2;
            this.color = gray;
            this.image = p;

            //id??
        }

        public void updateInfo(string name, string tag, Color c, byte[] image) {
            this.name = name;
            this.tag = tag;
            this.color = c;
            this.image = image;
        }

        public void addMember(uint i) {
            if (!isMember(i))
                members.Add(i);
        }
        public void removeMember(uint i) {
            if (i == guildMaster) return;//cannot remove gm
            members.Remove(i);
        }

        public bool isMember(uint i)
        {
            foreach (uint j in members) if (j == i)
                    return true;
            return false;
        }
    }
}
