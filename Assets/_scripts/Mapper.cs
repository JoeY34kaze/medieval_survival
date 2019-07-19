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


    public Item[] items;
    public PredmetRecepie[] recepies;
    #region ITEMS
    public Item getItemById(int id) {
        if (id == -1) return null;

        foreach (Item i in items)
            if (i.id == id)
                return i;

        return null;
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
    #endregion

    #region RECEPIES

    public PredmetRecepie getPredmetRecepieForItem(Item i)
    {
        foreach (PredmetRecepie p in this.recepies)
            if (p.Product.Equals(i)) return p;

        return null;
    }

    public List<PredmetRecepie> getPredmetRecepiesThatAreUsingThisItem(Item i) {
        List<PredmetRecepie> pr = new List<PredmetRecepie>();
        foreach (PredmetRecepie p in this.recepies)
            foreach (Item ingredient in p.ingredients)
                if (ingredient.Equals(i))
                    pr.Add(p);
        return pr;
    }
    #endregion
}
