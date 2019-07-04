using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// hrani bolj kot ne vse kar zahtevajo guildi. samo server ima vse guilde, clienti posiljajo zahteve z objektov ce rabjo vidt kter je njihov guild.
/// </summary>
public class NetworkGuildManager : NetworkGuildManagerBehavior
{
    public GameObject GuildModificationPanel;

    public void OnButtonModifyClick() {
        Debug.Log("CLICKED");
        if (GuildModificationPanel != null)
            GuildModificationPanel.SetActive(true);
    }



    public void OnModifyGuildConfirmClick() {
        string name="";
        string tag="";
        Color color=Color.blue;
        byte[] image_byte = new byte[25];
        ///image nekak dobit

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

        }
        else
            return;
    }

    /// <summary>
    /// odgovor od requesta. prvi parameter je server_id od playerja k je poslov request da vemo komu odgovort
    /// </summary>
    /// <param name="args"></param>
    public override void GuildAffiliationResponse(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// playerjev objekt poslje poizvedbo ktermu guildu pripada. ne rabi bit owner.
    /// </summary>
    /// <param name="args"></param>
    public override void RequestGuildAffiliation(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }

    class Guild {
        int id;
        string name;
        string tag;
        Color color;
        byte[] image;
        uint guildMaster;
        List<uint> members;



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

        private bool isMember(uint i)
        {
            foreach (uint j in members) if (j == i)
                    return true;
            return false;
        }
    }
}
