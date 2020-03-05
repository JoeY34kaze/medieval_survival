using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
public class Networked_siege_projectile : NetworkedSiegeProjectileBehavior
{

    private Predmet p;
    private local_siege_projectile local_projectile;

    private void Start()
    {
        this.local_projectile = GetComponent<local_siege_projectile>();
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer) {
            networkObject.TakeOwnership();
        }
    }

    private void Update()
    {
        if (networkObject != null)
        {
            if (networkObject.IsServer)
            {
                networkObject.position = transform.position;
            }
            else
            {
                transform.position = networkObject.position;
            }
        }
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
       
        print("Detected collision between " + gameObject.name + " and " + collisionInfo.collider.name);
        print("There are " + collisionInfo.contacts.Length + " point(s) of contacts");
        print("Their relative velocity is " + collisionInfo.relativeVelocity);

        this.local_projectile.handle_on_hit_effects();
        Debug.LogWarning("DEBUG CODE!");
        if (collisionInfo.collider.gameObject.GetComponent<NetworkPlaceable>() != null) {
            collisionInfo.collider.gameObject.GetComponent<NetworkPlaceable>().handle_object_destruction();
        }
    }

    /// <summary>
    /// poslje server, klice se na clientih da nrdijo efekt explozije or whatever
    /// </summary>
    /// <param name="args"></param>
    public override void sendHitToClients(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            Vector3 pos = args.GetNext<Vector3>();
            if (this.local_projectile != null)
                this.local_projectile.handle_on_hit_effects();
        }
    }

    /// <summary>
    /// poslje server playerju, ki je ustrelil ta projectile da mu izrise un reticle. zaenkrat ni informacije o dmg, samo indikator da je neki zadel
    /// </summary>
    /// <param name="args"></param>
    public override void send_hit_response_to_owner(RpcArgs args)
    {
        throw new NotImplementedException();
    }

}
