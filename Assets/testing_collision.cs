using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing_collision : MonoBehaviour
{
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        print("Collision detected with trigger object " + other.name);
        
    }


    void OnCollisionEnter(Collision collisionInfo)
    {
        print("Detected collision between " + gameObject.name + " and " + collisionInfo.collider.name);
        print("There are " + collisionInfo.contacts.Length + " point(s) of contacts");
        print("Their relative velocity is " + collisionInfo.relativeVelocity);
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        print(gameObject.name + " and " + collisionInfo.collider.name + " are still colliding");
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        print(gameObject.name + " and " + collisionInfo.collider.name + " are no longer colliding");
    }

    /*   void OnTriggerEnter(Collider other)
       {
           print("Collision detected with trigger object " + other.name);

       }*/

    void OnTriggerStay(Collider other)
    {
        print("Still colliding with trigger object " + other.name);
    }

    void OnTriggerExit(Collider other)
    {
        print(gameObject.name + " and trigger object " + other.name + " are no longer colliding");
    }

}
