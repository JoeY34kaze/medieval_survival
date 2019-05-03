using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reticle_hit : MonoBehaviour
{ 
    public Animator animator;

    void OnEnable()
    {

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        Destroy(gameObject, clipInfo[0].clip.length);
    }
}
