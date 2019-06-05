using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public class PlayerInstantiationLogic : MonoBehaviour {

    private void Start()
    {
        NetworkManager.Instance.InstantiateNetworkPlayerStats(0,new Vector3(-4f,2f,6.09f));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
