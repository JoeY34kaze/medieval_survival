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


    public Item getItemById(int id) {
        foreach (Item i in items)
            if (i.id == id)
                return i;

        throw new Exception("Item not found by id!");
    }

    public int getIdFromItem(Item i) {
        for (int j= 0; j < items.Length; j++) {
            if (items[j].Equals(i)) return j;
        }
        throw new Exception("Id not found from item!");
    }


    internal int getWeaponIdFromName(string name)
    {
        foreach (Item i in items)
            //Debug.Log(equippable_weapons[i].name + "(Clone)" + name + " " + (equippable_weapons[i].name + "(Clone)").Equals(name));
            if (i.name.Equals(name) || (i.name  +"(Clone)").Equals(name))
                return i.id;

        throw new Exception("Id not found from requested weapon name!");
    }

}
