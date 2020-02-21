using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkContainer_craftingStation : NetworkContainer
{
    protected NetworkContainer_items nci;

    internal void init(int capacity)
    {
        this.nci = GetComponent<NetworkContainer_items>();
        this.nci.init(capacity);
    }

    internal string getItemsNetwork()
    {
        return this.nci.getItemsNetwork();
    }

    internal Predmet[] parseItemsNetworkFormat(string v)
    {
        return this.nci.parseItemsNetworkFormat(v);
    }
}
