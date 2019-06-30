using UnityEngine;

using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System;
using System.Collections;

public class NetworkPlayerMovement : NetworkPlayerMovementBehavior
{

    public string horizontalInput = "Horizontal";
    public string verticallInput = "Vertical";

    [Header("Camera Settings")]
    public string rotateCameraXInput = "Mouse X";
    public string rotateCameraYInput = "Mouse Y";

    #region Layers
    [Header("---! Layers !---")]
    [Tooltip("Layers that the character can walk on")]
    public LayerMask groundLayer = 1 << 0;
    [Tooltip("Distance to became not grounded")]
    [SerializeField]
    protected float groundMinDistance = 0.2f;
    [SerializeField]
    protected float groundMaxDistance = 0.5f;
    #endregion

    #region Character Variables

    public enum LocomotionType
    {
        FreeWithStrafe,
        OnlyStrafe,
        OnlyFree
    }

    [Header("--- Locomotion Setup ---")]

    public LocomotionType locomotionType = LocomotionType.FreeWithStrafe;
    [Tooltip("lock the player movement")]
    public bool lockMovement;
    [Tooltip("Speed of the rotation on free directional movement")]
    [SerializeField]
    public float freeRotationSpeed = 10f;
    [Tooltip("Speed of the rotation while strafe movement")]
    public float strafeRotationSpeed = 10f;

    [Header("Jump Options")]

    [Tooltip("Check to control the character while jumping")]
    public bool jumpAirControl = true;
    [Tooltip("How much time the character will be jumping")]
    public float jumpTimer = 0.3f;
    [HideInInspector]
    public float jumpCounter;
    [Tooltip("Add Extra jump speed, based on your speed input the character will move forward")]
    public float jumpForward = 3f;
    [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
    public float jumpHeight = 4f;

    [Header("--- Movement Speed ---")]
    [Tooltip("Check to drive the character using RootMotion of the animation")]
    public bool useRootMotion = false;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float freeWalkSpeed = 2.5f;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float freeRunningSpeed = 3f;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float freeSprintSpeed = 4f;
    [Tooltip("Add extra speed for the strafe movement, keep this value at 0 if you want to use only root motion speed.")]
    public float strafeWalkSpeed = 2.5f;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float strafeRunningSpeed = 3f;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float strafeSprintSpeed = 4f;

    [Header("--- Grounded Setup ---")]

    [Tooltip("ADJUST IN PLAY MODE - Offset height limit for sters - GREY Raycast in front of the legs")]
    public float stepOffsetEnd = 0.45f;
    [Tooltip("ADJUST IN PLAY MODE - Offset height origin for sters, make sure to keep slight above the floor - GREY Raycast in front of the legs")]
    public float stepOffsetStart = 0.05f;
    [Tooltip("Higher value will result jittering on ramps, lower values will have difficulty on steps")]
    public float stepSmooth = 4f;
    [Tooltip("Max angle to walk")]
    [SerializeField]
    protected float slopeLimit = 45f;
    [Tooltip("Apply extra gravity when the character is not grounded")]
    [SerializeField]
    protected float extraGravity = -10f;
    protected float groundDistance;
    public RaycastHit groundHit;

    private NetworkPlayerAnimationLogic animation_handler_script;
    private NetworkPlayerCombatHandler combat_handler_script;
    private NetworkPlayerInventory networkPlayerInventory;
    private NetworkPlayerStats stats;
    #endregion


    #region Actions

    // movement bools
    [HideInInspector]
    public bool
        isGrounded,
        isStrafing,
        isSprinting,
        isSliding;

    // action bools
    [HideInInspector]
    public bool
        isJumping;

    protected void RemoveComponents()
    {
        if (_capsuleCollider != null) Destroy(_capsuleCollider);
        if (_rigidbody != null) Destroy(_rigidbody);
        if (animator != null) Destroy(animator);
        var comps = GetComponents<MonoBehaviour>();
        for (int i = 0; i < comps.Length; i++)
        {
            Destroy(comps[i]);
        }
    }

    #endregion

    #region Direction Variables
    [HideInInspector]
    public Vector3 targetDirection;
    protected Quaternion targetRotation;
    [HideInInspector]
    public Quaternion freeRotation;
    [HideInInspector]
    public bool keepDirection;

    #endregion

    #region Components               

    [HideInInspector]
    public Animator animator;                                   // access the Animator component
    [HideInInspector]
    public Rigidbody _rigidbody;                                // access the Rigidbody component
    [HideInInspector]
    public PhysicMaterial maxFrictionPhysics, frictionPhysics, slippyPhysics;       // create PhysicMaterial for the Rigidbody
    [HideInInspector]
    public CapsuleCollider _capsuleCollider;                    // access CapsuleCollider information

    #endregion

    #region Hide Variables

    [HideInInspector]
    public float colliderHeight;                        // storage capsule collider extra information                
    [HideInInspector]
    public Vector2 input;                               // generate input for the controller        
    [HideInInspector]
    public float speed, direction, verticalVelocity;    // general variables to the locomotion
    [HideInInspector]
    public float velocity;                              // velocity to apply to rigidbody  
    
    private bool isCrouched;

    #endregion

    protected virtual void Start()
    {
        // access components
        animator = GetComponent<Animator>();

        // slides the character through walls and edges
        frictionPhysics = new PhysicMaterial();
        frictionPhysics.name = "frictionPhysics";
        frictionPhysics.staticFriction = .25f;
        frictionPhysics.dynamicFriction = .25f;
        frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

        // prevents the collider from slipping on ramps
        maxFrictionPhysics = new PhysicMaterial();
        maxFrictionPhysics.name = "maxFrictionPhysics";
        maxFrictionPhysics.staticFriction = 1f;
        maxFrictionPhysics.dynamicFriction = 1f;
        maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;

        // air physics 
        slippyPhysics = new PhysicMaterial();
        slippyPhysics.name = "slippyPhysics";
        slippyPhysics.staticFriction = 0f;
        slippyPhysics.dynamicFriction = 0f;
        slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;

        // rigidbody info
        _rigidbody = GetComponent<Rigidbody>();

        // capsule collider info
        _capsuleCollider = GetComponent<CapsuleCollider>();

        combat_handler_script = GetComponent<NetworkPlayerCombatHandler>();
        networkPlayerInventory = GetComponent<NetworkPlayerInventory>();
        stats = GetComponent<NetworkPlayerStats>();
    }

    protected virtual void LateUpdate()
    {		    
        InputHandle();                      // update input methods
        UpdateCameraStates();               // update camera states
    }

    protected virtual void FixedUpdate()
    {
        AirControl();
        //CameraInput();
    }

    protected virtual void Update()
    {
        UpdateMotor();                   // call ThirdPersonMotor methods               
        UpdateAnimator();                // call ThirdPersonAnimator methods		               
    }

    protected virtual void UpdateCameraStates()
    {
        /*
        // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData
        if (tpCamera == null)
        {
            tpCamera = FindObjectOfType<vThirdPersonCamera>();
            if (tpCamera == null)
                return;
            if (tpCamera)
            {
                tpCamera.SetMainTarget(this.transform);
                tpCamera.Init();
            }
        }*/
    }

    protected virtual void InputHandle()
    {
        ExitGameInput();
        //CameraInput();

        if (!lockMovement)
        {
            MoveCharacter();
            SprintInput();
            CrouchedInput();

            JumpInput();
        }
    }

    public virtual void UpdateMotor()
    {
        CheckGround();
        ControlJumpBehaviour();
        ControlLocomotion();
    }

    protected virtual void ExitGameInput()
    {
        // just a example to quit the application 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Cursor.visible)
                Cursor.visible = true;
            else
                Application.Quit();
        }
    }

    protected virtual void MoveCharacter()
    {
        input.x = Input.GetAxis(horizontalInput);
        input.y = Input.GetAxis(verticallInput);
    }


    protected virtual void SprintInput()
    {
        if (Input.GetButtonDown("Sprint"))
            isSprinting=true;
        else if (Input.GetButtonUp("Sprint"))
            isSprinting = false;
    }
    protected virtual void CrouchedInput()
    {
        if (Input.GetButtonDown("Crouch"))
            isCrouched = true;
        else if (Input.GetButtonUp("Crouch"))
            isCrouched = false;
    }

    protected virtual void JumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            // conditions to do this action
            bool jumpConditions = isGrounded && !isJumping;
            // return if jumpCondigions is false
            if (!jumpConditions) return;
            // trigger jump behaviour
            jumpCounter = jumpTimer;
            isJumping = true;
            // trigger jump animations            
            if (_rigidbody.velocity.magnitude < 1)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", 0.2f);
        }
    }

    public void AirControl()
    {
        if (isGrounded) return;
        if (!jumpFwdCondition) return;

        var velY = transform.forward * jumpForward * speed;
        velY.y = _rigidbody.velocity.y;
        var velX = transform.right * jumpForward * direction;
        velX.x = _rigidbody.velocity.x;

        if (jumpAirControl)
        {
                var vel = transform.forward * (jumpForward * speed);
                _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
        }
        else
        {
            var vel = transform.forward * (jumpForward);
            _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
        }
    }

    protected bool jumpFwdCondition
    {
        get
        {
            Vector3 p1 = transform.position + _capsuleCollider.center + Vector3.up * -_capsuleCollider.height * 0.5F;
            Vector3 p2 = p1 + Vector3.up * _capsuleCollider.height;
            return Physics.CapsuleCastAll(p1, p2, _capsuleCollider.radius * 0.5f, transform.forward, 0.6f, groundLayer).Length == 0;
        }
    }

    public virtual void UpdateAnimator()
    {
        if (animator == null || !animator.enabled) return;

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("crouched", isCrouched);
        animator.SetFloat("GroundDistance", groundDistance);

        //if (!isGrounded)
        animator.SetFloat("walking_vertical", verticalVelocity);
        animator.SetFloat("walking_horizontal", verticalVelocity);

        // fre movement get the input 0 to 1
        animator.SetFloat("InputVertical", speed, 0.1f, Time.deltaTime);
    }

}