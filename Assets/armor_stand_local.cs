using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class armor_stand_local : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DynamicCharacterAvatar dyn = GetComponent<DynamicCharacterAvatar>();
        Debug.Log(dyn.characterColors);
    }

}
