using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;

public class NetworkWorldManager : NetworkWorldManagerBehavior
{
    public int resourceRefreshTime;
    protected override void NetworkStart()
    {
        base.NetworkStart();
        
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();
            StartCoroutine(serverResourceRefresh(resourceRefreshTime));
        }


        
    }

    public IEnumerator serverResourceRefresh(int seconds)
    {
        while (true) {
            yield return new WaitForSecondsRealtime(seconds);
            sendResourceRefresh();
        }
    }

    private void sendResourceRefresh()
    {
        if (networkObject.IsServer)
            networkObject.SendRpc(RPC_RESOURCE_REFRESH, Receivers.All);
    }

    public override void resourceRefresh(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            foreach (Transform child in transform) {
                if (!child.gameObject.activeSelf) {
                    child.gameObject.SetActive(true);
                    child.GetComponent<NetworkResource>().onRefresh();
                }
            }
        }
    }
}
