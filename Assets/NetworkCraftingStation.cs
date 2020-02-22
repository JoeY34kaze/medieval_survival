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
    protected NetworkContainer container;
    protected Predmet p;//p = ta crafting station

    public int crafting_tick = 5;
    public bool active = true;
    public bool require_fuel = false;
    public PredmetRecepie fuel_recipe;

    public PredmetRecepie[] valid_recipes;

    private IEnumerator crafting_coroutine;
    /// <summary>
    /// klice se ob networkInstantiation v placeable ob kreaciji objekta
    /// </summary>
    /// <param name="p"></param>
    public void init(Predmet p)
    {
        Debug.Log("crafting station initialization!");
        this.container = GetComponent<NetworkContainer>();
        this.p = p;

        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();//server prevzame ownership
            this.container = GetComponent<NetworkContainer>();
            if (this.p != null)
                this.container.init(p);
            else throw new System.Exception("cannot find crafting station capacity in item");

            if (this.valid_recipes.Length > 0) {
                start_coroutine();
            }

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


    private bool set_active(bool state) {
        if (networkObject.IsServer)
            this.active = state;
        return this.active;
    }


    #region crafting_queue

    private void start_coroutine() {
        if (networkObject.IsServer) {
            this.crafting_coroutine = craft_checker();
            StartCoroutine(this.crafting_coroutine);
        }
    }

    private void resetCoroutine()
    {
        if (this.crafting_coroutine != null)
            StopCoroutine(this.crafting_coroutine);
        this.crafting_coroutine = null;
        if (this.crafting_coroutine == null)
        {
            this.crafting_coroutine = craft_checker();
            StartCoroutine(this.crafting_coroutine);
        }
    }

    /// <summary>
    /// coroutine na vsake par sekund pogleda ce so resourci not in scrafta en recept ce so.  ce sattion nima fuela potem ne nrdi nic
    /// </summary>
    /// <returns></returns>
    private IEnumerator craft_checker()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(crafting_tick);
            if (this.active&&!this.require_fuel || this.active && this.require_fuel)
                try_crafting_all();
        }
    }

    private void try_crafting_all() {
        bool did_something=false;
        foreach (PredmetRecepie r in this.valid_recipes) {
            if (this.container.isEmpty()) break;
            if (is_crafting_possible(r))
            {
                CraftingTransaction(r);//kopiran iz npi
                did_something = true;
            }
        }

        //also burn the fuel;
        if (this.require_fuel)
            if (is_crafting_possible(this.fuel_recipe))
            {
                CraftingTransaction(this.fuel_recipe);
                did_something = true;
            }
            else
                set_active(false);

        if (did_something)
        {
            this.container.send_container_update_crafting_station();
        }

    }

    private bool is_crafting_possible(PredmetRecepie p) {
        return getMaxNumberOfPossibleCraftsForRecipe(p) > 0;
    }

    private int getMaxNumberOfPossibleCraftsForRecipe(PredmetRecepie p)
    {
        int minimum = int.MaxValue;
        for (int i = 0; i < p.ingredients.Length; i++)
        {
            //get max number of crafts for this particular item.
            int q = p.ingredient_quantities[i];
            int pool = getQuantityOfItemInContainer(p.ingredients[i]);

            if (pool / q < minimum) minimum = pool / q;
        }
        return minimum;
    }

    internal int getQuantityOfItemInContainer(Item item) {
        Predmet[] pool = this.container.get_container_inventory();
        int q = 0;
        foreach (Predmet p in pool)
            if (p != null)
                if (p.item.Equals(item))
                    q += p.quantity;
        return q;
    }

    //koda bazira precej na kodi iz npi
    private void CraftingTransaction(PredmetRecepie p)
    {
        //loop cez vse matse in jih brisemo iz inventorija dokler ne zbrisemo zadost.
        for (int i = 0; i < p.ingredients.Length; i++)
        {
            int q = p.ingredient_quantities[i];//tolkle moramo zbrisat iz nekje
            this.container.Remove(p.ingredients[i], q);
        }
        Predmet the_baby = new Predmet(p.Product, p.final_quantity, p.Product.durability, this.p.creator);
        this.container.try_to_add_predmet(the_baby);//POSLJE TUD POTREBNI NETWORKUPDATE
    }


    #endregion
}
