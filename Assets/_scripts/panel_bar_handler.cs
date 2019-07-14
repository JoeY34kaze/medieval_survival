using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class panel_bar_handler : MonoBehaviour
{
    public GameObject[] slots;
    private int selected;

    private void Start()
    {
        this.selected = -1;
    }


    //TODO zamenjat GetComponent. namest da polinkamo gameobject bi loh dal direkt image
    /// <summary>
    /// nastav obrodo okol izbranga slota. ce damo -1 kot parameter bo vse sclearal
    /// </summary>
    /// <param name="v"></param>
    internal void setSelectedSlot(int v)
    {
        bool clear = true;
        for (int i = 0; i < slots.Length; i++) {
            if (i == v)
            {
                //nared d a je selected
                slots[i].GetComponent<Image>().color = Color.black;
                this.selected = v;
                clear = false;
            }
            else {
                //nared da ni selected
                slots[i].GetComponent<Image>().color = Color.white;
            }
        }
        if (clear) this.selected = -1;
    }

    internal int getSelectedIndex()
    {
        return this.selected;
    }
}
