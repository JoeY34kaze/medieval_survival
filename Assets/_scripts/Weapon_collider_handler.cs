using UnityEngine;

public class Weapon_collider_handler : MonoBehaviour
{
    public Item item;
    private NetworkPlayerInventory inv;
    private NetworkPlayerAnimationLogic anim;

    private void Start()
    {
        this.anim = gameObject.transform.root.gameObject.GetComponent<NetworkPlayerAnimationLogic>();
    }
    void OnTriggerEnter(Collider other)//nima networkobjekta. ce je server se preverja v stats.
    {
        if (other.transform.root.name.Equals("NetworkPlayer(Clone)") && !other.transform.root.gameObject.Equals(transform.root.gameObject) && !other.transform.name.Equals("NetworkPlayer(Clone)")) {//ce je player && ce ni moj player && ce ni playerjev movement collider(kter je samo za movement)


            if (gameObject.CompareTag("block_player"))
            {
                //zadel smo enemy shield

            }
            else
            {
               // Debug.Log("Hit another player in the " + other.name + " | " + other.tag);

                other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().take_weapon_damage_server_authority(this.item,other.tag, other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id(), transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id());
                GetComponent<Collider>().enabled = false;
            }

            set_swing_IK(other);
        }

        if (other.transform.tag.Equals("resource"))//ce smo zadel nek resource
        {
            print("Collision detected with a resource object " + other.name);

            //other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().take_weapon_damage_server_authority(this.item, other.tag, gameObject.tag, other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id(), transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id());
            if (this.inv == null) { this.inv = transform.root.GetComponent<NetworkPlayerInventory>(); }
            inv.requestResourceHitServer(this.item, other.gameObject);
            GetComponent<Collider>().enabled = false;

            set_swing_IK(other);          
        }

    }

    public void set_offensive_colliders(bool b) { GetComponent<Collider>().enabled = b; }

    private void set_swing_IK(Collider other) {
        //RaycastHit hit;
        //Vector3 dir = other.transform.position - transform.position;
        //  if (Physics.Raycast(transform.position, dir, out hit))
        // {
        anim.on_weapon_or_tool_collision();

        // }
    }




















    /*

     void OnCollisionEnter(Collision collisionInfo)
      {
          print("Detected collision between " + gameObject.name + " and " + collisionInfo.collider.name);
          print("There are " + collisionInfo.contacts.Length + " point(s) of contacts");
          print("Their relative velocity is " + collisionInfo.relativeVelocity);
      }

      void OnCollisionStay(Collision collisionInfo)
      {
          print(gameObject.name + " and " + collisionInfo.collider.name + " are still colliding");
      }

      void OnCollisionExit(Collision collisionInfo)
      {
          print(gameObject.name + " and " + collisionInfo.collider.name + " are no longer colliding");
      }

      void OnTriggerEnter(Collider other)
      {
          print("Collision detected with trigger object " + other.name);

      }

      void OnTriggerStay(Collider other)
      {
          print("Still colliding with trigger object " + other.name);
      }

      void OnTriggerExit(Collider other)
      {
          print(gameObject.name + " and trigger object " + other.name + " are no longer colliding");
      }
      
    */
}
