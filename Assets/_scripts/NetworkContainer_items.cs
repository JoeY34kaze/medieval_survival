using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// superclass k deluje kot modul k ga loh nalimas na druge objekte k rabjo networkStorage. recimo crafting tables, backpack, un stockpile al kva..
/// RPCJEV NEBO MEL, RPCJE SPISAT V RAZREDIH, KI TA CLASS UPORABLAJO
/// 
/// </summary>
public class NetworkContainer_items : NetworkContainerItemsBehavior
{
    private int size;
    public Predmet[] predmeti;//samo id ni zadost ker v prihodnosti bo treba hrant tud kolicino in ali durability itemov.

    internal int Getsize()
    {
        return this.size;
    }

    public void Start()
    {
            //init(20);
    }
    /// <summary>
    /// pohendla da je vse nrjen na zacetku. ce je server mora tle bit logika da prebere iz baze al pa kej
    /// </summary>
    /// <param name="size"></param>
    public void init(int size)
    {
        this.size = size;
        this.predmeti = new Predmet[this.size];
        for (int i = 0; i < this.size; i++)
            this.predmeti[i] = null;
    }

    internal bool Remove(Item item, int q)
    {
        //zarad craftingstationa moramo met logiko da pobere samo del stacka in ne celega.


        for (int i = 0; i < this.predmeti.Length; i++)
        {

            if (this.predmeti[i] != null)
                if (this.predmeti[i].getItem().Equals(item))
                {
                    if (this.predmeti[i].quantity <= q)
                    {
                        //pobral bomo cel stack.
                        q -= this.predmeti[i].quantity;
                        this.predmeti[i] = null;
                    }
                    else
                    {//poberemo samo del stacka in smo zakljucli
                        this.predmeti[i].quantity -= q;
                        return true ;
                    }
                }
        }
        return q == 0;
    }

    public Predmet getPredmet(int index) {
        //if (networkObject.IsServer || networkObject.IsOwner)
            return this.predmeti[index];
        //return null;
    }

    public void setPredmet(int index, Predmet i) {
        if (networkObject.IsServer) {
            if (getPredmet(index) != null)
                Debug.Log("Overwriting an item in NetworkContainer with another item. I hope you know what you are doing.");
            this.predmeti[index] = i;
        }
    }

    internal bool isEmpty()
    {
        foreach (Predmet p in this.predmeti) if (p != null) return false;
        return true;
    }

    public Predmet popPredmet(int index) {
        if (networkObject.IsServer)
        {
            if (this.predmeti[index] == null) return null; else { Predmet r = this.predmeti[index]; this.predmeti[index] = null; return r; }
        }
        throw new NotImplementedException();//ce ni server se tole nemore sprozit.
    }

    //vrne stevilo teh itemov znotraj tega inventorija. pogleda predmet.quantity
    public int getNumberofItems(int id) {
        if (!networkObject.IsServer) return -1;
        int c = 0;
        foreach (Predmet i in this.predmeti)
            if(i!=null)
                if (i.getItem().id == id)
                    c+=i.quantity;
        return c;
    }

    public Predmet popPredmetFirst(int id) {
        if (!networkObject.IsServer) return null;
        Predmet k = null;
        for (int i = 0; i < this.size; i++)
        {
            if (this.predmeti[i] != null)
                if (this.predmeti[i].getItem().id == id)
                    return popPredmet(i);
        }
        return null;
    }

    public int getEmptySpace() {
        if (!networkObject.IsServer) return -1;
        int c = 0;
        foreach (Predmet i in this.predmeti)
            if (i == null)
                c++;
        return c;
    }

    public bool hasSpace() {
        return getEmptySpace() > 0;
    }

    public bool hasSpace(Predmet p) {
        return getEmptySpace() > 0 || CanBePlacedOnExistingStacksInFull(p);
    }

    private bool CanBePlacedOnExistingStacksInFull(Predmet p)
    {
        int kol = p.quantity;
        foreach (Predmet stack in this.predmeti)
            if (stack != null)
                if (stack.getItem() != null)
                    if (stack.getItem().Equals(p.getItem()))
                        if (stack.quantity < stack.getItem().stackSize)
                        {
                            kol -= stack.getItem().stackSize - stack.quantity;
                            if (kol <= 0) return true;
                        }
        return false;
    }

    internal IEnumerable<Predmet> getAllOfType(Item.Type t)
    {
        List<Predmet> p = new List<Predmet>();
        foreach (Predmet pr in this.predmeti)
            if(pr!=null)
                if (pr.getItem().type == t)
                    p.Add(pr);
        return p;
    }

    internal bool containsAmount(Item i, int amount) {
        int q = 0;
        foreach (Predmet p in this.predmeti)
            if(p!=null)
                if (p.getItem().id == i.id) q += p.quantity;
        return (q >= amount) ? true : false;
    }
    /*
    internal bool contains(int id, int amount)
    {
        if (!networkObject.IsServer) return false;
        int number_of_items = getNumberofItems(id);
        if (number_of_items >= amount) return true;
        return false;

    }
    */
    internal void setAll(Predmet[] all) {//tle se nekje zabugga in jih posle 21!
        if (this.predmeti.Length == all.Length || !networkObject.IsServer)
            this.predmeti = all;
        else
            Debug.LogError("Trying to set array when sizes mismatch..");
    }

    internal Predmet[] getAll()
    {
        return this.predmeti;
    }






    internal Predmet putFirst(Predmet predmet) {
        if (!networkObject.IsServer) return predmet;
        for (int i = 0; i < this.size; i++) {
            if (this.predmeti[i] == null)
            {
                setPredmet(i, predmet);
                return null;
            }
        }
        return predmet;
    }

    

    /// <summary>
    /// pogleda ves inventorij in poskusi dodat ze obstojecim stackom tega itema. vrne del itema, ki ga ni mogu dat na stack, vrne celoten item nazaj ce ni nobenga stacka, vrne null ce mu je uspelo vse dat na nek stack.
    /// </summary>
    /// <param name="resp"></param>
    /// <returns></returns>
    internal Predmet tryToAddPredmetToExistingStack(Predmet resp)
    {
        Predmet p = resp;
        foreach (Predmet stack in this.predmeti)
            if (stack != null)
                if (stack.getItem() != null)
                    if (stack.getItem().Equals(p.getItem()))
                        if (stack.quantity < stack.getItem().stackSize)
                        {
                            p = stack.addQuantity(p);
                            if (p == null)
                                return null;
                        }
        return p;
    }

    internal bool Add(Predmet p) {
        p = tryToAddPredmetToExistingStack(p);
        if (p != null)
            this.putFirst(p);
        return p == null;
    }

    internal void swap(int p, int v)
    {
        if(networkObject.IsServer)
            if (p < this.size && v < this.size) {
                Predmet temp = this.predmeti[p];
                this.predmeti[p] = this.predmeti[v];
                this.predmeti[v] = temp;
            }
    }


}
