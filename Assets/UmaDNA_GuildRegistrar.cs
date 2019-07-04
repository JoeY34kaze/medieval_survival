using System.Collections;
using System.Collections.Generic;
using UMA.CharacterSystem;
using UnityEngine;

public class UmaDNA_GuildRegistrar : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DynamicCharacterAvatar avatar = GetComponent<DynamicCharacterAvatar>();
        Dictionary<string, DnaSetter> dna = avatar.GetDNA();


        dna["breastSize"].Set(1f);
        dna["headSize"].Set(0f);

        avatar.BuildCharacter();
    }

}
