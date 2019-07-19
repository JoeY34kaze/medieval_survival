using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Osnova predmeta v igri. nesmemo uporablat Itema vec, ampak uporablamo ta objekt
/// </summary>
public class Predmet 
{
    public Item item;
    public int quantity;
    //public string creator;
    public int durability;
    public string creator = "some schmuk";
    public Predmet(Item i) {
        this.item = i;
    }

    public Predmet(Item i, int quantity) {
        this.item = i;
        this.quantity = quantity;
    }

    public Predmet(Item i, int quantity, int durability) {
        this.item = i;
        this.quantity = quantity;
        this.durability = durability;
    }

    public Predmet(Item i, int quantity, int durability, string creator)
    {
        this.item = i;
        this.quantity = quantity;
        this.durability = durability;
        this.creator = creator;
    }

    /// <summary>
    /// vrne strik k ga fuknes u RPC, predstavlja toString tega predmeta da ga pol stlacs v predmet.SetParametersFromNetworkString v rpcju na drug strani in smo tko prekopiral objekt po networku
    /// </summary>
    /// <returns></returns>
    internal string toNetworkString()
    {
        return this.item.id + "," + this.quantity + "," + this.durability + "," + this.creator;
    }

    /// <summary>
    /// not vrzes predmet.toNetworkString in mas objekt. tole se naceloma uporabla po poslanmu rpcju na receiverju da se prekopira predmet
    /// </summary>
    /// <param name="s"></param>
    public void setParametersFromNetworkString(string s) {
        string[] parametri = s.Split(',');//mogl bi bit size 4

        if (parametri.Length < 2) return;

        this.item = Mapper.instance.getItemById(Int32.Parse(parametri[0]));
        this.quantity = Int32.Parse(parametri[1]);
        this.durability = Int32.Parse(parametri[2]);
        this.creator = parametri[3];
    }

    internal static Predmet createNewPredmet(string networkString)
    {
        Predmet r = new Predmet(null);
        r.setParametersFromNetworkString(networkString);
        if (r.item == null) return null;
        return r;
    }

    internal Predmet addQuantity(Predmet p)
    {
        if (!p.item.Equals(this.item) || this.quantity >= this.item.stackSize) return p;

        if (this.quantity + p.quantity <= this.item.stackSize) { this.quantity += p.quantity; return null; }
        else {
            p.quantity = p.quantity - (this.item.stackSize - this.quantity);
            this.quantity = this.item.stackSize;
            return p;
        }
    }
}
