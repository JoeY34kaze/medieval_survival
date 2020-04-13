using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tester : MonoBehaviour
{
    public bool resetAll = false;

    public bool test = false;
    private bool prev_test=false;

    public bool release = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (resetAll) {
            resetAll = false;
            test = false;
            prev_test = false;
            release = false;
            GetComponent<Animator>().SetBool("test", false);
            GetComponent<Animator>().ResetTrigger("release");
        }


        if (test != prev_test) {
            prev_test = test;
            GetComponent<Animator>().SetBool("test",test);
        }

        if (test && release ) {
            
            release = false;
            GetComponent<Animator>().SetTrigger("release");
            test = false;
        }
    }
}
