using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class team_leave_button_handler : MonoBehaviour
{
    private NetworkPlayerStats st;
    public void leave_team_onClick() {
        st.local_tryToLeaveTeam();
    }

    private void Start()
    {
        st = transform.root.GetComponent<NetworkPlayerStats>();
    }


}
