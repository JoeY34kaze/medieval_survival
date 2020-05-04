﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tester : MonoBehaviour
{
    public bool resetAll = false;

    public bool test = false;
    private bool prev_test=false;

    public bool release = false;
    private Animator anim;
    [Range(-1,2f)]
    public float forward;
    [Range(-1, 1f)]
    public float horizontal;
    public float crouched_walk_right_y;
    public float crouched_walk_right_z;
    public float crouched_walk_left_y;
    public float crouched_walk_left_x;
    public Transform hips;

    void Start()
    {
        this.anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (resetAll) {
            resetAll = false;
            test = false;
            prev_test = false;
            release = false;
            anim.SetBool("test", false);
            anim.ResetTrigger("release");
        }


        if (test != prev_test) {
            prev_test = test;
            GetComponent<Animator>().SetBool("test",test);
        }

        if (test && release ) {
            
            release = false;
            anim.SetTrigger("release");
            test = false;
        }
        anim.SetFloat("forward", this.forward);
        anim.SetFloat("horizontal", this.horizontal);

        apply_vertical_rotation();

    }

    private void apply_vertical_rotation()
    {


           float horizontal_angle = 90f;
            if (horizontal > 0f)
            {


                    hips.Rotate(-horizontal_angle, this.crouched_walk_right_y, this.crouched_walk_right_z);


            }
            else if (horizontal < 0f)
            {
                hips.Rotate(horizontal_angle, this.crouched_walk_left_y, this.crouched_walk_left_x);
                
            }
        

    }
}
