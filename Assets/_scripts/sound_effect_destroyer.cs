using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sound_effect_destroyer : MonoBehaviour
{
    // Start is called before the first frame update
    private float sound_effect_length=10f;
    void Start()
    {
        this.sound_effect_length = GetComponent<AudioSource>().clip.length;
        StartCoroutine("killer");
    }

    IEnumerator killer()
    {
        yield return new WaitForSecondsRealtime(this.sound_effect_length);
        Destroy(gameObject);
    }
}
