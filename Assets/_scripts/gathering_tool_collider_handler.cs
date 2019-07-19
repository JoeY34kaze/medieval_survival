
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
            inv.requestResourceHitServer(this.item, other.gameObject);
            GetComponent<Collider>().enabled = false;

        }

        if (other.transform.root.name.Equals("NetworkPlayer(Clone)") && !other.transform.root.gameObject.Equals(transform.root.gameObject) && !other.transform.name.Equals("NetworkPlayer(Clone)"))
        {//ce je player && ce ni moj player && ce ni playerjev movement collider(kter je samo za movement)


            if (gameObject.CompareTag("block_player"))
            {
                //zadel smo enemy shield

            }
            else
            {
                // Debug.Log("Hit another player in the " + other.name + " | " + other.tag);

                other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().take_weapon_damage_server_authority(this.item, other.tag, other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id(), transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id());
                GetComponent<Collider>().enabled = false;
            }
        }
    }
    

}
