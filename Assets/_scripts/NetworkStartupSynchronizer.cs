using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// class skrbi za sinhronizacijo clienta ob connectanju na server. metoda v njegovem NetworkPlayerStats se sprozi ob connectanju, nato se pa klicejo vse instance te skripte da posljejo update njemu.(player da pohendlajo fielde, backpack za lokaicjo in iteme, armor stand za iteme in lokacijo, pickup object za lokacijo)
/// </summary>
public class NetworkStartupSynchronizer : NetworkStartupSynchronizerBehavior
{

    public virtual void SendDataToStartingClient(NetworkingPlayer p) {
        throw new NotImplementedException();
    }
}
