using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class placeable_campfire_local_handler : MonoBehaviour
{
    public bool active = false;
    public int index_of_variable_material = 2;
    public Material active_mat;
    public Material inactive_mat;
    private ParticleSystem flame;
    public Renderer rend;
    private Material[] original_materials;
    [SerializeField]  private GameObject sound_effect;
    private void Start()
    {
        save_starting_material_array();

        this.flame = GetComponentInChildren<ParticleSystem>();
        if (active)
        {
            turn_on_campfire_local();
        }
        else {
            turn_off_campfire_local();
        }
    }
    private void save_starting_material_array() {
        this.original_materials = this.rend.materials;
    }

    void turn_on_campfire_local() {
        this.flame.Play(true);
        Material[] m = this.original_materials;
        m[2] = this.active_mat;
        rend.materials = m;
        this.sound_effect.SetActive(true);
    }

    void turn_off_campfire_local()
    {
        this.flame.Stop(true);
        rend.materials = this.original_materials;
        this.sound_effect.SetActive(false);
    }

}
