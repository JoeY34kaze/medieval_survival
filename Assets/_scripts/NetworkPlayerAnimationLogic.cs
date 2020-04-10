using System.Collections;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;
using BeardedManStudios.Forge.Networking.Unity;

public class NetworkPlayerAnimationLogic : NetworkPlayerAnimationBehavior
{
    private Animator anim;


    //private bool needNetworkAnimUpdate = false;
    private Quaternion chestRotation;
    public Transform chest;
    public Transform _camera_framework;
    private NetworkPlayerStats stats;
    private NetworkPlayerCombatHandler combat_handler;
    private NetworkPlayerMovement movement;

    private bool IK_swing_active = false;
    private Vector3 IK_swing_target;

    public bool hookChestRotation=true;
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

        if (movement.lockMovement) {
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

    public void ServerSendAll(NetworkingPlayer p) {
        if (networkObject.IsServer)
        {
            networkObject.SendRpc(p, RPC_SEND_ALL, anim.GetInteger("combat_mode"), anim.GetBool("combat_blocking"), anim.GetBool("crouched"), anim.GetInteger("weapon_animation_class"), anim.GetBool("downed"));
        }
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
            anim.SetInteger("weapon_animation_class", p.item.weapon_animation_class);
    }

    internal void setCombatBlocking(bool v)
    {
        anim.SetBool("combat_blocking",v);
    }

    internal void setFeign()
    {
        anim.SetTrigger("feign");
        anim.ResetTrigger("ready_attack");
        anim.ResetTrigger("release_attack");
    }

    internal void setFire1(byte dir)
    {
        combat_handler.in_attack_animation = true;
        combat_handler.is_readying_attack = true;
        anim.SetFloat("attack_direction", (float)dir);
        anim.SetTrigger("ready_attack");
        anim.ResetTrigger("feign");
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

    internal void setReleaseOfAttack()
    {
        anim.SetTrigger("release_attack");
    }
}
