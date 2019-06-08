﻿using UnityEngine;

using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;
using System.Collections;

public class NetworkPlayerMovement : NetworkPlayerMovementBehavior
{
    /// <summary>
    /// The speed that the cube will move by when the user presses a
    /// Horizontal or Vertical mapped key
    /// </summary>
    public float normal_speed = 1.0f;
    private int dodge_direction = 0;

    public float sprint_modifier = 2.0f;
    public float crouched_modifier = 0.25f;

    public float visina_skoka = 2.0f;

    private bool crouched;

    public float light_weapon_speed_modifier = 0.8f;
    public float heavy_weapon_speed_modifier = 0.6f;

    private float speed = 1.0f;
    private Animator anim;
    //public CapsuleCollider movement_collider_checker;

    //public bool do_ragdoll = false;
    public float dolzina_za_ground_check_raycast = 0.4f;
    public float distance_from_center_raycast = 0.2f;
    private bool isGrounded = true;
    private bool in_a_jump = false;

    private Rigidbody rigidbody;

    private NetworkPlayerAnimationLogic animation_handler_script;
    private NetworkPlayerCombatHandler combat_handler_script;
    private NetworkPlayerInventory networkPlayerInventory;
    private NetworkPlayerStats stats;

    Vector3 next_position;
    //--------------------------------RAGDOLL --------------- tutorial v=RrWrnp2DLD8 ------------------------



    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        combat_handler_script = GetComponent<NetworkPlayerCombatHandler>();
        networkPlayerInventory = GetComponent<NetworkPlayerInventory>();
        stats = GetComponent<NetworkPlayerStats>();
    }


    private void FixedUpdate()
    { //CE DELAMO KAKSNE EMOTE MORAMO ZAKLENT OPCIJO DA SE LAHKO PLAYER PREMIKA!!! SICER SE ZJEBE GROUND DETECTION

        // If we are not the owner of this network object then we should
        // move this cube to the position/rotation dictated by the owner
        if (networkObject == null)
        {
            Debug.LogError("networkObject is null.");
            return;
        }
        if (!networkObject.IsOwner)
        {


            transform.position = networkObject.position;
            transform.rotation = networkObject.rotation;

            if (!rigidbody.isKinematic) rigidbody.isKinematic = true; //clipping issues
            if (rigidbody.detectCollisions) rigidbody.detectCollisions = false;
            return;
        }
        if (animation_handler_script == null) { animation_handler_script = GetComponent<NetworkPlayerAnimationLogic>(); }
        /*
        if (do_ragdoll)//ko bo dejanska smrt
        {

            GetComponent<UMA.Dynamics.UMAPhysicsAvatar>().ragdolled = true;
            networkObject.SendRpc(RPC_RAGDOLL, Receivers.Others);

            return;//najbrz spawnat box z playerjevimi itemi pa mrde kamero okol vrtet kej tazga
        }

        */

        if (!stats.downed)//dvakrat je tale check. zato da ce je downan v zraku se zmer pade na tla in ne lebdi v zrak
        {
            if (Input.GetButtonDown("Dodge") && !stats.inDodge)
            {
                Debug.Log("Dodge started");
                handle_dodge_start();
            }
            else if (stats.inDodge)
            {
                //Debug.Log("dodging");
            }
            else
            {

                crouched = anim.GetBool("crouched");
                speed = normal_speed;

                if (crouched) speed = speed * crouched_modifier;


                //---------------------------------------------------DA TE MAL POSLOWA K NAPADAS Z WEAPONOM----------------------------------------------
                if (combat_handler_script.in_attack_animation)
                {
                    if (combat_handler_script.index_of_currently_selected_weapon_from_equipped_weapons == 2) speed *= light_weapon_speed_modifier;
                }
                //----------------------------------------------------------------------------------------------------------------------------------------

                next_position = transform.position;

                Vector3 dirVector = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized * speed * Time.fixedDeltaTime;
                if (Input.GetButton("Sprint") && Input.GetAxis("Vertical") > 0)
                    dirVector *= sprint_modifier;

                next_position += dirVector;

                if (!networkPlayerInventory.panel_inventory.activeSelf)//ce nimamo odprt inventorij - to je samo za horizontalno premikanje miske. vertikalno je nekje drugje
                {
                    Quaternion turnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * GetComponent<player_camera_handler>().mouse_sensitivity_multiplier, Vector3.up);
                    transform.eulerAngles = transform.eulerAngles + turnAngle.eulerAngles;
                }

                check_ground_raycast(distance_from_center_raycast);
            }
            //gravity
            //if(!isGrounded)
            rigidbody.AddForce(Vector3.up * Physics.gravity.y * 2, ForceMode.Acceleration);


            if (!stats.downed)
            {
                if (Input.GetAxis("Jump") > 0.01f && isGrounded && !in_a_jump) // && isGrounded??? isGrounded je trenutno se mal buggy
                {
                    //jump();
                    Debug.Log(Vector3.up * 6.3f);

                    rigidbody.AddForce(Vector3.up * visina_skoka * 2, ForceMode.VelocityChange);
                    StartCoroutine(lock_jumping(1));
                }
                /*
                Vector3 point_on_ground = get_capsulecasted_position_downward_from_chest();
                int state_of_vertical=check_ground(point_on_ground);
                next_position=apply_gravity(next_position,point_on_ground,state_of_vertical);
                */
            }
        }
        rigidbody.MovePosition(next_position);
        //transform.position = next_position;
        networkObject.position = transform.position;
        networkObject.rotation = transform.rotation;
    }


    private void check_ground_raycast(float distance_from_center)
    {
        bool b = Physics.Raycast(transform.position, Vector3.down, dolzina_za_ground_check_raycast);

        if (!b) b = Physics.Raycast(transform.position + transform.forward * distance_from_center, Vector3.down, dolzina_za_ground_check_raycast);
        if (!b) b = Physics.Raycast(transform.position - transform.forward * distance_from_center, Vector3.down, dolzina_za_ground_check_raycast);
        if (!b) b = Physics.Raycast(transform.position + transform.right * distance_from_center, Vector3.down, dolzina_za_ground_check_raycast);
        if (!b) b = Physics.Raycast(transform.position - transform.right * distance_from_center, Vector3.down, dolzina_za_ground_check_raycast);

        isGrounded = b;
        anim.SetBool("grounded", b);

    }

    IEnumerator lock_jumping(float t)//sends vertical and horizontal speed to network
    {
        in_a_jump = true;
        anim.SetTrigger("jump");
            yield return new WaitForSeconds(t);
        in_a_jump = false;

    }

    public override void ragdoll(RpcArgs args)//mrde preimenovat v player death pa izpisat ksno stvar playerjim. mogoce gor desno kdo je koga ubiu alk pa kej dunno.
    {
        GetComponent<UMA.Dynamics.UMAPhysicsAvatar>().ragdolled = true;
    }

    private void handle_dodge_start()
    {
        stats.inDodge = true;

        this.dodge_direction = 0;
        if (Input.GetAxis("Horizontal") < 0) this.dodge_direction = 1;
        else if (Input.GetAxis("Horizontal") > 0) this.dodge_direction = 2;
        else if (Input.GetAxis("Vertical") < 0) this.dodge_direction = 3;


        GetComponent<NetworkPlayerAnimationLogic>().handle_dodge_start(this.dodge_direction);
    }

    public void handleDodgeEnd() {//animacija poklice tole metodo na vsah clientih da resetirajo vse kar je povezano z dodganjem
        stats.inDodge = false;
        Debug.Log("dodge parameters cleared");
    }


    /* void OnCollisionEnter(Collision collision)
     {
         Debug.Log("GROUNDED!");
         if (collision.gameObject.tag != ("Player") && Physics.Raycast(transform.position, Vector3.down,0.4f))
         {
             isGrounded = true;
             anim.SetBool("grounded", true);
         }
     }
     // This function is a callback for when the collider is no longer in contact with a previously collided object.
     void OnCollisionExit(Collision collision)
     {
         Debug.Log("NOT GROUNDED!");
         if (collision.gameObject.tag != ("Player"))
         {
             isGrounded = false;
             anim.SetBool("grounded", false);
         }
     }

     void OnCollisionStay(Collision collisionInfo)
     {
         if (collisionInfo.collider.gameObject.tag != ("Player") && Physics.Raycast(transform.position, Vector3.down, 0.4f))
         {
             isGrounded = true;
             anim.SetBool("grounded", true);
         }
     }
     */

}