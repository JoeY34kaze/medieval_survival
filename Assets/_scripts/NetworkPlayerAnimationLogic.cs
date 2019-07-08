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
    private NetworkPlayerStats stats;

    public bool hookChestRotation=true;
    // ---------------------------------FUNCTIONS-------------------------------------
    void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
        stats = GetComponent<NetworkPlayerStats>();
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

        if (Input.GetButtonDown("Crouch")) {
            //Debug.Log("Crouch!!");
            Update_Crouched_State_Owner();
        }


    }
    /*
    internal bool isDodgeAllowed()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("movement") || anim.GetCurrentAnimatorStateInfo(0).IsName("crouched_movement"))
            return true;
        return false;
    }*/

    private void LateUpdate()
    {
        if (hookChestRotation)
            if (!stats.downed) {
                if (networkObject.IsOwner)
                {
                
                    chestRotation = _camera_framework.rotation;
                    networkObject.chestRotation = chestRotation;
                }
                else
                {
                    chestRotation = networkObject.chestRotation;
                }
            chest.rotation = chestRotation * Quaternion.Euler(new Vector3(0, 0, -90));
        }

        
    }

    private void Update_Crouched_State_Owner() {
        bool current_status = anim.GetBool("crouched");
        anim.SetBool("crouched", !current_status);

        networkObject.SendRpc(RPC_NETWORK_MOVEMENT_ANIMATION_CROUCHED_UPDATE, Receivers.OthersProximity, !current_status);
    }

    //----------------------------------KLICANO IZ DRUGIH SCRIPT KER IMA VEZE Z ANIMACIJO -------------------------------

    public void handle_start_of_jump_owner()
    {
        if (!networkObject.IsOwner) return;//this can never happen anyway but it doesnt hurt.
        //setup everything locally.
        anim.SetBool("jump",true);
        //broadcast it to remote objects
        networkObject.SendRpc(RPC_NETWORK_START_JUMP_REMOTE, Receivers.OthersProximity);
    }

    internal void handle_end_of_jump_owner()//??
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
        anim.SetBool("jump",true);
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
        anim.SetInteger("combat_mode", 0);//duplicated
        anim.SetLayerWeight(1, 0);//combat layer
    }

    internal void handle_player_revived()//i guess nerab zdej vec if statementa.
    {

        anim.SetTrigger("revived");
        Debug.Log("revived!");

        anim.SetLayerWeight(anim.GetLayerIndex("combat_layer"), 1);
        anim.SetInteger("combat_mode", 0);//duplicated
    }

    public void handle_player_death() {
        anim.SetTrigger("revived");
        anim.SetLayerWeight(anim.GetLayerIndex("combat_layer"), 1);
        anim.SetInteger("combat_mode", 0);//duplicated
    }

    internal void handle_dodge_start(int direction)
    {
        anim.SetTrigger("dodge");
        anim.SetInteger("dodge_direction", direction);
        anim.SetInteger("combat_mode", 0);//duplicated
        anim.SetLayerWeight(1, 0);//combat layer

        //release chest rotation from camera
        hookChestRotation = false;
    }

    internal void handle_dodge_end()
    {
        anim.SetLayerWeight(anim.GetLayerIndex("combat_layer"), 1);
        anim.SetInteger("combat_mode", 0);//duplicated

        //hook chest rotation back to camera
        hookChestRotation = true;
    }

    internal void setJump(bool st)
    {
        bool prej = anim.GetBool("grounded");
        anim.SetBool("jump", st);
        if (prej && !st)//we landed
            if(networkObject.IsOwner)
                handle_end_of_jump_owner();
        
    }

    internal void setCrouched(bool b) {
        anim.SetBool("crouched", b);
    }

    /// <summary>
    /// lokalni player v tej metodi poslje rpc serverju da nj mu da podatke o tej skripti
    /// </summary>
    internal void SendGetALL()
    {
        networkObject.SendRpc(RPC_GET_ALL, Receivers.Server);
    }

    public override void GetAll(RpcArgs args)
    {
        if (networkObject.IsServer) {
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_SEND_ALL, anim.GetInteger("combat_mode"), anim.GetBool("combat_blocking"), anim.GetBool("crouched"), anim.GetInteger("weapon_animation_class"), anim.GetBool("downed"));
        }
    }

    public override void SendAll(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            anim.SetInteger("combat_mode", args.GetNext<int>());
            anim.SetBool("combat_blocking", args.GetNext<bool>());
            anim.SetBool("crouched", args.GetNext<bool>());
            anim.SetInteger("weapon_animation_class", args.GetNext<int>());
            anim.SetBool("downed", args.GetNext<bool>());
        }
    }
}
