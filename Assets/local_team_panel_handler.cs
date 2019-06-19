using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class local_team_panel_handler : MonoBehaviour
{
    public GameObject panel_prefab;

    /// <summary>
    /// metoda v celoti updejta UI za team. pricakuje da dobi vse podatke, ker je metoda lokalna. ker je v celoti lokalno je array lahko neurejen in bo metoda sortirala po networkId predn izrise.
    ///vhod je array uint[networkId]
    /// </summary>
    public void refreshAll(uint[] my_boys) {
        //sortirej nekak
        Array.Sort(my_boys);

        //kill the current panels
        foreach (Transform c in transform)
            Destroy(c.gameObject);


        for (int i = 0; i < my_boys.Length; i++) {
            GameObject p = GameObject.Instantiate(panel_prefab);
            p.transform.SetParent(transform);
            Text t=p.GetComponentInChildren<Text>();
            NetworkPlayerStats s = FindByid(my_boys[i]).GetComponent<NetworkPlayerStats>();
            float max = s.max_health;
            float current = s.health;
            p.transform.GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = current / (max);
            p.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = s.player_name.text;
        }
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log("interactable.findplayerById");
        Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
            if (p.GetComponent<NetworkPlayerStats>().server_id == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }


}
