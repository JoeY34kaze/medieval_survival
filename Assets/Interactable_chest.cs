using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_chest : Interactable
{
    NetworkChest chest;

    //verejtno bi blo najbols prestavt to logiko na parenta? - problem z vrati ker se potem vidi cez zarad transparentnosti materjala
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
}
