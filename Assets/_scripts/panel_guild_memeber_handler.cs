using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class panel_guild_memeber_handler : MonoBehaviour
{
    private uint designated_player;
    public GameObject btn_kick;
    public Text PlayerName;
    public Text GuildRank;
    private panel_guild_handler pgh;


    public void init(uint member_id, string playername, panel_guild_handler pgh, bool isGm)
    {
        this.designated_player = member_id;
        this.pgh = pgh;
        if(!isGm)this.btn_kick.SetActive(false);
        this.GuildRank.text = "Member";
        this.PlayerName.text = playername;
        
    }

    public void initGm(uint GM_id, string playername, panel_guild_handler pgh, bool isGm)
    {
        this.designated_player = GM_id;
        this.GuildRank.text = "Guild Master";
        this.PlayerName.text = playername;
        this.pgh = pgh;
        if (!isGm) this.btn_kick.SetActive(false);
    }

    public void OnButtonKickClicked() {
        pgh.localKickRequest(this.designated_player);
    }

}
