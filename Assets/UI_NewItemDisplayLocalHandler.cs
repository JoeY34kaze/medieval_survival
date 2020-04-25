using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_NewItemDisplayLocalHandler : MonoBehaviour
{
    public Text sign;
    public Text quantity;
    public Text displayName;
    public Color c;

    private Image i;
    public void init(Predmet p)
    {
        this.i = GetComponent<Image>();
        if (p.quantity > 0)
        {
            sign.text = "+";
            this.c = Color.green;
            this.quantity.text = p.quantity+"";
        }
        else if (p.quantity < 0) {
            sign.text = "-";
            this.c = Color.red;
            this.quantity.text = (p.quantity - p.quantity - p.quantity) + "";
        }

        this.displayName.text = p.getItem().Display_name;

        update_color();
        StartCoroutine(fade());
    }

    private void update_color() {
        this.sign.color = this.c;
        this.quantity.color = this.c;
        this.displayName.color = this.c;

        this.i.color = new Color(this.i.color.r, this.i.color.g, this.i.color.b, this.c.a);
    }

    IEnumerator fade()
    {
        while (this.c.a > 0.0f)
        {
            if (this.c.a - 0.2f * Time.deltaTime > 1.0f)
                this.c = new Color(this.c.r, this.c.g, this.c.b, 0);
            else
                this.c = new Color(this.c.r, this.c.g, this.c.b, this.c.a - 0.2f * Time.deltaTime);
            update_color();
            yield return new WaitForEndOfFrame();
        }
        //zdj rabmo napis pokazat pa druge podrobnosti o smrti
        Destroy(gameObject);
        
    }
}
