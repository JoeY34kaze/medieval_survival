using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible_ArmorStand : Interactable
{
    public NetworkArmorStand nas;
    public int collider_number;
    internal override void interact(uint server_id)//sprozi na playerju
    {
        //server_id nimamo kaj rabit zdjle
        Debug.Log("Interactible armor stand : " + collider_number);
        nas.local_interaction_request(this.collider_number, server_id);

    }
}
