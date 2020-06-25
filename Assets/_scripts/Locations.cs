using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locations : MonoBehaviour
{
    public static List<Location> All;


    private void Start()
    {
        //generate all locations
        Locations.All = new List<Location>();

        foreach (Location l in transform.GetComponentsInChildren<Location>())
            if(!Locations.All.Contains(l))
                Locations.All.Add(l);

        // MESTA ------- Turjak, Zuzemberk , Sticna, Visnja gora

        //vasi ------- marinca, kompolje, krka
    }

    internal static Location GetCurrentLocation(Vector3 position)
    {
        position = position + Vector3.up * 5000f;
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 17;
        // This would cast rays only against colliders in layer 8.

        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
          //  Debug.DrawRay(position, Vector3.down * hit.distance, Color.yellow);
           // Debug.Log("Did Hit");
            return hit.transform.GetComponent<Location>();
        }
        else
        {
           // Debug.DrawRay(position, Vector3.down * 5000, Color.white);
           // Debug.Log("Did not Hit");
            return null;
        }
    }

    internal static Location GetNearestRespawnLocation(Vector3 position)
    {
        float min_v = float.MaxValue;
        Location curr = null;
        float dist = min_v;
        foreach (Location l in All) {
            if (l.Type!=Location.LocationType.Town && l.Type != Location.LocationType.village)
                continue;

            dist = Vector3.Distance(position, l.transform.position);
            if (dist < min_v) {
                min_v = dist;
                curr = l;
            }
        }
        return curr;
    }
}


