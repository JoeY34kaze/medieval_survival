using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrain_rock_collider_rotator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //poiskat kasno rotacijo mora imet
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(new Vector3(270, transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.z));
    }
}
