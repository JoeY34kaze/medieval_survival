using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decicions_handler_ui : MonoBehaviour
{
    public GameObject team_invite_panel;
    public bool test =false;

    private void Update()
    {
        if (test) {
            test = false;
            draw_team_invite_decision2();
        }
    }
    internal void draw_team_invite_decision2()
    {

        GameObject g = GameObject.Instantiate(team_invite_panel);
        g.transform.SetParent(transform);
        g.GetComponent<panel_team_invite_handler>().init2();
        //transform.GetComponentInParent<CanvasGroup>().blocksRaycasts = true;

    }



    internal void draw_team_invite_decision(GameObject other_gameobject)
    {

        GameObject g =GameObject.Instantiate(team_invite_panel);
        g.transform.SetParent(transform);
        g.GetComponent<panel_team_invite_handler>().init(other_gameobject);
    }
}
