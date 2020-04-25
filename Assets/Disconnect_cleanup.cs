using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disconnect_cleanup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (NetworkManager n in GameObject.FindObjectsOfType<NetworkManager>())
            Destroy(n.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
