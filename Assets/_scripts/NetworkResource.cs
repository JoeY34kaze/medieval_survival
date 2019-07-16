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

    public float max_size = 2f;
    public float min_size = 0.5f;

    public Item resourceItem;/// <summary>
    /// item od tega resourca. stone recimo za stone
    /// </summary>

    public enum ResourceType { stone, wood, iron};
    public ResourceType type;

    private void Start()
    {
        transform.localScale = new Vector3(max_size, max_size, max_size);

        StartCoroutine(setupParentDelayed());

    }

    private IEnumerator setupParentDelayed()
    {
        yield return new WaitForSeconds(2);
        transform.SetParent(GameObject.FindGameObjectWithTag("world_manager").transform);
    }

    public int onHit(Item tool)
    {
        if (!networkObject.IsServer) return 0;
        if (this.hp > 0)
        {
            
            float before = this.hp;
            switch (this.type)
            {
                case ResourceType.stone:
                    this.hp -= tool.stone_gather_rate;
                    break;
                case ResourceType.wood:
                    this.hp -= tool.wood_gather_rate;
                    break;
                default:
                    return 0;
            }
            if (this.hp <= 0) this.hp = 0f;

            float amount = before - this.hp;

            if (this.hp == 0) amount = amount + max_hp * 0.2f;//reward za pomajnanje tega resourca

            networkObject.SendRpc(RPC_SET_HP, Receivers.All, this.hp);

            if (amount < 0) amount = 0;
            return (int)amount;
        }
        else {
            return 0;
        }
    }

    /// <summary>
    /// klice se na vseh clientih ko enablajo resource nazaj. na serverju se klice identicna stvar so its good
    /// </summary>
    internal void onRefresh()
    {
        this.hp = max_hp;
        transform.localScale = new Vector3(max_size, max_size, max_size);
    }

    public override void setHp(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            this.hp = args.GetNext<float>();
            float new_scale = getScale();
            transform.localScale = new Vector3(new_scale, new_scale, new_scale);

            if (this.hp == 0) gameObject.SetActive(false);
        }
    }

    private float getScale() {
        return this.min_size + (this.hp / this.max_hp) * max_size;
    }
}

