using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappableObject
{
    public Vector3 position;
    public Quaternion rotation;

    public SnappableObject(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
    }
}
