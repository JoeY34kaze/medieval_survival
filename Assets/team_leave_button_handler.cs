using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class team_leave_button_handler : MonoBehaviour
{
    private NetworkPlayerStats st;
    public bool retard = false;
    public void leave_team_onClick() {
        st.local_tryToLeaveTeam();
    }

    private void Start()
    {
        st = transform.root.GetComponent<NetworkPlayerStats>();
    }

    private void Update()
    {
        if(retard || transform.parent.childCount<2)
                    DestroyImmediate(this.gameObject);
         
    }

}
