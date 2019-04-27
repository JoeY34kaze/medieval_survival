using System;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
/*
 Iz brackeys
 https://www.youtube.com/watch?v=9tePzyL6dgc&list=PLPV2KyIb3jR4KLGCCAciWQ5qHudKtYeP7&index=3
 INTERACTION - Making an RPG in Unity (E02)
     
    ni lih tko kot u tutorialu ker uno je za singleplayer tle je pa multiplayer. večina stvari je zato v NetworkPlayerInteractions.
    inheritance nekak dela ker je vse lokalno
    to se vse overrida
     */
public class Interactable : Interactable_objectBehavior
{


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
}
