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
public class NetworkContainer_items : NetworkContainerBehavior
{
    private int size;
    public Item[] items;//samo id ni zadost ker v prihodnosti bo treba hrant tud kolicino in ali durability itemov.

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
        this.items = new Item[this.size];
        for (int i = 0; i < this.size; i++)
            this.items[i] = null;
    }

    public Item getItem(int index) {
        //if (networkObject.IsServer || networkObject.IsOwner)
            return this.items[index];
        //return null;
    }

    public void setItem(int index, Item i) {
        if (networkObject.IsServer) {
            if (getItem(index) != null)
                Debug.Log("Overwriting an item in NetworkContainer with another item. I hope you know what you are doing.");
            this.items[index] = i;
        }
    }
    public Item popItem(int index) {
        if (networkObject.IsServer)
        {
            Item i = getItem(index);
            this.items[index] = null;
            return i;
        }
        throw new NotImplementedException();//ce ni server se tole nemore sprozit.
    }

    public int getNumberofItems(int id) {
        if (!networkObject.IsServer) return -1;
        int c = 0;
        foreach (Item i in this.items)
            if(i!=null)
                if (i.id == id)
                    c++;
        return c;
    }

    public Item popItemFirst(int id) {
        if (!networkObject.IsServer) return null;
        Item k = null;
        for (int i = 0; i < this.size; i++)
        {
            if (this.items[i] != null)
                if (this.items[i].id == id)
                    return popItem(i);
        }
        return null;
    }

    public int getEmptySpace() {
        if (!networkObject.IsServer) return -1;
        int c = 0;
        foreach (Item i in this.items)
            if (i == null)
                c++;
        return c;
    }

    public bool contains(Item i, int amount) {
        throw new NotImplementedException();
    }
    public bool contains(int id, int amount)
    {
        if (!networkObject.IsServer) return false;
        int number_of_items = getNumberofItems(id);
        if (number_of_items >= amount) return true;
        return false;

    }

    public void setAll(Item[] all) {//tle se nekje zabugga in jih posle 21!
        this.items = all;
    }

    public Item[] getAll()
    {
        return this.items;
    }


    /// <summary>
    /// zapise nekak v nek network format da pol plunes u rpc. magar csv al pa nekej
    /// </summary>
    /// <returns></returns>
    internal string getItemsNetwork()
    {
        string s = "";
        for (int i = 0; i < this.size; i++) {
            if (this.items[i] != null)
                s = s + "|" + this.items[i].id;
            else
                s = s + "|-1";
        }
        Debug.Log(s);
        return s;
    }

    internal Item[] parseItemsNetworkFormat(string s) {//implementacija te metode je garbage ker bo itak zamenjan ksnej z kšnmu serialized byte array al pa kej namest stringa. optimizacija ksnej
        string[] ss = s.Split('|');
        Item[] rez = new Item[ss.Length -1];//zacne se z "" zato en slot sfali
        for (int i = 1; i < ss.Length; i++) {//zacne z 1 ker je ss[0] = ""
            int k=-1;
             Int32.TryParse(ss[i],out k);
            rez[i] = Mapper.instance.getItemById(k);
        }
        return rez;
    }

    public void putFirst(Item item, int q) {
        if (!networkObject.IsServer) return;
        for (int i = 0; i < this.size; i++) {
            if (this.items[i] == null)
            {
                setItem(i, item);
                return;
            }
        }
    }

}
