using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactable_trebuchet : Interactable
{
    //rearming, pickup
    private Material[][] original_materials;
    private Material glow;
    public Renderer[] currentPlaceableRenderers;

    private void Awake()
    {
        this.glow = (Material)Resources.Load("Glow_green", typeof(Material));
        this.original_materials = new Material[this.currentPlaceableRenderers.Length][];
        for (int i = 0; i < this.currentPlaceableRenderers.Length; i++)
            this.original_materials[i] = this.currentPlaceableRenderers[i].materials;

    }

    private void set_material(Material m)
    {
        for (int i = 0; i < this.currentPlaceableRenderers.Length; i++)
            this.currentPlaceableRenderers[i].material = m;
    }

    public override void setMaterialGlow()
    {
        for (int i = 0; i < this.currentPlaceableRenderers.Length; i++)
            this.currentPlaceableRenderers[i].material = glow;

    }

    public override void resetMaterial()
    {
        for (int i = 0; i < this.currentPlaceableRenderers.Length; i++)
            this.currentPlaceableRenderers[i].materials = this.original_materials[i];
    }
}
