using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class Network_body : Network_bodyBehavior
{
    public override void setup_UMA(RpcArgs args)//poslje server vsem
    {
        string data = args.GetNext<string>();
        Debug.Log("received UMA data : " + data);
    }

    public void update_UMA_body(string d) {
        networkObject.SendRpc(RPC_SETUP__U_M_A, Receivers.All, d);
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();

        GetComponent<UMA.Dynamics.UMAPhysicsAvatar>().ragdolled = true;
    }

    private void LateUpdate()
    {
        if (GetComponent<UMA.Dynamics.UMAPhysicsAvatar>() != null) {
            if(!GetComponent<UMA.Dynamics.UMAPhysicsAvatar>().ragdolled)
                GetComponent<UMA.Dynamics.UMAPhysicsAvatar>().ragdolled = true;
        }
    }
}
