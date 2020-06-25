using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{

    public string Title;
    public string Description;
    public LocationType Type;
    public enum LocationType { Town, village, PointOfInterest, Other };

}
