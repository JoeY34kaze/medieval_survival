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
                if (i.item.id == id)
                    c+=i.quantity;
        return c;
    }

    public Predmet popPredmetFirst(int id) {
        if (!networkObject.IsServer) return null;
        Predmet k = null;
        for (int i = 0; i < this.size; i++)
        {
            if (this.predmeti[i] != null)
                if (this.predmeti[i].item.id == id)
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

    public bool containsAmount(Item i, int amount) {
        int q = 0;
        foreach (Predmet p in this.predmeti)
            if (p.item.id == i.id) q += p.quantity;
        return (q >= amount) ? true : false;
    }

    public bool contains(int id, int amount)
    {
        if (!networkObject.IsServer) return false;
        int number_of_items = getNumberofItems(id);
        if (number_of_items >= amount) return true;
        return false;

    }

    public void setAll(Predmet[] all) {//tle se nekje zabugga in jih posle 21!
        if (this.predmeti.Length == all.Length || !networkObject.IsServer)
            this.predmeti = all;
        else
            Debug.LogError("Trying to set array when sizes mismatch..");
    }

    public Predmet[] getAll()
    {
        return this.predmeti;
    }


    /// <summary>
    /// zapise nekak v nek network format da pol plunes u rpc. magar csv al pa nekej
    /// </summary>
    /// <returns></returns>
    internal string getItemsNetwork()
    {
        string s = "";
        for (int i = 0; i < this.size; i++) {
            if (this.predmeti[i] != null)
                s = s + "|" + this.predmeti[i].toNetworkString();
            else
                s = s + "|-1";
        }
        Debug.Log(s);
        return s;
    }

    internal Predmet[] parseItemsNetworkFormat(string s) {//implementacija te metode je garbage ker bo itak zamenjan ksnej z kšnmu serialized byte array al pa kej namest stringa. optimizacija ksnej
        string[] ss = s.Split('|');
        Predmet[] rez = new Predmet[ss.Length -1];//zacne se z "" zato en slot sfali
        for (int i = 1; i < ss.Length; i++) {//zacne z 1 ker je ss[0] = ""
            rez[i - 1] = Predmet.createNewPredmet(ss[i]);//ce je format networkstringa ured vrne predmet sicer vrne null
        }
        return rez;
    }

    public void putFirst(Predmet predmet) {
        if (!networkObject.IsServer) return;
        for (int i = 0; i < this.size; i++) {
            if (this.predmeti[i] == null)
            {
                setPredmet(i, predmet);
                return;
            }
        }
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
