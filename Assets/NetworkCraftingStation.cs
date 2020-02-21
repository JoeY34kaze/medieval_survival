using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used for handling the networked behaviour of crafting stations : campfire, furnace, smithy, stove. Players will not be crafting like in ROK or Conan, but like in RUST, as in inside their inventory.
/// Crafting station like Smithy will only unlock higher tier recipes to craft while in range.
/// Crafting stations like Furnace and Campfire will do passively processing resources when they are active. Wood->coal & ore -> metal
/// Players can deposit items inside(not unrestricted by type), and withdraw items.
/// </summary>
public class NetworkCraftingStation : NetworkCraftingStationBehavior
{
    public enum CraftingStationType { none, campfire, furnace, cooking_stove, smithy };
    public CraftingStationType station_type;
    /// <summary>
    /// [0,1,2]. 0-basic recipes, 1-intermediate recipes, 2-high tier recipes
    /// </summary>
    protected NetworkContainer_craftingStation container;
    protected Predmet p;


    /// <summary>
    /// klice se ob networkInstantiation v placeable ob kreaciji objekta
    /// </summary>
    /// <param name="p"></param>
    public void init(Predmet p)
    {
        Debug.Log("crafting station initialization!");
        this.container = GetComponent<NetworkContainer_craftingStation>();
        this.p = p;

        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();//server prevzame ownership
            this.container = GetComponent<NetworkContainer_craftingStation>();
            if (this.p != null)
                this.container.init(p.item.capacity);
            else throw new System.Exception("cannot find crafting station capacity in item");
        }
    }



    public override void ToggleActiveRequest(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }

    public override void SendActiveUpdate(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }
}
