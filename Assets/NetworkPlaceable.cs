using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

/// <summary>
/// stvari k so skupne vsem stvarem k jih loh postavs. health, lastnistvo, take fore. boljs specificne stvari nrdit svojo skripto i guess
/// </summary>
public class NetworkPlaceable : NetworkPlaceableBehavior
{
    public Predmet p;
    [SerializeField]
    public Item.SnappableType snappableType;

    public void init(Predmet p)
    {
        this.p = p;
        //server mu nastavi vse stvari k jih rab nastavt ob instanciaciji objekta.
        if (GetComponent<NetworkChest>() != null) GetComponent<NetworkChest>().init(this.p);
        //
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();//server prevzame ownership


            /*
         networking sinhronizacija...... kaj se nrdi ob connectu, kko poupdejtamo med clienti pa take fore    
         
         */
        }
    }



    /*
    internal override void setStartingInstantiationParameters(Predmet p, Vector3 pos, Vector3 dir)
    {
        if (networkObject.IsServer)
        {
            setForce(pos, dir);
            this.p = p;
            networkObject.SendRpc(RPC_SET_PREDMET_FOR_PICKUP, Receivers.Others, p.toNetworkString());

        }

    }*/

}
