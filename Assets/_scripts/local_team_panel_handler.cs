using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class local_team_panel_handler : MonoBehaviour
{
    public GameObject panel_prefab;
    public GameObject leave_button;
    public GameObject btn_retard;

    /// <summary>
    /// metoda v celoti updejta UI za team. pricakuje da dobi vse podatke, ker je metoda lokalna. ker je v celoti lokalno je array lahko neurejen in bo metoda sortirala po networkId predn izrise.
    ///vhod je array uint[networkId]
    /// </summary>
    /// 

    public void refreshAll(uint[] my_boys) {//bols blo nrdit da e sam preshiftajo ko se spremeni ampak to bo ksnej se jebat


        //if(btn_retard!=null)Destroy(btn_retard);
        //transform.GetChild(i).gameObject.SetActive(false);
        //transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(false);
        //transform.GetChild(i).transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        //Destroy(transform.GetChild(i));


        //if (btn_retard != null) DestroyImmediate(btn_retard);
        //if (btn_retard != null) if (btn_retard.GetComponent<CanvasGroup>()!=null) Destroy(btn_retard.GetComponent<CanvasGroup>());
        //if (btn_retard != null) Destroy(btn_retard);
        if (btn_retard != null) btn_retard.GetComponent<team_leave_button_handler>().retard = true;

        

        for (int i = 0; i < this.transform.childCount; i++) {
            Debug.Log(transform.GetChild(i).name);
            if(transform.GetChild(i).GetComponent<team_leave_button_handler>()==null)//ce ni button
                Destroy(transform.GetChild(i).gameObject);
        }





        if (my_boys != null)//ce je null smo zakljucli. smo rabil samo zbrisat
        {

            //sortirej nekak
            Array.Sort(my_boys);


            //----------------------BUTTON-----------------
      
                GameObject p = GameObject.Instantiate(leave_button);
                p.transform.SetParent(transform);
                this.btn_retard = p;
            
            // leave_button.SetActive(true);
            // leave_button.transform.GetChild(0).gameObject.SetActive(true);
            // leave_button.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            //---------------------PANELS----------------------
            for (int i = 0; i < my_boys.Length; i++)
            {
                 p = GameObject.Instantiate(panel_prefab);
                p.transform.SetParent(transform);
                Text t = p.GetComponentInChildren<Text>();
                NetworkPlayerStats s = FindByid(my_boys[i]).GetComponent<NetworkPlayerStats>();
                float max = s.max_health;
                float current = s.health;
                p.transform.GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = current / (max);
                p.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = s.player_displayed_name.text;
                p.GetComponent<team_memeber_panel_helper>().init(my_boys[i]);
            }
        }

    }



    public void refreshHp(uint player, float newHp) {//inneficient. optimize later
        if (transform.childCount > 2)
            foreach (Transform child in transform)
            {
                team_memeber_panel_helper th = child.GetComponent<team_memeber_panel_helper>();
                if (th != null)
                    if (th.id_player == player)
                        th.changeHp(newHp);
            }
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log("interactable.findplayerById");
        Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }


}
