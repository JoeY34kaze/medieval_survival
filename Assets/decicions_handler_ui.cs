using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decicions_handler_ui : MonoBehaviour
{
    public GameObject team_invite_panel;




    internal void draw_team_invite_decision(GameObject other_gameobject)
    {

        GameObject g =GameObject.Instantiate(team_invite_panel);
        g.transform.SetParent(transform);
        g.GetComponent<panel_team_invite_handler>().init(other_gameobject);
    }
}
