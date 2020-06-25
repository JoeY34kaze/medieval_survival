using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    private void Update()
    {
        if (UILogic.localPlayerGameObject != null)
            transform.position = UILogic.localPlayerGameObject.transform.position;
    }
}
