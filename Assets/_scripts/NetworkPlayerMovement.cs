using UnityEngine;

using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;
using System.Collections;


/// <summary>
/// INVECTOR FREE ASSET
/// </summary>
public class NetworkPlayerMovement : NetworkPlayerMovementBehavior
{
    public Transform groundCheckPosition;
    [Tooltip("radius of spherecast.")]
    public float groundDistance = 0.3f;
    [Tooltip("which layers should register player as being grounded on.")]
    public LayerMask groundMask;

    public float jump_velocity;

    public string horizontalInput = "Horizontal";
    public string verticallInput = "Vertical";
    [Header("Camera Settings")]
    public string rotateCameraXInput = "Mouse X";
    #region Character Variables
    [Header("--- Locomotion Setup ---")]
    private bool isDodging;
    private Vector3 dodgeVector;
    [Tooltip("lock the player movement")]
    public bool lockMovement;
    [Header("--- Movement Speed ---")]
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float walk_speed = 2.5f;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float sprint_speed = 3f;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float crouched_speed = 0.5f;
    [Tooltip("Speed of the dodge movement.")]
    public float dodge_speed = 3f;
    [Header("--- Grounded Setup ---")]
    [SerializeField]
    protected float gravity = -10f;
    

    private NetworkPlayerAnimationLogic animation_handler_script;
    private NetworkPlayerCombatHandler combat_handler_script;
    private NetworkPlayerInventory networkPlayerInventory;
    private NetworkPlayerStats stats;

    private Vector3 jump_vector_start;
    #endregion


    #region Actions

    // movement bools
    //[HideInInspector]
    public bool
        isGrounded,
        isStrafing,
        isSprinting,
        isSliding,
        isJumping;



    #endregion
    #region Components               

    [HideInInspector]
    public Animator animator;                                   // access the Animator component
    #endregion

    #region Hide Variables       
    [HideInInspector]
    public float speed;    // herizontalno premikanje
    public Vector3 direction;
    public Vector3 current_gravity_velocity;
    
    private bool isCrouched;
    private int dodge_direction;
    private NetworkPlayerAnimationLogic animation_logic_script;

    private UILogic uiLogic;
    #endregion

    private CharacterController controller;
    protected virtual void Start()
    {
        // access components
        animator = GetComponent<Animator>();
        combat_handler_script = GetComponent<NetworkPlayerCombatHandler>();
        networkPlayerInventory = GetComponent<NetworkPlayerInventory>();
        stats = GetComponent<NetworkPlayerStats>();
        animation_logic_script = GetComponent<NetworkPlayerAnimationLogic>();
        this.controller = GetComponent<CharacterController>();
    }

    protected virtual void LateUpdate()
    {
        if (networkObject.IsOwner)
        {
            InputHandle();                      // update input methods
                                              
        }
    }

    protected virtual void FixedUpdate()
    {
        if (networkObject.IsOwner)
        {
            AirControl();
            Rotate_character_horizontally();
        }
    }

    private bool isCameraRotationAllowed()
    {
        return !UILogic.Instance.hasOpenWindow;
    }

    protected virtual void Update()
    {
        if (networkObject.IsOwner)
        {
            UpdateMotor();                   // call ThirdPersonMotor methods               
            UpdateAnimator();                // call ThirdPersonAnimator methods	
            networkObject.position = transform.position;
            networkObject.rotation = transform.rotation;
        }
        else {
            transform.position = networkObject.position;
            transform.rotation = networkObject.rotation;

            return;
        }
    }
    protected virtual void Rotate_character_horizontally()
    {
        if (!isCameraRotationAllowed()) return;
        var X = Input.GetAxis(rotateCameraXInput);
        Quaternion turnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * GetComponent<player_camera_handler>().mouse_sensitivity_multiplier, Vector3.up);
        transform.eulerAngles = transform.eulerAngles + turnAngle.eulerAngles;


    }

    protected virtual void InputHandle()
    {
        if (!lockMovement && !stats.downed)//ce je downed je treba clearat movement sicer se se vedno vozi naprej
        {
            SetAxisInput();
            SetSprintInput();
            SetCrouchedInput();
            SetJumpInput();
            SetDodgeInput();
        }
        else if (stats.downed) {//mora bit pri miru oziroma mogoce padat na tla/slidat se. ni pa nujno
            this.direction = Vector3.zero;
            this.speed = 0;
        }
    }


    public virtual void UpdateMotor()
    {
       // na tem mestu so VSE spremenjivke nastavljene za smer / hitrost / stanje playerja
       
        CheckGround();
        
        ControlJumpBehaviour();
        ControlMovement();
        ControlGravity();
    }


    protected virtual void SetAxisInput()
    {
        float x = Input.GetAxis(horizontalInput);
        float y = Input.GetAxis(verticallInput);

        this.direction = Vector3.Normalize(transform.forward * y + transform.right * x);
        this.speed = this.walk_speed;
    }

    #region Input

    protected virtual void SetSprintInput()
    {
        if (Input.GetButtonDown("Sprint"))
            isSprinting=true;
        else if (Input.GetButtonUp("Sprint"))
            isSprinting = false;
        if (this.isSprinting)
            this.speed = this.sprint_speed;
    }
    protected virtual void SetCrouchedInput()
    {
        if (Input.GetButtonDown("Crouch"))
            isCrouched = true;
        else if (Input.GetButtonUp("Crouch"))
            isCrouched = false;
        if (this.isCrouched) this.speed = this.crouched_speed;
    }

    protected virtual void SetJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            // conditions to do this action
            bool jumpConditions = isGrounded && !isJumping && !isCrouched;
            // return if jumpCondigions is false
            if (!jumpConditions) return;
            // trigger jump behaviour
            this.current_gravity_velocity.y =  this.jump_velocity;
            this.isJumping = true;

        }
    }
    private void SetDodgeInput()
    {
        if (Input.GetButtonDown("Dodge") && isGrounded && !isDodging && !lockMovement)
        {
            handle_dodge_start();
        }
    }

#endregion

    public virtual void UpdateAnimator()
    {
        if (animation_logic_script == null) animation_logic_script = GetComponent<NetworkPlayerAnimationLogic>();
        animation_logic_script.setJump(!isGrounded);
        animation_logic_script.setCrouched(isCrouched);
       
    }


    #region Ground Check

    void CheckGround()
    {
        if (this.current_gravity_velocity.y < 0)
        {
            this.isGrounded = Physics.CheckSphere(this.groundCheckPosition.position, this.groundDistance, groundMask);
            if (this.isGrounded) this.isJumping = false;
            if (this.isGrounded && this.current_gravity_velocity.y < 0) this.current_gravity_velocity.y = -1f;
        }
        else {
            this.isGrounded = false;
        }
    }


    #endregion

    #region Jump Methods

    protected void ControlJumpBehaviour()
    {

    }

    public void AirControl()
    {

    }

    #endregion

    #region Locomotion 

    void ControlGravity() {
        this.current_gravity_velocity.y += this.gravity * Time.deltaTime;
        controller.Move(this.current_gravity_velocity * Time.deltaTime);
    }
    void ControlMovement()
    {

           controller.Move(this.direction * this.speed * Time.deltaTime);  
    }

    #endregion

    #region Dodge



    public override void setDodge(RpcArgs args)//mrde preimenovat v player death pa izpisat ksno stvar playerjim. mogoce gor desno kdo je koga ubiu alk pa kej dunno.
    {
        int smer = args.GetNext<int>();
        isDodging = true;
        this.dodge_direction = smer;
        GetComponent<NetworkPlayerAnimationLogic>().handle_dodge_start(this.dodge_direction);
    }

    private void handle_dodge_start()
    {
        Debug.Log("Dodge start..");
        isDodging = true;
        //lockMovement = true;
        this.dodge_direction = 0;
        if (Input.GetAxis("Horizontal") < 0) this.dodge_direction = 1;//pretvort na karkoli ze ta skripta uporabla
        else if (Input.GetAxis("Horizontal") > 0) this.dodge_direction = 2;//pretvort na karkoli ze ta skripta uporabla
        else if (Input.GetAxis("Vertical") < 0) this.dodge_direction = 3;//pretvort na karkoli ze ta skripta uporabla
        GetComponent<NetworkPlayerAnimationLogic>().handle_dodge_start(this.dodge_direction);

        dodgeVector = transform.forward;
        switch (dodge_direction) {//to damo u control speed da ga mece v to smer
            case 1: dodgeVector = -transform.right;
                break;
            case 2:
                dodgeVector = transform.right;
                break;
            case 3:
                dodgeVector = -transform.forward;
                break;
            default:
                break;
        }

        networkObject.SendRpc(RPC_SET_DODGE, Receivers.OthersProximity, this.dodge_direction);
    }

    public void handleDodgeEnd()
    {//animacija poklice tole metodo na vsah clientih da resetirajo vse kar je povezano z dodganjem
        Debug.Log("Dodge end");
        isDodging = false;
        GetComponent<NetworkPlayerAnimationLogic>().handle_dodge_end();
    }

    #endregion
}