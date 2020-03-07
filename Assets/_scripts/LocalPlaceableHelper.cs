using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlaceableHelper : MonoBehaviour
{
    internal bool isSnapping;
    private bool isInCollisionWithTerrain;//za foundation recimo
    private bool setup = false;

    [SerializeField] private BoxCollider collider_for_placement;
    internal bool isCollidingWithTerrain()
    {
        if (this.setup) return this.isInCollisionWithTerrain;
        else return true;//mogoce nebo ured . vrne true zato, ker prvi frame se takoj chekira ce je valid placement, ampak detekcije kolizije se ni blo sploh.
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider is TerrainCollider) this.isInCollisionWithTerrain = true;
        this.setup = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider is TerrainCollider) this.isInCollisionWithTerrain = false;
        this.setup = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider is TerrainCollider) this.isInCollisionWithTerrain = true;
        this.setup = true;
    }

    internal BoxCollider getColliderForPlacement()
    {
        if (this.collider_for_placement == null) return GetComponent<BoxCollider>();
        else return this.collider_for_placement;
    }
}
