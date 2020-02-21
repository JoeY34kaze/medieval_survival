using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;

[CreateAssetMenu(fileName = "New item",menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string Display_name = "New item";
    public enum Type { head, chest, hands, feet, legs, ranged, weapon, shield, backpack,tool,resource, placeable };
    public enum SnappableType { none, foundation, ceiling,wall, door_frame,windows_frame, wall_attachment_top, wall_attachment_side,wall_attachment_free, door, window, window_attachment, free_in_range, pillar, stairs_wide, stairs_narrow}//tle so opisani kaksnega tipa so objekti. snappable foundation recimo pomen da je foundation
    public Type type;
    public Sprite icon = null;
    public int damage;
    public bool hasDurability;
    public int durability = 1000;//max durability na itemu. staticno - mora bit
    public int stackSize = 100;
    public int capacity;
    public float damage_reduction = 0;
    public int id;
    public GameObject prefab_pickup;//rabmo za iskanje network_Id-ja
    public  int weapon_animation_class;//za switchanje po animatorju iz not_in_combat state na primern idle state. 0-unarmed, 2-1h sword, 3-1h shield
    public string uma_item_recipe_name; //recept za uma equipat
    public int stone_gather_rate;
    public int wood_gather_rate;
    public int flesh_gather_rate;
    public string description="no description yet.";
    public GameObject placeable_Local_object;
    public GameObject placeable_networked_object;
    public bool ignorePlacementNormal;
    public SnappableType PlacementType;
    public SnappableType[] blocks_placements;
}
