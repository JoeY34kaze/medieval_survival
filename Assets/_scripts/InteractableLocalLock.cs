using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableLocalLock : MonoBehaviour
{
    public bool item_allows_interaction = true;
    public bool item_waiting_for_destruction = false;

    internal void setupInteractionLocalLock()
    {
        StartCoroutine(lockInteraction());
    }

    IEnumerator lockInteraction()//sends vertical and horizontal speed to network
    {
        this.item_allows_interaction = false;
        yield return new WaitForSeconds(2.0f);
        this.item_allows_interaction = true;
    }
}
