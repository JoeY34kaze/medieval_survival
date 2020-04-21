using UnityEngine;

using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;
using System.Collections;
using BeardedManStudios.Forge.Networking.Unity;
using static PlayerManager;

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
    
    private NetworkPlayerStats stats;
    public Transform camera_frame;
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
    
    internal bool isCrouched;
    private int dodge_direction;
    private NetworkPlayerAnimationLogic animation_logic_script;

    public float horizontal_angle_offset_multiplier=2f;

    #endregion

    private CharacterController controller;

    protected virtual void Start()
    {
        // access components
        animator = GetComponent<Animator>();
        stats = GetComponent<NetworkPlayerStats>();
        this.controller = GetComponent<CharacterController>();

    }

    /// <summary>
    /// KO SE DATA NALOZI MORAMO NEKAK PAMETNO POSKRBET DA SE STVARI APPLAYAjo
    /// </summary>
    internal void OnPlayerDataLoaded()//tole imamo namest networkSTart ker morajo nekatere skripe bit nalozene v zaporedju in je samo onnetworkStart za vsako pa yolo
    {
        Debug.Log("Applying movement.");
        if (networkObject.IsServer)
        {
            networkObject.SendRpc(RPC_SERVER_UPDATE_FROM_SAVED_DATA, Receivers.All, current_gravity_velocity.y, transform.position, transform.rotation);
        }
    }


    protected virtual void LateUpdate()
    {
        if (networkObject.IsOwner)
        {
            if(!UILogic.hasControlOfInput)InputHandle();                      // update input methods
                    
        }
    }

    protected virtual void FixedUpdate()
    {
        if (networkObject.IsOwner)
        {
            if (!UILogic.hasControlOfInput) Rotate_character_horizontally();
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
            if(!stats.dead && !UILogic.hasControlOfInput ){ 
                UpdateMotor();                   // call ThirdPersonMotor methods               
                UpdateAnimator();                // call ThirdPersonAnimator methods	
            }
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
        float rotation = Input.GetAxis("Mouse X") * Prefs.mouse_sensitivity;
        rotation *=  (1+(camera_frame.transform.localRotation.x * 1 / 0.7f) * horizontal_angle_offset_multiplier);
        Quaternion turnAngle = Quaternion.AngleAxis(rotation, Vector3.up);
        transform.eulerAngles = transform.eulerAngles + turnAngle.eulerAngles;


    }

    protected virtual void InputHandle()
    {
        if (!stats.downed && !stats.dead)//ce je downed je treba clearat movement sicer se se vedno vozi naprej
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
        if(!stats.downed)ControlMovement();
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
        if (Input.GetButtonDown("Dodge") && isGrounded && !isDodging)
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

    #region synchronization


    /// <summary>
    /// received by everyone. updates object to data from rpc
    /// </summary>
    /// <param name="args"></param>
    public override void ServerUpdateFromSavedData(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            this.current_gravity_velocity.y = args.GetNext<float>();//na vseh razen ownerju se zavrze
            this.transform.position = args.GetNext<Vector3>();
            this.transform.rotation = args.GetNext<Quaternion>();
        }
    }

    internal void OnRemotePlayerDataSet()
    {
        //throw new NotImplementedException();
        //nevem kaj bi blo sploh za pohandlat zaenkrat. crouches se nastima v metodi zunej te
    }


    #endregion
}