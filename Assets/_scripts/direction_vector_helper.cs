using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class direction_vector_helper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal Vector3 getForward()
    {
        return Vector3.Normalize(transform.GetChild(0).transform.position- transform.position );
    }
}
