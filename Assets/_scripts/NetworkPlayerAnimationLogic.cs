﻿using System.Collections;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;
using BeardedManStudios.Forge.Networking.Unity;

public class NetworkPlayerAnimationLogic : NetworkPlayerAnimationBehavior
{
    private Animator anim;




    //private bool needNetworkAnimUpdate = false;
    private Quaternion rotation_remote;
    public Transform lower_back;
    public Transform spine;
    public Transform spine1;
    public Transform chest;
    public Transform neck;
    public Transform head;

    public Transform hips;
    

    public Transform _camera_framework;
    private NetworkPlayerStats stats;
    private NetworkPlayerCombatHandler combat_handler;
    private NetworkPlayerMovement movement;

    private bool IK_swing_active = false;
    private Vector3 IK_swing_target;

    public bool hookChestRotation=true;

    public float crouched_walk_right_y;
    public float crouched_walk_right_z;
    public float crouched_walk_left_y;
    public float crouched_walk_left_x;

    // ---------------------------------FUNCTIONS-------------------------------------
    void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
        stats = GetComponent<NetworkPlayerStats>();
        combat_handler = GetComponent<NetworkPlayerCombatHandler>();
        movement = GetComponent<NetworkPlayerMovement>();
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

        if (UILogic.hasControlOfInput) {
            result[0] = 0;
            result[1] = 0;
            return result;
        }


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
        if (Input.GetButtonUp("Crouch"))
        {
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
        if (networkObject == null) return;
        if (hookChestRotation)
            if (!stats.downed) {
                if (networkObject.IsOwner)
                {
                    rotation_remote = _camera_framework.rotation * Quaternion.Euler(new Vector3(0, 0, -90));
                    networkObject.chestRotation = rotation_remote;
                }
                else
                {
                    rotation_remote = networkObject.chestRotation;
                }
                //  chest.rotation = expected_rotation_for_head * Quaternion.Euler(new Vector3(0, 0, -90));
                apply_vertical_rotation(rotation_remote);
            }
    }

    private void apply_vertical_rotation(Quaternion q)
    {

         Quaternion BaseRotation = Quaternion.Euler(q.x, q.eulerAngles.y, q.eulerAngles.z);   

        //Quaternion BaseRotation = Quaternion.Euler(q.eulerAngles.x, q.eulerAngles.y, q.eulerAngles.z);
        //Debug.Log("base "+BaseRotation.eulerAngles);
       // Debug.Log("q " + q.eulerAngles);
        float vertical_angle = Quaternion.Angle(q, BaseRotation);
        if (q.eulerAngles.x > 180) vertical_angle *= -1;
        //Debug.Log("x axis: "+vertical_angle);
        vertical_angle = vertical_angle / 6;
        neck.Rotate(0, vertical_angle, 0);
        chest.Rotate(0,vertical_angle,0);
        spine.Rotate(0, vertical_angle, 0);
        spine1.Rotate(0, vertical_angle, 0);
        lower_back.Rotate(0, vertical_angle, 0);
        head.Rotate(0, vertical_angle, 0);


        //popravt rotacije za hojo?
        float walk_vertical = anim.GetFloat("walking_vertical");
        float walk_horizontal = anim.GetFloat("walking_horizontal");

        float horizontal_angle = 45f;

        if (walk_vertical > 0f && walk_horizontal > 0 || walk_vertical < 0f && walk_horizontal < 0)
        {
            // Debug.Log("walking diagonally forward right");
            if(anim.GetBool("crouched"))hips.Rotate(-horizontal_angle, 0, 0);
            else hips.Rotate(-horizontal_angle, 0, 0);

            horizontal_angle = horizontal_angle / 5;
            chest.Rotate(horizontal_angle, 0, 0);
            spine.Rotate(horizontal_angle, 0, 0);
            spine1.Rotate(horizontal_angle, 0, 0);
            lower_back.Rotate(horizontal_angle, 0, 0);


        }
        else if (walk_vertical > 0f && walk_horizontal < 0 || walk_vertical < 0f && walk_horizontal > 0)
        {
            if (anim.GetBool("crouched")) hips.Rotate(horizontal_angle, 0, 0);
            else hips.Rotate(horizontal_angle, 0, 0);
            horizontal_angle = -horizontal_angle / 5;
            chest.Rotate(horizontal_angle, 0, 0);
            spine.Rotate(horizontal_angle, 0, 0);
            spine1.Rotate(horizontal_angle, 0, 0);
            lower_back.Rotate(horizontal_angle, 0, 0);
        }
        else if (walk_vertical == 0f)
        {
            horizontal_angle = 90f;
            if (walk_horizontal > 0f)
            {

                // Debug.Log("walking right");
                if (anim.GetBool("crouched")) {

                    hips.Rotate(-horizontal_angle, this.crouched_walk_right_y, this.crouched_walk_right_z);
                    
                
                }
                else hips.Rotate(-horizontal_angle, 0, 0);

                horizontal_angle = horizontal_angle / 5;
                chest.Rotate(horizontal_angle, 0, 0);
                spine.Rotate(horizontal_angle, 0, 0);
                spine1.Rotate(horizontal_angle, 0, 0);
                lower_back.Rotate(horizontal_angle, 0, 0);
            }
            else if (walk_horizontal < 0f)
            {

                if (anim.GetBool("crouched")) hips.Rotate(horizontal_angle, this.crouched_walk_left_y, this.crouched_walk_left_x);
                else hips.Rotate(horizontal_angle, 0, 0);

                horizontal_angle = -horizontal_angle / 5;
                chest.Rotate(horizontal_angle, 0, 0);
                spine.Rotate(horizontal_angle, 0, 0);
                spine1.Rotate(horizontal_angle, 0, 0);
                lower_back.Rotate(horizontal_angle, 0, 0);
            }
        }
      //  Debug.Log(hips.localRotation.eulerAngles);
    }

    private void Update_Crouched_State_Owner() {
        bool current_status = anim.GetBool("crouched");
        anim.SetBool("crouched", !current_status);
        //treba je tud upddejtat weight layerja mogoce?
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
        //if (!networkObject.IsOwner) return;
        //anim.SetTrigger("land");

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
        this.setJump(false);
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

    internal void setJump(bool inAir)
    {
        bool prej = anim.GetBool("jump");
        anim.SetBool("jump", inAir);
        if (prej && !inAir)//we landed
            if(networkObject.IsOwner)
                handle_end_of_jump_owner();
        
    }

    internal void reset_readying_attack()
    {
        anim.ResetTrigger("ready_atack");
    }

    internal void setCrouched(bool b) {
        anim.SetBool("crouched", b);
    }

    public void BeginExecution() {
        //Debug.Log(NetworkManager.Instance.Networker.Me.NetworkId + " " + networkObject.Owner.NetworkId);
        networkObject.SendRpc(RPC_NETWORK_EXECUTION_UPDATE, Receivers.All);
    }

    public override void NetworkExecutionUpdate(RpcArgs args)
    {
        //if (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        //{
            anim.SetTrigger("execution");
            anim.SetInteger("combat_mode", 0);//duplicated
            anim.SetLayerWeight(1, 0);//combat layer
        //}
    }

    /// <summary>
    /// klice se na vseh, resetira tezo animacijske maske za combat
    /// </summary>
    public void ResetWeight() {
        anim.SetLayerWeight(1, 1);//combat layer
    }

    #region combat


    internal void setCombatState(byte new_mode)
    {
        if (new_mode == 1) setCombatClass(combat_handler.currently_equipped_weapon);//to bi moral bit ured ker se najprej porihta vse za item, nato pa pride se en rpc da vrze vseskup v combat mode. ce se zjebe zaporedje mamo lahko problem..
        anim.SetInteger("combat_mode", new_mode);
        
    }

    public void setCombatClass(Predmet p)
    {
        
        if(p!=null)
            anim.SetInteger("weapon_animation_class", p.getItem().weapon_animation_class);
    }

    internal void setCombatBlocking(bool blocking, byte dir)
    {

        setFeign();//najprej vrzemo v nevtralno stanje nato handlamo pripravo za block/kenslanje blocka

        if (blocking)
        {
            anim.SetBool("combat_blocking", true);
            anim.SetFloat("attack_direction", (float)dir);
        }
        else{
            anim.SetBool("combat_blocking", false);
        }
    }

    public void onShieldChanged(Predmet s) { 
        if(s!=null)
            anim.SetBool("shield_equipped", true);
        else
            anim.SetBool("shield_equipped", false);
    }

    internal void setFeign()
    {
        anim.SetTrigger("feign");
        anim.ResetTrigger("ready_attack");
        anim.ResetTrigger("release_attack");
        
    }


    #endregion

    internal void startToolAction(Item tool_to_use)
    {
        //ker bomo mel razlicne toole bi blo pametno nastimas se druge fielde na podlagi itema...

        anim.SetTrigger("tool_activated");
    }

    //--------------------   INVERSE KINEMATICS

    public void on_weapon_or_tool_collision() {
        //sprozi se z weapona ali toola ko zadane nek objekt.
        //sprozi se tudi na lokalnemu playerju ko zadane drugega playerja. v isti metodi kjer se izrise povratna informacija o damageu

        Debug.Log("on client we are setting IK target: " + anim.GetIKPosition(AvatarIKGoal.RightHand));


        this.IK_swing_active = true;
        this.IK_swing_target = anim.GetIKPosition(AvatarIKGoal.RightHand);

    }

    public void reset_swing_IK() {
        this.IK_swing_active = false;
        this.IK_swing_target = Vector3.zero;
    }

     void OnAnimatorIK(int layerIndex)
    {
        
        if (this.IK_swing_active && this.IK_swing_target != Vector3.zero)
        {
            //Debug.Log("IK!!");
            anim.SetIKPosition(AvatarIKGoal.RightHand, this.IK_swing_target);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        }
    }



    internal void handle_readying_of_attack(byte dir)
    {
        anim.SetFloat("attack_direction", (float)dir);
        anim.SetTrigger("ready_attack");
        anim.ResetTrigger("feign");
        anim.ResetTrigger("release_attack");
    }

    internal void handle_execution_of_attack()
    {
        anim.ResetTrigger("ready_attack");
        anim.ResetTrigger("feign");
        anim.SetTrigger("release_attack");
    }
}
