using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class alert_world_object : MonoBehaviour
{

    public NetworkPlayerInteraction linked_player_interaction;
    private void Start()
    {
        StartCoroutine(Kill(15));
    }

    IEnumerator Kill(float time)
    {
        yield return new WaitForSeconds(time);

        linked_player_interaction.kill_alert_from_alert(this.transform);
        Destroy(this.gameObject);
    }

}
