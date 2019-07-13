
using UnityEngine;

public class gathering_tool_collider_handler : MonoBehaviour
{
    public Item item;
    private NetworkPlayerInventory inv;

    void OnTriggerEnter(Collider other)//nima networkobjekta. ce je server se preverja v stats.
    {
        if (other.transform.root.tag.Equals("resource"))//ce smo zadel nek resource
        {
            print("Collision detected with trigger object " + other.name);

            //other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().take_weapon_damage_server_authority(this.item, other.tag, gameObject.tag, other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id(), transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id());
            if (this.inv == null) { this.inv = transform.root.GetComponent<NetworkPlayerInventory>(); }
            inv.requestResourceHitServer(this,other.gameObject);
            this.set_offensive_colliders(false);
            
        }

    }

    public void set_offensive_colliders(bool b)
    { //BUG NETWORKOBJECT JE NULL
      // Debug.Log("--------->" + b);
        GetComponent<Collider>().enabled = b;
    }
}
