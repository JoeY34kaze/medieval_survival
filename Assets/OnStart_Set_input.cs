using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnStart_Set_input : MonoBehaviour
{
    // Start is called before the first frame update
    public string t="";
    void Update()
    {
        if(GetComponent<Text>().text=="") GetComponent<Text>().text = t;
    }

}
