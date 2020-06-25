using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public class PlayerInstantiationLogic : MonoBehaviour {

    private void Start()
    {
        NetworkManager.Instance.InstantiateNetworkPlayerStats(0,new Vector3(15423, 423, 15817));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
