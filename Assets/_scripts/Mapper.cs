using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapper : MonoBehaviour
{
    //ta array bi blo najbrz pametno da je enak kot v unmu mapperju za instanciacijo na networkManagerju. sj nevem a bi pol kz z networkmanagerja pobirov kej al kva


    /*
     * 0 - unarmed fists
     * 1 - unarmed block
     * 
     */
    public GameObject[] equippable_weapons;
    public string[] passthrough_tags;

    internal int getWeaponIdFromName(string name)
    {
        for (int i = 0; i < equippable_weapons.Length; i++) {
            //Debug.Log(equippable_weapons[i].name + "(Clone)" + name + " " + (equippable_weapons[i].name + "(Clone)").Equals(name));
            if (equippable_weapons[i].name.Equals(name) || (equippable_weapons[i].name  +"(Clone)").Equals(name)) return i;
        }
        return -1;
    }

    internal bool tag_allows_passthrough(string t) {
        foreach (string s in passthrough_tags) {
            if (s.Equals(t)) return true;
        }
        return false;
    }
}
