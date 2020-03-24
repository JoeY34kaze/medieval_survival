using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class panel_guild_handler : MonoBehaviour
{
    public GameObject member_prefab;
    public Transform listTransform;
    public Text GuildNameText;
    public Text GuildMasterName;

    public void Clear() {
        foreach (Transform child in listTransform) Destroy(child.gameObject);
    }

    public void init(uint member_id, string playername, bool isGm)
    {
        GameObject inst =GameObject.Instantiate(member_prefab);
        inst.transform.SetParent(listTransform);
        inst.GetComponent<panel_guild_memeber_handler>().init(member_id, playername, this, isGm);

    }

    /// <summary>
    /// leave guild lahko klikne samo lokalni player itak tko da samo poslemo rpc
    /// </summary>
    public void OnLeaveGuildClicked() {
        NetworkGuildManager.Instance.leaveGuildRequest();
    }

    internal void initGm(uint gm, string playerName, bool isGm)
    {
        GameObject inst = GameObject.Instantiate(member_prefab);
        inst.transform.SetParent(listTransform);
        inst.GetComponent<panel_guild_memeber_handler>().initGm(gm, playerName, this, isGm);
    }

    /// <summary>
    /// poslje njetworkguildmanagerju lokalni request da nj poslje na server request da kicka playerja z guilda.
    /// </summary>
    /// <param name="designated_player"></param>
    internal void localKickRequest(uint designated_player)
    {
        NetworkGuildManager.Instance.localKickRequest((designated_player));

    }
}
