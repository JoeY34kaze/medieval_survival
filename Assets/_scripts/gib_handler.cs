using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gib_handler : MonoBehaviour
{
    public static float timeForGib_Death = 10f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(kill());
    }

    private IEnumerator kill() {
        yield return new WaitForSeconds(gib_handler.timeForGib_Death);
        Destroy(gameObject);
    }

}
