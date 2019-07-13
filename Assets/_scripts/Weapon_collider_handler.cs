using UnityEngine;

public class Weapon_collider_handler : MonoBehaviour
{
    public Item item;

    
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

                other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().take_weapon_damage_server_authority(this.item,other.tag, gameObject.tag, other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id(), transform.root.gameObject.GetComponent<NetworkPlayerStats>().Get_server_id());
                if (gameObject.tag.Equals("weapon_player")) this.set_offensive_colliders(false);
            }
        }
        
    }
    
    public void set_offensive_colliders(bool b) { //BUG NETWORKOBJECT JE NULL
       // Debug.Log("--------->" + b);
        GetComponent<Collider>().enabled = b;
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
