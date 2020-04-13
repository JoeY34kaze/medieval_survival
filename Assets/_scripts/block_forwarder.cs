using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class block_forwarder : MonoBehaviour
{
    void OnTriggerEnter(Collider other)//nima networkobjekta. ce je server se preverja v stats.
    {
        if(other.gameObject.tag.Equals("weapon_player"))
            transform.parent.GetComponent<Weapon_collider_handler>().OnTriggerEnter(other);
    }
}
