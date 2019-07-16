
using UnityEngine;

public class gathering_tool_collider_handler : MonoBehaviour
{
    public Item item;
    private NetworkPlayerInventory inv;

    void OnTriggerEnter(Collider other)
    {
        print("Collision detected with trigger object " + other.name);
        if (other.transform.tag.Equals("resource"))//ce smo zadel nek resource
        {
            print("Collision detected with a resource object " + other.name);

            //other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().take_weapon_damage_server_authority(this.item, other.tag, gameObject.tag, other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id(), transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id());
            if (this.inv == null) { this.inv = transform.root.GetComponent<NetworkPlayerInventory>(); }
            inv.requestResourceHitServer(this, other.gameObject);
            GetComponent<Collider>().enabled = false;

        }
    }
    

}
