using UnityEngine;

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

    public float sprint_modifier = 2.0f;
    public float crouched_modifier = 0.25f;

    public float visina_skoka = 2.0f;

    private bool crouched; 

    private float speed = 1.0f;
    private Animator anim;
    public CapsuleCollider movement_collider_checker;


    public float dolzina_za_ground_check_raycast = 0.4f;
    private bool isGrounded = true;
    private bool in_a_jump = false;

    private Rigidbody rigidbody;

    private NetworkPlayerAnimationLogic animation_handler_script;
    
    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    { //CE DELAMO KAKSNE EMOTE MORAMO ZAKLENT OPCIJO DA SE LAHKO PLAYER PREMIKA!!! SICER SE ZJEBE GROUND DETECTION


        // If we are not the owner of this network object then we should
        // move this cube to the position/rotation dictated by the owner
        if (!networkObject.IsOwner)
        {
            transform.position = networkObject.position;
            transform.rotation = networkObject.rotation;
            return;
        }
        if (animation_handler_script == null) { animation_handler_script = GetComponent<NetworkPlayerAnimationLogic>(); }

        crouched = anim.GetBool("crouched");
        speed = normal_speed;

        if (crouched) speed = speed * crouched_modifier;


        Vector3 next_position = transform.position;

        Vector3 dirVector = (transform.forward * Input.GetAxis("Vertical") +transform.right * Input.GetAxis("Horizontal")).normalized * speed * Time.fixedDeltaTime;
        if (Input.GetButton("Sprint") && Input.GetAxis("Vertical") > 0)
            dirVector *= sprint_modifier;

        next_position +=dirVector ;

        Quaternion turnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * GetComponent<player_camera_handler>().mouse_sensitivity_multiplier, Vector3.up);

        transform.eulerAngles = transform.eulerAngles + turnAngle.eulerAngles;

        check_ground_raycast();

        //gravity
        //if(!isGrounded)
        rigidbody.AddForce(Vector3.up * Physics.gravity.y*2, ForceMode.Acceleration);
        

        if (Input.GetAxis("Jump")>0.01f && isGrounded && !in_a_jump) // && isGrounded??? isGrounded je trenutno se mal buggy
        {
            //jump();
            Debug.Log(Vector3.up * 6.3f);

            rigidbody.AddForce(Vector3.up * visina_skoka*2, ForceMode.VelocityChange);
            StartCoroutine(lock_jumping(1));
        }
        /*
        Vector3 point_on_ground = get_capsulecasted_position_downward_from_chest();
        int state_of_vertical=check_ground(point_on_ground);
        next_position=apply_gravity(next_position,point_on_ground,state_of_vertical);
        */

        rigidbody.MovePosition(next_position);
        //transform.position = next_position;
        networkObject.position = transform.position;
        networkObject.rotation = transform.rotation;
    }

    private void check_ground_raycast()
    {
        bool b = Physics.Raycast(transform.position, Vector3.down, dolzina_za_ground_check_raycast);

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