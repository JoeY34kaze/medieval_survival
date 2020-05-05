using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Osnova predmeta v igri. nesmemo uporablat Itema vec, ampak uporablamo ta objekt
/// </summary>
[System.Serializable] public class Predmet 
{
    public int item_id;
    public int quantity=1;
    //public string creator;
    public float current_durabilty;
    public string creator = "Server";
    public int tier = 0;//mislm da je samo za pšosiljanje null vrednosti. dejanski tier je u itemu


    public Predmet(Predmet ppp)
    {
        if (ppp != null)
        {
            this.item_id = ppp.item_id;
            this.quantity = ppp.quantity;
            this.current_durabilty = ppp.current_durabilty;
            this.creator = ppp.creator;
        }
    }
    public Predmet(Item i) {
        this.item_id = i.id;
        this.quantity = 1;
        this.current_durabilty = i.Max_durability;
    }

    public Predmet(Item i, int quantity) {
        this.item_id = i.id;
        this.quantity = quantity;
        this.current_durabilty = i.Max_durability;
    }

    public Predmet(Item i, int quantity, float durability) {
        this.item_id = i.id;
        this.quantity = quantity;
        this.current_durabilty = durability;
    }

    public Predmet(Item i, int quantity, float durability, string creator)
    {
        this.item_id = i.id;
        this.quantity = quantity;
        this.current_durabilty = durability;
        this.creator = creator;
    }

    public Predmet(int v)
    {
        this.item_id = v;
        this.quantity = 0;
        this.tier = -1;
        this.creator = "if you see this tell developers.";
    }

    internal Predmet addQuantity(Predmet p)
    {
        if (p.item_id!=this.item_id || this.quantity >= this.getItem().stackSize) return p;

        if (this.quantity + p.quantity <= this.getItem().stackSize) { this.quantity += p.quantity; return null; }
        else {
            p.quantity = p.quantity - (this.getItem().stackSize - this.quantity);
            this.quantity = this.getItem().stackSize;
            return p;
        }
    }

    public Item getItem()
    {
        return Mapper.instance.getItemById(this.item_id);
    }

    public float GetWeight() {
        return this.getItem().weight * this.quantity;
    }

}
