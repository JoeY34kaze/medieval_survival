using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class unityUIjeRetard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<InputField>().characterLimit = 150;
    }
}
