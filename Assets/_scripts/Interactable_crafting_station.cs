using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_crafting_station : Interactable
{
    #region setting material 
    private Material glow;
    public Material original_material;
    private MeshRenderer renderer;

    private void Start()
    {
        if (this.renderer == null) this.renderer = GetComponent<MeshRenderer>();
        if (this.renderer == null) this.renderer = GetComponentInChildren<MeshRenderer>();

        this.glow = (Material)Resources.Load("Glow_green", typeof(Material));
        this.original_material = this.renderer.material;


    }

    public override void setMaterialGlow()
    {
        if (this.renderer == null) this.renderer = GetComponent<MeshRenderer>();
        if (this.renderer == null) this.renderer = GetComponentInChildren<MeshRenderer>();


        this.renderer.material = this.glow;
    }

    public override void resetMaterial()
    {
        if (this.renderer == null) this.renderer = GetComponent<MeshRenderer>();
        if (this.renderer == null) this.renderer = GetComponentInChildren<MeshRenderer>();
        this.renderer.material = this.original_material;
    }


    #endregion

    internal void local_inventory_request()
    {
        GetComponent<NetworkContainer>().local_open_container_request();
    }

    internal void local_pickup_request()
    {
        GetComponent<NetworkContainer>().local_container_pickup_request();
    }

    internal void local_toggle_request() {
        GetComponent<NetworkCraftingStation>().local_crafting_station_togle_request();
    }
}
