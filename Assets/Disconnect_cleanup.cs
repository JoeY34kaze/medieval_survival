using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disconnect_cleanup : MonoBehaviour{
    void Start()
    {
        foreach (NetworkManager n in GameObject.FindObjectsOfType<NetworkManager>())
            Destroy(n.gameObject);
    }
}
