using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class deathScreenFadeToBlack : MonoBehaviour
{
    // Start is called before the first frame update
    Image i;
    public GameObject bed_btn;
    public GameObject no_bed_btn;
    private void Start()
    {
        i = GetComponent<Image>();
    }
    private void OnEnable()
    {
        if (i == null) i = GetComponent<Image>();
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        StartCoroutine(fade());
    }

    IEnumerator fade() {
        Text youDied = transform.GetChild(0).GetComponent<Text>();
        youDied.color = new Color(255, 255, 255, 0);


        while (i.color.a < 1.0f) {
            if (i.color.a + 0.2f * Time.deltaTime > 1.0f)
                i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
            else
                i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + 0.3f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        //zdj rabmo napis pokazat pa druge podrobnosti o smrti

        while (youDied.color.a < 1.0f)
        {
            if (youDied.color.a + 0.2f * Time.deltaTime > 1.0f)
                youDied.color = new Color(255, 255, 255, 1);
            else
                youDied.color = new Color(255,255, 255, youDied.color.a + 0.3f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        //zdj rabmo izrisat gumbe za postelje
        UILogic.Instance.deathScreenBeds.SetActive(true);

        reset_beds();
        populate_beds(UILogic.Instance.deathScreenBeds);
    }

    private void populate_beds(GameObject deathScreenBeds)
    {
        GameObject btn2 = GameObject.Instantiate(this.no_bed_btn);
        btn2.transform.SetParent(UILogic.Instance.deathScreenBeds.transform);
        btn2.transform.localScale = Vector3.one;

        foreach (NetworkPlayerBed b in GameObject.FindObjectsOfType<NetworkPlayerBed>()) {
            if (b.transform.GetComponent<NetworkPlaceable>().is_player_owner(UILogic.localPlayerGameObject.GetComponent<NetworkPlayerStats>().Get_server_id())) {
                GameObject btn = GameObject.Instantiate(this.bed_btn);
                btn.transform.SetParent(UILogic.Instance.deathScreenBeds.transform);
                btn.transform.localScale = Vector3.one;
                btn.GetComponent<bed_btn_handler>().bed_pointer = b;//TO MORA IMET. CE CRKNE TUKAJ JE PROV KER TO MORA ZMER DELAT IN JE TREBA POPRAVT
                btn.GetComponentInChildren<Text>().text = b.name;//to bi mogl delat ker ima button samo 1 childa in to je napis na buttonu
            }
        }
    }

    private void reset_beds() {
        Transform[] c = new Transform[UILogic.Instance.deathScreenBeds.transform.childCount];
        for (int i = 0; i < UILogic.Instance.deathScreenBeds.transform.childCount; i++)
            c[i] = UILogic.Instance.deathScreenBeds.transform.GetChild(i);

        foreach (Transform t in c) Destroy(t.gameObject);
    }
}
