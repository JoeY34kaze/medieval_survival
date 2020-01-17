using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class local_trap_bear_handler : MonoBehaviour
{
    public bool test = false;

    private bool armed = false;

    [SerializeField]
    public bool Armed
    {
        get
        {
            return armed;
        }
        set
        {
            armed = value;
            OnArmedChanged();
        }
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }



    void OnArmedChanged()
    {
        anim.SetBool("triggered", !armed);
    }

    private Animator anim;

    private void Update()
    {
        if (test) { test = !test; Armed = !Armed; }
    }

}
