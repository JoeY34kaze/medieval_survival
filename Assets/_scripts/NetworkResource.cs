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
    public GameObject[] sound_effects_on_hit;
    public GameObject[] sound_effects_on_depletion;

    private void Start()
    {
        StartCoroutine(setupParentDelayed());

        this.start_position = transform.position;
        this.start_rotation = transform.rotation;
        
        
    }

    private IEnumerator setupParentDelayed()
    {
        yield return new WaitForSeconds(2);
        transform.SetParent(GameObject.FindGameObjectWithTag("world_manager").transform);
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


        on_resource_hit_sound();
    }

    private void on_resource_hit_sound() {

        if (this.sound_effects_on_hit.Length > 0) {
            instantiate_random_gameobject_from_array(this.sound_effects_on_hit);
        }
    }

    private void on_resource_depleted()
    {
        this.recently_depleted = true;

        if(this.sound_effects_on_depletion.Length>0)
            instantiate_random_gameobject_from_array(this.sound_effects_on_depletion);
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
        GetComponent<Rigidbody>().mass = 500;
        StartCoroutine("disable_resource_delayed", 10);

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


    /// <summary>
    /// arr je array of prefab sound effects with onInstantiated -> play sound
    /// </summary>
    /// <param name="arr"></param>
    private void instantiate_random_gameobject_from_array(GameObject[] arr) {
        System.Random r = new System.Random();
        int rInt = r.Next(0, arr.Length-1);
        GameObject g = GameObject.Instantiate(arr[rInt], gameObject.transform.position+this.sound_effect_position, gameObject.transform.rotation);
        g.transform.SetParent(null);
    }
}

