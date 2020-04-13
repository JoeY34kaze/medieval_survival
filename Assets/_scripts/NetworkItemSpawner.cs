using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkItemSpawner : NetworkItemSpawnerBehavior
{
    public Item i;
    public int quantity=1;
    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (!networkObject.IsServer) return;

        if (this.quantity >= i.stackSize) this.quantity = i.stackSize;
        if (this.quantity <= 0) this.quantity = 1;
        Predmet p = new Predmet(i, this.quantity);

        int net_id = getNetworkIdFromInteractableObject(i);
        if (net_id != -1)
        { //item is interactable object
            Interactable_objectBehavior b = NetworkManager.Instance.InstantiateInteractable_object(net_id, transform.position);
            //apply force on clients, sets predmet
            b.gameObject.GetComponent<Interactable>().setStartingInstantiationParameters(p, transform.position, Vector3.zero);
        }
    }
        private int getNetworkIdFromInteractableObject(Item item)//to naceloma skor vedno spawna en zakelj
        {
            GameObject[] prefabs = NetworkManager.Instance.Interactable_objectNetworkObject;
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i].Equals(item.prefab_pickup))
                    return i;
            }
            Debug.LogWarning("Id of item not found. Item is probably registered as something different from Interactable_objectNetworkObject. Like for example backpack.");
            return -1;
        }
    }
