using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    #region singleton
    private static SFXManager _instance;
    public static SFXManager Instance
    {

        get
        {
            if (_instance == null) Debug.LogError("SFXMANAGER.INSTANCE IS NULL! CRITICAL ERROR!!");
            return _instance;
        }
        set { _instance = value; }
    }

    private void Start()
    {
        SFXManager.Instance = this;
    }

    #endregion


    #region ARRAYS
    // Start is called before the first frame update
    [Header("Combat SFX")]
    public GameObject[] player_swing_bladed_weapon_sfx;//swoosh
    public GameObject[] sfx_block_with_weapon;
    public GameObject[] sfx_block_with_shield;
    public GameObject[] sfx_player_hit_with_sword;
    public GameObject[] sfx_player_death_by_weapon;
    public GameObject[] sfx_draw_bladed_weapon;

    [Header("RESOURCE SFX")]
    public GameObject[] WoodHitSfx;
    public GameObject[] WoodDepletedSfx;

    public GameObject[] StoneHitSfx;
    public GameObject[] StoneDepletedSfx;

    public GameObject[] OreHitSfx;
    public GameObject[] OreDepletedSfx;

    [Header("Movement SFX")]
    public GameObject[] footsteps_basic;
    public GameObject[] footsteps_on_stone;
    public GameObject[] footsteps_on_dirt;
    public GameObject[] footsteps_on_leaves;
    public GameObject[] footsteps_metal_on_stone;
    public GameObject[] footsteps_metal_on_wood;


    [Header("INVENTORY SFX")]
    public GameObject[] basicItemPickup;

    [Header("INGAME UI SFX")]
    public GameObject BasicLargeNotificationSFX;
    #endregion

    #region misc

    private static void play_random_sound_effect(GameObject[] sound_fx, Transform t)
    {
        if (sound_fx.Length > 0)
        {
            int k = (int)UnityEngine.Random.Range(0, sound_fx.Length);
            GameObject.Instantiate(sound_fx[k], t.position, t.rotation, t);
        }
    }
    private static void play_random_sound_effect(GameObject[] sound_fx, Transform t, float sound_multiplier)
    {
        if (sound_fx.Length > 0)
        {
            int k = (int)UnityEngine.Random.Range(0, sound_fx.Length);
            GameObject g = GameObject.Instantiate(sound_fx[k], t.position, t.rotation, t);
            g.GetComponent<AudioSource>().volume = g.GetComponent<AudioSource>().volume * sound_multiplier;
        }
    }

    #endregion


    #region Combat
    public static void OnWeaponAttackSFX(Transform player, Item i) {

        if (is_bladed_weapon(i.id))
        {
            //GameObject clip = 
                play_random_sound_effect(SFXManager.Instance.player_swing_bladed_weapon_sfx, player);
            //clip.transform.parent = player;
        }
        else
            Debug.LogWarning("Weapon is not a bladed one so we are missing sound effects on its attack!");
    }

    private static bool is_bladed_weapon(int id)//Item.id
    {
       return id == 22 || id == 23 || id == 24 || id == 37 || id == 38 || id == 39;
    }

    internal static void OnWeaponDrawn(Transform t, Item item)
    {
        if (is_bladed_weapon(item.id))
            play_random_sound_effect(SFXManager.Instance.sfx_draw_bladed_weapon, t);
        else
            Debug.LogWarning("Weapon is not a bladed one so we are missing sound effects on its selection!");
    }

    internal static void OnPlayerDeath(Transform t,bool is_violent_death)
    {
        play_random_sound_effect(SFXManager.Instance.sfx_player_death_by_weapon,t);
    }

    internal static void OnPlayerHit(Transform t)
    {
        play_random_sound_effect(SFXManager.Instance.sfx_player_hit_with_sword,t);
    }

    internal static void OnBlock(Transform t, bool with_shield)
    {
        if (with_shield)
        {
            //blocked with shield. i guess neki wooden sound effects
            play_random_sound_effect(SFXManager.Instance.sfx_block_with_weapon,t);
        }
        else
        {
            play_random_sound_effect(SFXManager.Instance.sfx_block_with_shield,t);
        }
    }
    #endregion


    #region RESOURCES

    internal static void OnResourceHit(Transform t,NetworkResource.ResourceType type) {
        if (type == NetworkResource.ResourceType.wood)
            play_random_sound_effect(SFXManager.Instance.WoodHitSfx, t);
        else if (type == NetworkResource.ResourceType.stone)
            play_random_sound_effect(SFXManager.Instance.StoneHitSfx, t);
        else if (type == NetworkResource.ResourceType.ore)
            play_random_sound_effect(SFXManager.Instance.OreHitSfx, t);
    }

    internal static void OnResourceDepleted(Transform t, NetworkResource.ResourceType type)
    {
        if (type == NetworkResource.ResourceType.wood)
            play_random_sound_effect(SFXManager.Instance.WoodDepletedSfx, t);
        else if (type == NetworkResource.ResourceType.stone)
            play_random_sound_effect(SFXManager.Instance.StoneDepletedSfx, t);
        else if (type == NetworkResource.ResourceType.ore)
            play_random_sound_effect(SFXManager.Instance.OreDepletedSfx, t);
    }

    #endregion

    #region Movement

    internal static void OnFootstep(Transform t, int surface_type, bool metal_feet, bool running) {
        //0 je vse zaenkrat- basic cist
        float multiplier = 1f;
        if (running) multiplier = 2f;


               switch (surface_type)
        {
            case 1://-on dirt
                if (metal_feet)
                    play_random_sound_effect(SFXManager.Instance.footsteps_on_dirt, t, multiplier);//we are missing proper sound fx so we're using basic
                else
                    play_random_sound_effect(SFXManager.Instance.footsteps_on_dirt, t, multiplier);
                break;
            case 2://-on leaves
                if (metal_feet)
                    play_random_sound_effect(SFXManager.Instance.footsteps_on_leaves, t, multiplier);
                else
                    play_random_sound_effect(SFXManager.Instance.footsteps_on_leaves, t, multiplier);
                break;
            case 3://on stone
                if (metal_feet)
                    play_random_sound_effect(SFXManager.Instance.footsteps_metal_on_stone, t, multiplier);
                else
                    play_random_sound_effect(SFXManager.Instance.footsteps_on_stone, t, multiplier);
                break;
            case 4://on wood
                if (metal_feet)
                    play_random_sound_effect(SFXManager.Instance.footsteps_metal_on_wood, t, multiplier);
                else
                    play_random_sound_effect(SFXManager.Instance.footsteps_basic, t, multiplier);//we are missing proper sound fx so we're using basic
                break;
            default://0 - basic
                if(metal_feet)
                    play_random_sound_effect(SFXManager.Instance.footsteps_basic, t, multiplier);
                else
                    play_random_sound_effect(SFXManager.Instance.footsteps_basic, t, multiplier);
                break;
        }
    }

    #endregion

    #region INVENTORY
    internal static void OnItemPickup(Transform t, int rarity) {
        if (rarity > 0) Debug.LogWarning("Rarity of item is higher that what we have sound effects! fix this before alpha");
        else if (rarity == 0)
            play_random_sound_effect(SFXManager.Instance.basicItemPickup, t);
    }
    #endregion

    #region UI
    public static void PlayLargeNotification(Transform t) {
        GameObject g = GameObject.Instantiate(SFXManager.Instance.BasicLargeNotificationSFX, t.position, t.rotation, t);
    }

    #endregion
}
