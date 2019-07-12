using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;

/// <summary>
/// instanca sinhronizerja ki se nalima na playerja
/// </summary>
public class NetworkStartupSynchronizerPlayer : NetworkStartupSynchronizer
{
    NetworkPlayerStats stats;
    NetworkPlayerAnimationLogic anim;
    NetworkPlayerCombatHandler combat;
    NetworkPlayerInventory npi;
    private void Start()
    {
        updateScripts();
    }

    private void updateScripts()
    {
        this.stats = GetComponent<NetworkPlayerStats>();
        this.anim = GetComponent<NetworkPlayerAnimationLogic>();
        this.combat = GetComponent<NetworkPlayerCombatHandler>();
        this.npi = GetComponent<NetworkPlayerInventory>();
    }

    public override void SendDataToStartingClient(NetworkingPlayer p)
    {

        if (this.stats == null || this.anim == null || this.combat == null || this.npi == null)
            updateScripts();

        

        Debug.Log("Sending data from NetworkStartupSynchronizerPlayer");

        stats.ServerSendAll(p);//za stats

        //uma dna

        //movement ?

        //animation logic ? - mrde bi kr vsa stanja iz animatorja poslov cez pa je
        anim.ServerSendAll(p);//mislm d je

        //combatHandler
        combat.ServerSendAll(p);//mislm d je
                                                                  //inventory
        npi.ServerSendAll(p);//mislm d je

    }
}
