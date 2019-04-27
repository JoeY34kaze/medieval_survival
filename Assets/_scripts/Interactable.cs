using System;
using UnityEngine;
/*
 Iz brackeys
 https://www.youtube.com/watch?v=9tePzyL6dgc&list=PLPV2KyIb3jR4KLGCCAciWQ5qHudKtYeP7&index=3
 INTERACTION - Making an RPG in Unity (E02)
     
    ni lih tko kot u tutorialu ker uno je za singleplayer tle je pa multiplayer. večina stvari je zato v NetworkPlayerInteractions.
    inheritance nekak dela ker je vse lokalno
     */
public class Interactable : MonoBehaviour
{

    internal virtual void interact(NetworkPlayerInteraction networkPlayerInteraction)
    {

    }
}
