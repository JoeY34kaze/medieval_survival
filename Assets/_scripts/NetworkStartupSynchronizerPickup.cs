using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;

public class NetworkStartupSynchronizerPickup : NetworkStartupSynchronizer
{

    private void Start()
    {
        updateScripts();
    }

    private void updateScripts()
    {

    }

    public override void SendDataToStartingClient(NetworkingPlayer p)
    {



    }
}
