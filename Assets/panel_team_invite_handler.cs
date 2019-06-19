using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// handles everything regarding the team invite panel.
/// </summary>
public class panel_team_invite_handler : MonoBehaviour
{
    public Text info;
    public Text countdown;

    private uint id_other;


    public void init(GameObject player_other) {
        info.text = "You are being invited to team by : " + player_other.GetComponent<NetworkPlayerStats>().player_name;
        this.id_other = player_other.GetComponent<NetworkPlayerStats>().server_id;
    }

    public void declineClick() {

        transform.root.GetComponent<NetworkPlayerStats>().LocalTeamRequestResponse(this.id_other, false);
        Destroy(this.gameObject);
    }

    public void acceptClick() {
        transform.root.GetComponent<NetworkPlayerStats>().LocalTeamRequestResponse(this.id_other, true);
        Destroy(this.gameObject);
    }

    IEnumerator Timer()
    {
        for (int i = 60; i >= 0; i -= 1)
        {
            yield return new WaitForSecondsRealtime(1);
        }
        Debug.Log("Received no response! declining request to team!");
        declineClick();
    }
}
