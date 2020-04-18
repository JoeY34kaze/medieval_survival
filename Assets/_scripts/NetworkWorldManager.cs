using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;

public class NetworkWorldManager : NetworkWorldManagerBehavior
{
    public float resourceRefreshTime;
    public float upkeep_interval;
    private IEnumerator upkeep_checker;


    private void Update()
    {
        if(networkObject!=null)
            if (networkObject.IsServer)//bi dau u start ampak je izjemno pomembna metoda in bi blo treba chekirat skos če dela l pa ne. bolš bi blo da se na vsake par minut chekira ampak update je ured pomoje ker je check cist dzabe
                if (this.upkeep_checker == null)
                {
                    this.upkeep_checker = Upkeep_checker_coroutine();
                    StartCoroutine(this.upkeep_checker);
                }
    }

    private IEnumerator Upkeep_checker_coroutine()
    {
        int i = 0;
        while (true)
        {
            NetworkPlaceable[] list = GameObject.FindObjectsOfType<NetworkPlaceable>();
            //Debug.Log("------------UKEEP ITERATION " + (i++) + "------------------");
            for (int k=0;k<list.Length;k++)
            {
                if(list[k]!=null)
                    if (list[k].p.item.needs_upkeep)
                        list[k].on_upkeep_pay();
            }
           // Debug.Log("------------------------------------------------------");
            yield return new WaitForSecondsRealtime(upkeep_interval);
        }
    }







    protected override void NetworkStart()
    {
        base.NetworkStart();
        
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();
            StartCoroutine(serverResourceRefresh(resourceRefreshTime));

            this.upkeep_checker = Upkeep_checker_coroutine();
            StartCoroutine(this.upkeep_checker);
        }


        
    }

    public IEnumerator serverResourceRefresh(float seconds)
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
        if (args.Info.SendingPlayer.IsHost) {
            foreach (Transform child in transform) {
                    child.GetComponent<NetworkResource>().onRefresh();
            }
        }
    }
}
