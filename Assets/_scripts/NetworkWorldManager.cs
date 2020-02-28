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

    private IEnumerator upkeep_checker;


    private void Update()
    {
        if (networkObject.IsServer)//bi dau u start ampak je izjemno pomembna metoda in bi blo treba chekirat skos če dela l pa ne. bolš bi blo da se na vsake par minut chekira ampak update je ured pomoje ker je check cist dzabe
            if (this.upkeep_checker == null)
            {
                this.upkeep_checker = upkeep_checker_coroutine();
                StartCoroutine(this.upkeep_checker);
            }
    }

    private IEnumerator upkeep_checker_coroutine()
    {
        while (true)
        {
            foreach (NetworkPlaceable p in GameObject.FindObjectsOfType<NetworkPlaceable>())
            {
                if (p.p.item.needs_upkeep)
                    p.on_upkeep_pay();
            }
            yield return new WaitForSecondsRealtime(30);
        }
    }







    protected override void NetworkStart()
    {
        base.NetworkStart();
        
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();
            StartCoroutine(serverResourceRefresh(resourceRefreshTime));

            this.upkeep_checker = upkeep_checker_coroutine();
            StartCoroutine(this.upkeep_checker);
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
                    child.GetComponent<NetworkResource>().onRefresh();
            }
        }
    }
}
