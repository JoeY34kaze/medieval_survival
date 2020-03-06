 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;

 public class FlickeringLight : MonoBehaviour
 {
     public Light flickeringLight;
     public float FlickerSmooth;
 
     // Start is called before the first frame update
     void Start()
     {
        FlickerSmooth = 0.25f;
     }
 
     // Update is called once per frame
     void Update()
     {
         float lickerIntensity = Random.Range(0.0f,5.0f) * FlickerSmooth;// use float values to get smoothing intensity variation
         
         flickeringLight.intensity = lickerIntensity;
     
     }
 }