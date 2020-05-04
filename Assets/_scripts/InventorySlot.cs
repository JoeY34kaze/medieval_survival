using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InventorySlot : MonoBehaviour
{
    public Image icon;          // Reference to the Icon image
    protected Predmet predmet;  // Current item in the slot
    //public Image icon_background;          // Reference to the Icon image to display when empty
    protected Image durability_bar;
    protected Text text_quantity;
    public int index;//tole MORA bit nastiman v inspectorju ker se sibling index spreminja ko draggas iteme okol

    private void Start()
    {
        this.predmet = null;
        this.durability_bar = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        //this.durability_bar.fillAmount = 0.5f;
        this.text_quantity = transform.GetChild(2).GetComponent<Text>();
        //this.text_quantity.text = "x50";
        //icon_background = GetComponentInChildren<Image>();

        ClearSlot();
       
    }

    private void setReferences() {
        this.durability_bar = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        //this.durability_bar.fillAmount = 0.5f;
        this.text_quantity = transform.GetChild(2).GetComponent<Text>();
    }

    public void AddPredmet(Predmet p)
    {



        if (this.durability_bar == null || this.text_quantity == null)
            setReferences();
            if (p == null) { ClearSlot(); return; };

        predmet = p;
        icon.sprite = p.getItem().icon;
        icon.enabled = true;

        if (p.getItem().hasDurability)
        {
            if (this.durability_bar != null)
            {
                this.durability_bar.transform.parent.gameObject.SetActive(true);
                this.durability_bar.fillAmount = p.current_durabilty / p.getItem().Max_durability;
            }
        }
        else {
            if (this.durability_bar != null)
                this.durability_bar.transform.parent.gameObject.SetActive(false);
        }
        if (p.quantity > 1)
        {
            if (this.text_quantity != null)
            {
                this.text_quantity.gameObject.SetActive(true);
                this.text_quantity.text = "x" + p.quantity;
            }
        }
        else {
            if (this.text_quantity != null)
                this.text_quantity.gameObject.SetActive(false);
        }

        

    }

    public Predmet GetPredmet()
    {
        return this.predmet;
    }

    // Clear the slot
    public void ClearSlot()
    {
        if (this.durability_bar == null || this.text_quantity == null)
            setReferences();



        predmet = null;
        //icon.sprite = icon_background.sprite;
        icon.sprite = null;
        icon.enabled = false;
        if(this.durability_bar!=null)
            this.durability_bar.transform.parent.gameObject.SetActive(false);
        if(this.text_quantity!=null)
            this.text_quantity.gameObject.SetActive(false);
    }

    public void OnClicked() {
        Debug.Log("UI clot clicked!");
        if (this.predmet != null) {
            Debug.Log("It Contains predmet!");
            UILogic.Instance.OnInventoryPredmetSelected(this.index, this);
        }
    }

}
