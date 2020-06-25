using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class panel_bar_handler : MonoBehaviour
{
    public GameObject[] slots;
    private List<int> selected;

    private void Start()
    {
        this.selected = new List<int>();
        //GetComponent<RectTransform>().localPosition = new Vector3(0f, -490f, 0f);

    }



    //TODO zamenjat GetComponent. namest da polinkamo gameobject bi loh dal direkt image
    /// <summary>
    /// nastav obrodo okol izbranga slota. ce damo -1 kot parameter bo vse sclearal
    /// </summary>
    /// <param name="v"></param>
    internal void setSelectedSlot(int v)
    {
        setSelectedSlots(v, -1);
    }

    internal void setSelectedSlots(int v, int z)
    {
        this.selected.Clear();
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == v || i==z)
            {
                //nared d a je selected
                slots[i].GetComponent<Image>().color = Color.black;
                this.selected.Add(i);
            }
            else
            {
                //nared da ni selected
                slots[i].GetComponent<Image>().color = Color.white;
            }
        }
    }

}
