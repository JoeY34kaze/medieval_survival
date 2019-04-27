using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New item",menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string Display_name = "New item";
    public Sprite icon = null;
    public int damage;
    public int id;
    public GameObject prefab_pickup;
}
