using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public class PlayerInstantiationLogic : MonoBehaviour {

    private void Start()
    {
        NetworkManager.Instance.InstantiateNetworkPlayerStats(0,new Vector3(313,36,561));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
