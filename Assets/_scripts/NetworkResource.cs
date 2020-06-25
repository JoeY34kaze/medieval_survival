using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;

public class NetworkResource : NetworkResourceBehavior
{
    public float max_hp;
    public float hp; // == amount of resource

    private bool recently_depleted = false;

    private Vector3 start_position;
    private Quaternion start_rotation;

    public Vector3 sound_effect_position;

    public Item resourceItem;/// <summary>
    /// item od tega resourca. stone recimo za stone
    /// </summary>

    public enum ResourceType { stone, wood, ore};
    public ResourceType type;

    private void Start()
    {
        StartCoroutine(setupParentDelayed());//lol . lets fix this later. sej verjetno nebo tkole blo objektov sploh ampak bojo bli na terenu.

        this.start_position = transform.position;
        this.start_rotation = transform.rotation;
        
        
    }

    private IEnumerator setupParentDelayed()
    {
        yield return new WaitForSeconds(1);
        transform.SetParent(NetworkWorldManager.Instance.transform);
    }

    public Predmet onHitReturnItemWithQuantity(Item tool, string playerName)
    {
        if (!networkObject.IsServer) return null;
        if (this.hp > 0)
        {
            
            float before = this.hp;
            switch (this.type)
            {
                case ResourceType.stone:
                    this.hp -= tool.stone_gather_rate;
                    break;
                case ResourceType.ore:
                    this.hp -= tool.stone_gather_rate;
                    break;
                case ResourceType.wood:
                    this.hp -= tool.wood_gather_rate;
                    break;
                default:
                    return null;
            }
            if (this.hp <= 0) this.hp = 0f;

            float amount = before - this.hp;

            if (this.hp == 0) amount = amount + max_hp * 0.2f;//reward za pomajnanje tega resourca

            networkObject.SendRpc(RPC_SET_HP, Receivers.All, this.hp);

            if (amount < 0) amount = 0;

            return new Predmet(this.resourceItem, (int)amount, 0, playerName);
        }
        else {
            return null;
        }
    }

    /// <summary>
    /// klice se na vseh clientih ko enablajo resource nazaj. na serverju se klice identicna stvar so its good
    /// </summary>
    internal void onRefresh()
    {
        if (!this.recently_depleted)
        {
            this.hp = max_hp;
            transform.position = this.start_position;
            transform.rotation = this.start_rotation;


            if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = true;
            if (GetComponent<MeshRenderer>() != null) GetComponent<MeshRenderer>().enabled = true;
            if (GetComponent<Rigidbody>() != null) Destroy(GetComponent<Rigidbody>());
        }
    }

    public override void setHp(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            this.hp = args.GetNext<float>();
            on_resource_hit_effects();


            if (this.hp == 0) {
                on_resource_depleted();
                
            }
        }
    }

    private void on_resource_hit_effects() {


        SFXManager.OnResourceHit(transform, this.type);
        //other effects ..
    }


    private void on_resource_depleted()
    {
        this.recently_depleted = true;

        SFXManager.OnResourceDepleted(transform, this.type);

        switch (this.type) {
            case ResourceType.wood:
                on_tree_depleted();
                break;
            case ResourceType.stone:
                on_stone_node_depleted();
                break;
            case ResourceType.ore:
                on_ore_node_depleted();
                break;
            default:
                gameObject.SetActive(false);
            return;
        }
    }

    private void on_tree_depleted()
    {
        if (GetComponent<Rigidbody>() == null) gameObject.AddComponent<Rigidbody>();
        GetComponent<Rigidbody>().mass = 5000;
        StartCoroutine(disable_resource_delayed(20));

    }

    private void on_stone_node_depleted()
    {
        disable_resource();
    }

    private void on_ore_node_depleted()
    {
        disable_resource();
    }

    private IEnumerator disable_resource_delayed(float t) {
        yield return new WaitForSecondsRealtime(t);
        disable_resource();
    }

    private void disable_resource() {

        this.recently_depleted = false;
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
        if (GetComponent<MeshRenderer>() != null) GetComponent<MeshRenderer>().enabled = false;
        if (GetComponent<Rigidbody>() != null) Destroy(GetComponent<Rigidbody>());
    }
}

