using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public class PlayerInstantiationLogic : MonoBehaviour {

    private void Start()
    {
        NetworkManager.Instance.InstantiateNetworkPlayerStats();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
