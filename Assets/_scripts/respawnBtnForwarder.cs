using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawnBtnForwarder : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnClick() {
        UILogic.Instance.OnRespawnWithoutBedButtonClicked();
    }
}
