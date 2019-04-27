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
    public static Mapper instance;

    private void Awake()
    {
        instance = this;
    }


    public Item[] items;//0,1 so pesti pa unarmed block tko da rabmo za 2 znizat oziroma pristet ko iscemo id

    public GameObject[] equippable_weapons;
    public string[] passthrough_tags;

    public Item getItemById(int id) {
        if (id - 2 < items.Length)
        {
            return items[id - 2];
        }
        else {
            Debug.LogError("Accessing item that doesnt exist ina Array!");
            return null;
        }
    }

    public int getIdFromItem(Item i) {
        for (int j= 0; j < items.Length; j++) {
            if (items[j].Equals(i)) return j;
        }
        Debug.LogError("No id for this item!");
        return -1;
    }


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
