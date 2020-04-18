using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bed_btn_handler : MonoBehaviour
{
    public NetworkPlayerBed bed_pointer;
    public void OnClick() {
        if (this.bed_pointer == null)
            Debug.LogError("button has no attached bed! this should not be possible!");
        this.bed_pointer.localRespawnRequest();
    }
}
