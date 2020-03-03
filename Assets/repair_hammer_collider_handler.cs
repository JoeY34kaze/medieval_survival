using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class repair_hammer_collider_handler : MonoBehaviour
{

    private NetworkPlayerAnimationLogic anim;
    public Item item;

    private void Start()
    {
        this.anim = gameObject.transform.root.gameObject.GetComponent<NetworkPlayerAnimationLogic>();
    }
    void OnTriggerEnter(Collider other)
    {
        print("Repair hammer hit " + other.name);
        if (other.gameObject.GetComponent<NetworkPlaceable>()!=null)//ce smo zadel nek networkplaceable
        {
            other.gameObject.GetComponent<NetworkPlaceable>().durability_repair_request_server();
            GetComponent<Collider>().enabled = false;
            set_swing_IK(other);
        }
    }

    private void set_swing_IK(Collider other)
    {
        RaycastHit hit;
        Vector3 dir = other.transform.position - transform.position;
        if (Physics.Raycast(transform.position, dir, out hit))
        {
            anim.on_weapon_or_tool_collision(hit.point);
        }
    }
}
