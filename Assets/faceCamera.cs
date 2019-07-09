using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class faceCamera : MonoBehaviour
{
    Camera c;
    Quaternion targetRotation;
    float str;

    private void Start()
    {
        c = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //obrn se proti kameri
        
        targetRotation = Quaternion.LookRotation(transform.position - c.transform.position);
        //str = Mathf.Min(0.5f * Time.deltaTime, 1);
        transform.rotation = targetRotation;//Quaternion.Lerp(transform.rotation, targetRotation, str);
    }
}
