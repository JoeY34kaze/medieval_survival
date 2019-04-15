using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision_lock : MonoBehaviour//class nrjen zato da server lokalno hrani stanje da izklopi collision detection. server nemore spreminjat fieldov od playerjev ker ni owner zato je lokalno za potrebe serverja
{
    public bool available = true;
}
