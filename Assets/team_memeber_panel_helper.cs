using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class team_memeber_panel_helper : MonoBehaviour
{
    public uint id_player;
    public Image hp;

    internal void init(uint id)
    {
        this.id_player = id;
    }

    public void changeHp(float newHp) {
        hp.fillAmount = newHp;
    }
}
