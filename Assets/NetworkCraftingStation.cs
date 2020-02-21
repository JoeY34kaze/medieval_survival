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

    /// <summary>
    /// client klice serverju da nj mu pokaze kaj se nahaja v tem inventoriju
    /// </summary>
    internal void local_inventory_open_request()
    {
        //if (networkObject.IsOwner)  to je zmer false?????? kako imam to na chestu what
            networkObject.SendRpc(RPC_INVENTORY_REQUEST, Receivers.Server);
    }


    /// <summary>
    /// client calls when he tries to deposit an item into the inventory of the crafting station
    /// </summary>
    /// <param name="args"></param>
    public override void Deposit(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }
    /// <summary>
    /// client calls when he tries to open the inventory of the crafting station. used to prevent ESP hacks since he had to physically be there to see the updated contents.
    /// </summary>
    /// <param name="args"></param>
    public override void InventoryRequest(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            //nekej securityja pa autorizacije rabmo ko bomo mel guilde pa tak

            if (isPlayerAuthorizedToOpen(args.Info.SendingPlayer.NetworkId))
            {
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_INVENTORY_RESPONSE, 1, this.container.getItemsNetwork());//ta metoda se klice tudi v vsakmu tipu requesta za manipulacijo z itemi
            }
            else
            {//fail, send fail response. pr rust bi ga kljucavnca shokirala recimo
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_INVENTORY_RESPONSE, 0, "-1");
            }
        }
    }

    /// <summary>
    /// called by the client to take an item from the crafting station.
    /// </summary>
    /// <param name="args"></param>
    public override void Withdraw(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }


    private bool isPlayerAuthorizedToOpen(uint networkId)
    {
        Debug.LogWarning("no security");
        return true;
    }

    public override void InventoryResponse(RpcArgs args)
    {
        //to mora vsak station pohendlat po svoje
        throw new System.NotImplementedException();
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        //Debug.Log("interactable.findplayerById");
        //Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
         //    Debug.Log(p.GetComponent<NetworkPlayerStats>().server_id);
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        //Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }
}
