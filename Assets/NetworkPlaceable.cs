using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

public class NetworkPlaceable : NetworkPlaceableBehavior
{

    public void init()
    {
        //server mu nastavi vse stvari k jih rab nastavt ob instanciaciji objekta.

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
