using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class local_player_testing_handler : MonoBehaviour
{
    public bool handle = false;
    public bool handled_automatically = false;
    // Start is called before the first frame update
    public GameObject new_root;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //
        //if (transform.childCount>1&&!handled_automatically || handle)
           if(handle)rewire();
    }

    private void rewire() {
        Debug.Log(transform.GetChild(0).name);

        Destroy(transform.GetChild(0).gameObject);
        new_root.transform.SetParent(transform);
        new_root.transform.SetAsFirstSibling();

        GetComponentInChildren<SkinnedMeshRenderer>().rootBone = new_root.transform;

        handle = false;
    }
}
