using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// handles everything regarding the team invite panel.
/// </summary>
public class panel_guild_invite_handler : MonoBehaviour
{
    public Text info;
    public Text countdown;

    private uint id_other;
    private NetworkGuildManager gManager;


    public void init(string gm_name, string guild_name, uint other, NetworkGuildManager ngm)
    {
        info.text = "Player " + gm_name+" wants to invite you to join his guild named "+guild_name;
        this.id_other = other;
        gManager = ngm;
        GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        StartCoroutine(Timer());
    }
    public void declineClick()
    {

        
        gManager.LocalGuildRequestResponse(this.id_other, false);
        Destroy(this.gameObject);
    }

    public void acceptClick()
    {
        gManager.LocalGuildRequestResponse(this.id_other, true);
        Destroy(this.gameObject);
    }

    IEnumerator Timer()
    {
        for (int i = 60; i >= 0; i -= 1)
        {
            countdown.text = "TIMEOUT : " + i + "s";
            yield return new WaitForSecondsRealtime(1);
        }
        Debug.Log("Received no response! declining request to join guild!");
        declineClick();
    }
}
