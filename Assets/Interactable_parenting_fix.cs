using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// class nalimas na objekt v hierarhiji ki ima collider. uporabla se zato da v networkplayerinteraction vrne pravilni interactable
/// </summary>
public class Interactable_parenting_fix : Interactable
    
{
    public Interactable parent_interactable;
}
