using System.Collections;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;

public class NetworkPlayerAnimationLogic : NetworkPlayerAnimationBehavior
{
    private Animator anim;


    //private bool needNetworkAnimUpdate = false;
    private Quaternion chestRotation;
    public Transform chest;
    public Transform _camera_framework;
    // ---------------------------------FUNCTIONS-------------------------------------
    void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
    }



    IEnumerator UpdateNetworkPlayerWalkingAnimations()//sends vertical and horizontal speed to network
    {
        float[] axis = getVerticalAndHorizontalRounded();
        for (int i = 0; i < 3; i++)
        {
            axis = getVerticalAndHorizontalRounded();
            networkObject.SendRpc(RPC_NETWORK_MOVEMENT_ANIMATION_UPDATE, Receivers.OthersProximity, axis[0], axis[1]);
            //Debug.Log("Sent Movement animation update for this object " + axis[0] + "  "+ axis[1]);
            yield return new WaitForSeconds(0.15f);
        }
    }

    private float[] getVerticalAndHorizontalRounded() {
        float[] result = new float[2];

        float round = 0.0f;
        float round_h = 0.0f;
        if (Input.GetAxis("Vertical") > 0)
            if (Input.GetButton("Sprint"))
            {
                round = 2.0f;
            }
            else
                round = 1.0f;
        else if (Input.GetAxis("Vertical") < 0) round = -1.0f;


        if (Input.GetAxis("Horizontal") > 0) round_h = 1.0f;
        else if (Input.GetAxis("Horizontal") < 0) round_h = -1.0f;
        else round_h = 0.0f;

        result[0] = round;
        result[1] = round_h;

        return result;

    }

  

    private void Update()
    {
        if (networkObject == null) {
            Debug.LogError("networkObject is null.");
            return; }

        if (!networkObject.IsOwner) return;

        float[] axis = getVerticalAndHorizontalRounded();

        anim.SetFloat("walking_vertical", axis[0]);
        anim.SetFloat("walking_horizontal", axis[1]);


        //---------------Handle movement inputs------------------
        //hoja naprej nazaj levo desno. this is only for testing, nobody should code like this

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyUp(KeyCode.D) || Input.GetButtonDown("Sprint") || Input.GetButtonUp("Sprint")) {
            StartCoroutine("UpdateNetworkPlayerWalkingAnimations");
        }

        if (Input.GetButtonDown("Toggle_Crouch")) {
            //Debug.Log("Crouch!!");
            toggle_Update_Crouched_State_Owner();
        }


    }



    private void LateUpdate()
    {
        if (networkObject.IsOwner)
        {
            chestRotation = _camera_framework.rotation;
            networkObject.chestRotation = chestRotation;
        }
        else
        {
            chestRotation = networkObject.chestRotation;
        }

        chest.rotation = chestRotation * Quaternion.Euler(new Vector3(0,0,-90));


        
    }

    private void toggle_Update_Crouched_State_Owner() {
        bool current_status = anim.GetBool("crouched");
        anim.SetBool("crouched", !current_status);

        networkObject.SendRpc(RPC_NETWORK_MOVEMENT_ANIMATION_CROUCHED_UPDATE, Receivers.OthersProximity, !current_status);
    }

    //----------------------------------KLICANO IZ DRUGIH SCRIPT KER IMA VEZE Z ANIMACIJO -------------------------------

    internal void handle_start_of_jump_owner()
    {
        if (!networkObject.IsOwner) return;//this can never happen anyway but it doesnt hurt.
        //setup everything locally.
        anim.SetTrigger("jump");
        //broadcast it to remote objects
        networkObject.SendRpc(RPC_NETWORK_START_JUMP_REMOTE, Receivers.OthersProximity);
    }

    internal void handle_end_of_jump_owner()
    {
        if (!networkObject.IsOwner) return;
        anim.SetTrigger("land");

        networkObject.SendRpc(RPC_NETWORK_LAND_JUMP_REMOTE, Receivers.OthersProximity);
    }



    //----------------------------------------RPCS------------------------------

    public override void NetworkMovementAnimationUpdate(RpcArgs args)
    {
        if (networkObject.IsOwner) return;//owner nima kaj delat tukaj ker je nastavu vse potrebno ze v updejtih
        float vertical = args.GetNext<float>();
        float horizontal = args.GetNext<float>();
        //Debug.Log("Received Movement animation update for this object "+ vertical+"    "+horizontal);

        anim.SetFloat("walking_vertical", vertical);
        anim.SetFloat("walking_horizontal", horizontal);


    }

    public override void NetworkMovementAnimationCrouchedUpdate(RpcArgs args)
    {
        bool status = args.GetNext<bool>();
        if (networkObject.IsOwner) return;//owner nima kaj delat tukaj ker je nastavu vse potrebno ze v updejtih
        anim.SetBool("crouched",status);
    }

    public override void NetworkStartJumpRemote(RpcArgs args)
    {
        if (networkObject.IsOwner) return;
        anim.SetTrigger("jump");
    }

    public override void NetworkLandJumpRemote(RpcArgs args)
    {
        if (networkObject.IsOwner) return;
        anim.SetTrigger("land");
    }

    internal void handle_downed_start()
    {
        
        Debug.Log("downed!");
        anim.SetTrigger("downed");
        anim.SetLayerWeight(1, 0);
    }

    internal void handle_downed_end(bool revived)
    {
        if (revived)
        {
            anim.SetTrigger("downed");
           Debug.Log("revived!");
        }
        else {
            GetComponent<NetworkPlayerMovement>().do_ragdoll = true;
            Debug.Log("dead!");
        }
        anim.SetLayerWeight(1, 1);
    }
}
