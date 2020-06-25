using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LargeNotificationHandler : MonoBehaviour
{
    public Text title;
    public Text body;
    private IEnumerator notificationChecker;

    public Image bg;

    private bool IsDisabling=false;

    private IEnumerator checker() {
        bool t = true;
        while (t) {
            if (UILogic.Queued_notifications.Count > 0)
            {
                NotificationDataObject d = UILogic.Queued_notifications[0];
                UILogic.Queued_notifications.Remove(d);

                show_notification(d);
                yield return new WaitForSeconds(5f + d.desc.Length/30);
                StartCoroutine(FadeOut(1f));
                yield return new WaitForSeconds(5f);
                notification_cleanup();

            }
            else {
                t = false;
            }
        }
        this.IsDisabling = true;
    }

    private void Update()
    {
        if (IsDisabling)
            disable_notifications();
    }

    private void notification_cleanup()
    {
        
        title.text = "";
        body.text = "";
    }

    private void disable_notifications()
    {
        if (this.notificationChecker != null)
            StopCoroutine(this.notificationChecker);
        gameObject.SetActive(false);

    }
    private IEnumerator FadeIn(float time) {
        while (bg.color.a < 1.0f)
        {
            if (bg.color.a + time * Time.deltaTime > 1.0f)
            {
                bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 1);
                title.color = new Color(title.color.r, title.color.g, title.color.b, 1);
                body.color = new Color(body.color.r, body.color.g, body.color.b, 1);
            }

            else
            {
                bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, bg.color.a + time * Time.deltaTime);
                title.color = new Color(title.color.r, title.color.g, title.color.b, title.color.a + time * Time.deltaTime);
                body.color = new Color(body.color.r, body.color.g, body.color.b, body.color.a + time * Time.deltaTime);
            }
            yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator FadeOut(float time)
    {
        while (bg.color.a > 0f)
        {
            if (bg.color.a - time * Time.deltaTime < 0f)
            {
                bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0);
                title.color = new Color(title.color.r, title.color.g, title.color.b, 0);
                body.color = new Color(body.color.r, body.color.g, body.color.b, 0);
            }

            else
            {
                bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, bg.color.a - time * Time.deltaTime);
                title.color = new Color(title.color.r, title.color.g, title.color.b, title.color.a - time * Time.deltaTime);
                body.color = new Color(body.color.r, body.color.g, body.color.b, body.color.a - time * Time.deltaTime);
            }
            yield return new WaitForEndOfFrame();
        }
    }
    private void show_notification(NotificationDataObject d)
    {

        SFXManager.PlayLargeNotification(transform);
        StartCoroutine(FadeIn(1f));
        title.text = d.title;
        body.text = d.desc;
    }

    private void OnEnable()
    {
        if (this.notificationChecker != null)
            StopCoroutine(this.notificationChecker);
        this.IsDisabling = false;
        this.notificationChecker = checker();
        StartCoroutine(this.notificationChecker);
    }

}

public class NotificationDataObject {
    public string title;
    public string desc;

    public NotificationDataObject(string t, string d){
        title=t;
        desc=d;
    }
}
