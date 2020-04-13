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
    public static readonly float destroy_wait_time=60f;
    public static readonly float destroychance = 0.1f;
    [SerializeField] Rigidbody rb;

    internal void init(Predmet p, Vector3 spawn, Vector3 direction, float force) {
        if (!networkObject.IsServer) return;
        this.p = p;

        //nastavt tud na serverju - velocity and such
        
        if (this.rb == null) GetComponent<Rigidbody>();

        this.rb.AddForce(direction * force);


        //poslat vsem clientim!!

        //razen seveda trenutno ker je nrjen z fieldi in ne rpcji..... - to eb changed before release trenutno ne nrdi nic, samo groundwork za presaltanje na rpcje
    }

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
        if (networkObject.IsServer)
        {
            print("Detected collision between " + gameObject.name + " and " + collisionInfo.collider.name);
            print("There are " + collisionInfo.contacts.Length + " point(s) of contacts");
            print("Their relative velocity is " + collisionInfo.relativeVelocity);

            this.local_projectile.handle_on_hit_effects();
            Debug.LogWarning("DEBUG CODE!");
            if (collisionInfo.collider.gameObject.GetComponent<NetworkPlaceable>() != null)
            {
                collisionInfo.collider.gameObject.GetComponent<NetworkPlaceable>().take_weapon_damage(this.p);
                networkObject.SendRpc(RPC_SEND_HIT_TO_CLIENTS, Receivers.OthersProximity);
                if (destroy_on_impact_chance())
                    networkObject.Destroy();
            }
        }
    }

    private bool destroy_on_impact_chance() {
        float f = UnityEngine.Random.value;//Returns a random number between 0.0 [inclusive] and 1.0 [inclusive] (Read Only).
        if (Networked_siege_projectile.destroychance <= f) return true;
        else return false;
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
