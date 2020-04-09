using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fadeout : MonoBehaviour
{
    // Start is called before the first frame update
    public float rate_per_second = 0.3f;

    private Image i;
    private void Start()
    {
        this.i = GetComponent<Image>();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if(this.i==null) this.i = GetComponent<Image>();
        Color c = i.color;
        float t = c.a - this.rate_per_second * Time.deltaTime;
        if (t < 0) t = 0;
        i.color = new Color(c.r, c.g, c.b, t);

        if (c.a <= 0) gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (this.i == null) this.i = GetComponent<Image>();
        Color c = i.color;
        this.i.color = new Color(c.r, c.g, c.b, 1.0f);
    }
}
