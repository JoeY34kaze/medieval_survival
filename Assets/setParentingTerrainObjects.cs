using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setParentingTerrainObjects : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform parent;
    void Start()
    {
        if(this.parent!=null)
            transform.parent = parent;
    }

}
