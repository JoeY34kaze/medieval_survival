using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPredmetDescriptionHandler : MonoBehaviour
{

    public Image predmet_image;
    public Slider amount_slider;
    public InputField inputfield;
    public Text maxSliderLabel;
    public Text ItemName;
    public Text ItemDescription;

    private Predmet temporary_selected_predmet;

    private InventorySlot currentSlot;

    public void init(Predmet p, InventorySlot slot) {
        this.currentSlot = slot;
        gameObject.SetActive(true);
        this.temporary_selected_predmet = p;
        if (p.quantity > 1)
        {
            this.amount_slider.gameObject.SetActive(true);
            amount_slider.minValue = 1;
            amount_slider.maxValue = p.quantity;
            amount_slider.value = Mathf.Round(amount_slider.minValue + amount_slider.maxValue / 2);
            this.inputfield.text = amount_slider.value + "";
            Item it = p.getItem();
            this.predmet_image.sprite =it.icon;
            this.maxSliderLabel.text = amount_slider.maxValue + "";
            this.ItemName.text = it.Display_name;
            this.ItemDescription.text = it.description;
        }
        else {
            this.amount_slider.gameObject.SetActive(false);
        }
    }

    public void Clear() {
        this.temporary_selected_predmet = null;
        this.currentSlot = null;
        this.predmet_image.sprite = null;
        this.maxSliderLabel.text = 0+"";
        this.ItemName.text = "";
        this.ItemDescription.text = "";

        gameObject.SetActive ( false);
    }

    public void OnSliderChanged() {
        int val = (int)this.amount_slider.value;
        this.inputfield.text = val+"";
    }

    public void onInputChanged() {
        if (is_text_legit(this.inputfield.text)) {
            int val = Int32.Parse(this.inputfield.text);
            this.amount_slider.value = val;
        }
    }

    private bool is_text_legit(string text)
    {
        foreach (char c in text)
            if (!Char.IsDigit(c))
                return false;
        if (text != null) return true;
        return false;
    }

    public void OnButtonClicked() {
        Debug.Log("Requesting to split the stack of " + this.temporary_selected_predmet.getItem().Display_name + " of size " + this.temporary_selected_predmet.quantity + " to a stack of " + this.inputfield.text + ". slot data: index: " + currentSlot.index + " type: " + currentSlot.GetType());
        UILogic.local_npi.localPlayerSplitStackRequest(this.currentSlot, Int32.Parse(this.inputfield.text));
    }
}
