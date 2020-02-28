using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Class used for handling the networked behaviour of crafting stations : campfire, furnace, smithy, stove. Players will not be crafting like in ROK or Conan, but like in RUST, as in inside their inventory.
/// Crafting station like Smithy will only unlock higher tier recipes to craft while in range.
/// Crafting stations like Furnace and Campfire will do passively processing resources when they are active. Wood->coal & ore -> metal
/// Players can deposit items inside(not unrestricted by type), and withdraw items.
/// </summary>
public class NetworkCraftingStation : NetworkCraftingStationBehavior
{
    public float distance_for_interaction = 5f;
    protected NetworkContainer container;
    protected Predmet p;//p = ta crafting station
    public int crafting_tick = 5;
    public bool require_fuel = false;
    public PredmetRecepie fuel_recipe;
    public PredmetRecepie[] valid_recipes;

    public bool active=true;
    public int index_of_material = 2;
    public Material active_mat;
    public Material inactive_mat;
    public GameObject sound_effect;
    private ParticleSystem particles;
    private Material[] original_materials;
    public Renderer rend;

    

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
            start_coroutine();
            

        }
    }



    #region active_handling

    internal void local_crafting_station_togle_request()
    {
        networkObject.SendRpc(RPC_TOGGLE_ACTIVE_REQUEST, Receivers.Server);
    }

    /// <summary>
    /// klice se ko crafting stationu zmanjka goriva
    /// </summary>
    private void disable_active_server() {
        if (networkObject.IsServer) {
            networkObject.SendRpc(RPC_SEND_ACTIVE_UPDATE, Receivers.All, false);
        }
    }

    public override void ToggleActiveRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            if (is_player_allowed_to_interact_with_crafting_station(args.Info.SendingPlayer.NetworkId)) {


                bool valid = false;
                if (this.require_fuel)
                    if (is_crafting_possible(this.fuel_recipe))
                        valid = true;
                if (!this.require_fuel)
                    valid = true;

                if (valid)
                    networkObject.SendRpc(RPC_SEND_ACTIVE_UPDATE, Receivers.All, !this.active);
                else
                    networkObject.SendRpc(RPC_SEND_ACTIVE_UPDATE, Receivers.All, this.active);
            }
        }
    }

    //pogleda ce je slucajno predalec da bi interactov
    private bool is_player_allowed_to_interact_with_crafting_station(uint network_id) {
        return Vector3.Distance(FindByid(network_id).transform.position, transform.position) < this.distance_for_interaction;
    }

    public override void SendActiveUpdate(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            this.active = args.GetNext<bool>();
            if (this.active) local_activate_crafting_station_effects();
            else local_turn_off_crafting_station_effects();
        }
    }

    private void Start()
    {
        save_starting_material_array();
        this.particles = GetComponentInChildren<ParticleSystem>();
        local_turn_off_crafting_station_effects();
   
    }
    private void save_starting_material_array()
    {
        if (this.active_mat != null) this.original_materials = this.rend.materials;
    }

    void local_activate_crafting_station_effects()
    {
        this.particles.Play(true);
        if (this.active_mat != null)//tole je samo za campfire ubistvu. forge recimo nima nc veze z materjali zaenkrat. mogoce ksnej
        {
            Material[] m = this.original_materials;
            m[2] = this.active_mat;
            rend.materials = m;
        }
        this.sound_effect.SetActive(true);
    }

    void local_turn_off_crafting_station_effects()
    {
        this.particles.Stop(true);
        if (this.active_mat != null) rend.materials = this.original_materials;
        this.sound_effect.SetActive(false);
    }


    #endregion


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
            if (this.active)
                try_crafting_all();
        }
    }

    private void try_crafting_all() {
        bool did_something=false;

        //first burn the fuel;
        if (this.require_fuel)
            if (is_crafting_possible(this.fuel_recipe))
            {
                CraftingTransaction(this.fuel_recipe);
                did_something = true;
            }
            else
                if (networkObject.IsServer)
                disable_active_server();

        foreach (PredmetRecepie r in this.valid_recipes) {
            if (this.container.isEmpty()) break;
            if (is_crafting_possible(r))
            {
                CraftingTransaction(r);//kopiran iz npi
                did_something = true;
            }
        }
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
        Predmet the_baby = new Predmet(p.Product, p.final_quantity, p.Product.Max_durability, this.p.creator);
        this.container.try_to_add_predmet(the_baby);//POSLJE TUD POTREBNI NETWORKUPDATE
    }


    #endregion

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log("interactable.findplayerById");
        //Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
         //    Debug.Log(p.GetComponent<NetworkPlayerStats>().server_id);
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }
}
