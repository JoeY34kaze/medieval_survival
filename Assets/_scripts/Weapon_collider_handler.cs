using System;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using UnityEngine;

public class Weapon_collider_handler : MonoBehaviour
{

    public float weapon_damage = 15;
    
    void OnTriggerEnter(Collider other)
    {
       // if (this.player == null || this.player_stats == null) {
           // Debug.Log("player or player_stats is null");
        //    return; }
        //if (!gameObject.GetComponent<collision_lock>().available) {
           // Debug.Log("Collision lock prevents collision handling!");
         //   return; }

        //print(player_stats.server_id+"   -   "+player_stats.player_name.text+" Collision detected with trigger object " + other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().server_id +" - "+ other.gameObject.GetComponent<NetworkPlayerStats>().player_name.text);
        //if (other.transform.root.gameObject.Equals(player)) { Debug.Log("Im hitting myself."); }
        if (other.transform.root.name.Equals("NetworkPlayer(Clone)") && !other.transform.root.gameObject.Equals(transform.root.gameObject) && !other.transform.name.Equals("NetworkPlayer(Clone)") && !gameObject.CompareTag("block_player")) {//ce je player && ce ni moj player && ce ni playerjev movement collider(kter je samo za movement)
            //Debug.Log("Hit another player in the " + other.name +" | "+ other.tag);
            other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().take_weapon_damage_server_authority(weapon_damage, other.tag, gameObject.tag, other.transform.root.gameObject.GetComponent<NetworkPlayerStats>().server_id, transform.root.gameObject.GetComponent<NetworkPlayerStats>().server_id);
            if(gameObject.tag.Equals("weapon_player"))this.set_offensive_colliders(false); 
        }
        
    }

    public void set_offensive_colliders(bool b) { //BUG NETWORKOBJECT JE NULL
        Debug.Log("--------->" + b);
        GetComponent<Collider>().enabled = b;
    }



   



















    /*  void OnCollisionEnter(Collision collisionInfo)
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
