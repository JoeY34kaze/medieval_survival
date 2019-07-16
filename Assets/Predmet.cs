using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Osnova predmeta v igri. nesmemo uporablat Itema vec, ampak uporablamo ta objekt
/// </summary>
public class Predmet : MonoBehaviour
{
    public Item item;
    public int quantity;
    //public string creator;
    public int durability;

    public Predmet(Item i) {
        this.item = i;
    }

    public Predmet(Item i, int quantity) {
        this.item = i;
        this.quantity = quantity;
    }

    public Predmet(Item i, int quantity, int durability) {
        this.item = i;
        this.quantity = quantity;
        this.durability = durability;
    }
}
