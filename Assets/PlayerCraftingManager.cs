using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class obstaja SAMO da updejta timer na playerjevem crafting panelu. server poslje crafting panelu updejt zmer ko ga odpremo, ta updejt nastima podatke na tem singletonu in ta singleton nato handla timer. brez tega singletona timer NE dela, ker korutina nemore laufat ce je gameobject (panel) disablan.
/// </summary>
public class PlayerCraftingManager : MonoBehaviour
{
    //public static PlayerCraftingManager Instance;
    [SerializeField]

    public static GameObject playerCraftingManager_static_prefab;

    internal craftingPanelHandler panel;


    internal Text timer;
    internal List<PredmetRecepie> queueRecepieList;
    internal int current_craft_remaining_time;

    private IEnumerator CuntQueue;



    public static PlayerCraftingManager _instance;
    public static PlayerCraftingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PlayerCraftingManager>();

                if (_instance == null)
                {
                    if (PlayerCraftingManager.playerCraftingManager_static_prefab == null) PlayerCraftingManager.playerCraftingManager_static_prefab = (GameObject)Resources.Load("craftingQueuePlayer");
                    _instance = GameObject.Instantiate(PlayerCraftingManager.playerCraftingManager_static_prefab).GetComponent<PlayerCraftingManager>();
                }
            }

            return _instance;
        }
    }

    private void OnCraftingQueueEnd() {
        this.queueRecepieList.Clear();
        this.timer.text = "";
    }

    /// <summary>
    /// samo lokalno se rihta, to je samo maska za playerja, vsa logika je na serverju
    /// </summary>
    internal bool fixNextInQueue()
    {
        if (this.queueRecepieList.Count > 0)
        {
            if(this.panel.queue_list.childCount>1)
                Destroy(this.panel.queue_list.GetChild(0).gameObject);
            if (this.queueRecepieList.Count > 1)
            {
                PredmetRecepie next = this.queueRecepieList[1];
                this.current_craft_remaining_time = next.crafting_time;
                this.queueRecepieList.Remove(this.queueRecepieList[0]);

            }
            else
            {
                OnCraftingQueueEnd();
                return false;
            }
        }
        return true;
    }

    internal IEnumerator Cunter()
    {//to nj bi skos delal. loh damo tud u field as in private IEnumerator coutningRoutine in mamo referenco na to in ce je kdaj ==null ga zastartamo. tko k je zdle tega nemormo ker je nek u ozadju dela bogvekaj
        bool t = true; ;
        while (t)
        {

            if (this.current_craft_remaining_time > 0)
            {
                this.current_craft_remaining_time -= 1;
                this.timer.text = current_craft_remaining_time + "";
            }
            else
            {
                if (this.queueRecepieList.Count > 0)
                    t = fixNextInQueue();//gremo naslednga uzet
                else
                {
                    OnCraftingQueueEnd();
                    t = false;
                }
            }
            yield return new WaitForSecondsRealtime(1);
        }
        OnCraftingQueueEnd();

    }

    internal void resetCoroutine()
    {
        if (this.CuntQueue != null)
            StopCoroutine(this.CuntQueue);
        this.CuntQueue = null;
        if (this.CuntQueue == null)
        {
            this.CuntQueue = Cunter();
            StartCoroutine(this.CuntQueue);
        }
    }
}
