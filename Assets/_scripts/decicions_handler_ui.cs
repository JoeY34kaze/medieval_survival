using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decicions_handler_ui : MonoBehaviour
{
    public GameObject team_invite_panel;
    public GameObject guild_invite_panel;



    internal void draw_team_invite_decision(GameObject other_gameobject)
    {

        GameObject g =GameObject.Instantiate(team_invite_panel);
        g.transform.SetParent(transform);
        g.GetComponent<panel_team_invite_handler>().init(other_gameobject);
    }

    internal void draw_guild_invite_decision(string gm_name, string guild_name, uint other, NetworkGuildManager ngm)
    {

        GameObject g = GameObject.Instantiate(guild_invite_panel);
        g.transform.SetParent(transform);
        g.GetComponent<panel_guild_invite_handler>().init(gm_name,guild_name,other,ngm);
    }
}
