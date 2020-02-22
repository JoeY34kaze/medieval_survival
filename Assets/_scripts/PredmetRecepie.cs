using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;

[CreateAssetMenu(fileName = "newPredmetRecepie", menuName = "Medieval Survival/PredmetRecepie")]
public class PredmetRecepie : ScriptableObject
{
    public Item Product;
    public int final_quantity;

    public Item[] ingredients;
    public int[] ingredient_quantities;

    public int tier = 0;
    public int crafting_time;

    public bool craftable_by_player = true;
    
}
