using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectVolumeController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("effectsVolume"))
            GetComponent<AudioSource>().volume *= PlayerPrefs.GetFloat("effectsVolume");
    }


}
