using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;

public class NetworkStartupSynchronizerArmorStand : NetworkStartupSynchronizer
{
    NetworkArmorStand stand;
    private void Start()
    {
        updateScripts();
    }

    private void updateScripts()
    {
        this.stand = GetComponent<NetworkArmorStand>();
    }

    public override void SendDataToStartingClient(NetworkingPlayer p)
    {
        if (this.stand == null) updateScripts();
        //stand.ServerSendAllToPlayer(p);

    }
}