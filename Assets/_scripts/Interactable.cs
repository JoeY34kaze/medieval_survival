﻿using System;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
/*
Iz brackeys
https://www.youtube.com/watch?v=9tePzyL6dgc&list=PLPV2KyIb3jR4KLGCCAciWQ5qHudKtYeP7&index=3
INTERACTION - Making an RPG in Unity (E02)

ni lih tko kot u tutorialu ker uno je za singleplayer tle je pa multiplayer. večina stvari je zato v NetworkPlayerInteractions.
inheritance nekak dela ker je vse lokalno
to se vse overrida

    NACELOMA BO VSAK INTERACTABLE OBJEKT OD SERVERJA, owener nebo player. player naceloma owna samo svojga characterja pa weapone
*/
public class Interactable : Interactable_objectBehavior
{
    protected InteractableLocalLock local_lock;

    private void Start()
    {
        this.local_lock = GetComponent<InteractableLocalLock>();
        //Debug.Log("found lock");
    }
    private void Update()//ce item slucajn ni od serverja
    {
        if (networkObject == null) return;
        if (networkObject.IsServer && !networkObject.IsOwner) {
            networkObject.TakeOwnership();
            //assignOwnership je za server ampak rabs mu dat networkPlayer argument.
        }
    }

    public override void DestroyWrapper(RpcArgs args)
    {
    }

    public override void HandleItemPickupServerSide(RpcArgs args)
    {
    }

    public override void ItemPickupResponse(RpcArgs args)
    {
        
    }

    internal virtual void interact(uint server_id)
    {

    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs
    {
        Debug.Log("interactable.findplayerById");
        Debug.Log(targetNetworkId);
        foreach(GameObject p in GameObject.FindGameObjectsWithTag("Player")){//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
            if (p.GetComponent<NetworkPlayerStats>().server_id == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
       // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
       // GameObject obj = networkBehavior.gameObject;


        return null;
    }

    public override void ApplyForceOnInstantiation(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public virtual void setForce(Vector3 pos, Vector3 dir)
    {
        throw new NotImplementedException();
    }

    internal virtual bool isPlayerDowned()
    {
        throw new NotImplementedException();
    }

    internal virtual void local_player_pickup_request(uint server_id)
    {
        throw new NotImplementedException();
    }

    public override void ReviveDownedPlayerRequest(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void ReviveDownedPlayerResponse(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public virtual void setMaterialGlow()
    {
        throw new NotImplementedException();
    }

    public virtual void resetMaterial()
    {
        throw new NotImplementedException();
    }
}
