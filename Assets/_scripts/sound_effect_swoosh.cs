using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_effect_swoosh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, gameObject.GetComponent<AudioSource>().clip.length);
    }



}
