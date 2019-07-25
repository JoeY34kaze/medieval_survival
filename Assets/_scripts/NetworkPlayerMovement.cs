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
    public bool test1 = true;
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



    [Header("--- Locomotion Setup ---")]

    private bool isDodging;
    private Vector3 dodgeVector;

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

    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float freeWalkSpeed = 2.5f;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float freeRunningSpeed = 3f;
    [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
    public float freeCrouchedSpeed = 0.5f;
    [Tooltip("Speed of the dodge movement.")]
    public float dodgeSpeed = 5f;

    [Header("--- Grounded Setup ---")]

    [Tooltip("ADJUST IN PLAY MODE - Offset height limit for stairs - GREY Raycast in front of the legs")]
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
    private float currentGroundAngle=0f;

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
        isSliding;

    // action bools
    //[HideInInspector]
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
    private int dodge_direction;
    private object tpCamera;
    private NetworkPlayerAnimationLogic animation_logic_script;
    private Vector3 sliding_velocity;

    private UILogic uiLogic;
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
        animation_logic_script = GetComponent<NetworkPlayerAnimationLogic>();
        this.uiLogic = GetComponentInChildren<UILogic>();
    }

    protected virtual void LateUpdate()
    {
        if (networkObject.IsOwner)
        {
            InputHandle();                      // update input methods
                                                //UpdateCameraStates();               // update camera states
        }
    }

    protected virtual void FixedUpdate()
    {
        if (networkObject.IsOwner)
        {
            AirControl();

            CameraInput();
        }
    }

    private bool CameraRotationAllowed()
    {
        return !uiLogic.hasOpenWindow;
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

            if (!_rigidbody.isKinematic) _rigidbody.isKinematic = true; //clipping issues
            if (_rigidbody.detectCollisions) _rigidbody.detectCollisions = false;
            return;
        }
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

    protected virtual void CameraInput()
    {
        if (!CameraRotationAllowed()) return;
        var X = Input.GetAxis(rotateCameraXInput);
        Quaternion turnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * GetComponent<player_camera_handler>().mouse_sensitivity_multiplier, Vector3.up);
        transform.eulerAngles = transform.eulerAngles + turnAngle.eulerAngles;


    }

    protected virtual void InputHandle()
    {
        //CameraInput();

        if (!lockMovement && !stats.downed)
        {
            MoveCharacter();
            SprintInput();
            CrouchedInput();
            JumpInput();
            DodgeInput();
        }
    }


    public virtual void UpdateMotor()
    {
       // Debug.Log("motor");
       
        CheckGround();
        ControlJumpBehaviour();
        ControlLocomotion();
        ControlSpeed();


    }


    protected virtual void MoveCharacter()
    {
        input.x = Input.GetAxis(horizontalInput);
        input.y = Input.GetAxis(verticallInput);
    }

    #region Input

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
            this.jump_vector_start = transform.TransformVector(_rigidbody.velocity);


            // trigger jump animations
            animation_logic_script.handle_start_of_jump_owner();

            //if (_rigidbody.velocity.magnitude < 1)
            //animator.CrossFadeInFixedTime("Jump", 0.1f);
            //else
            //animator.CrossFadeInFixedTime("JumpMove", 0.2f);
        }
    }
    private void DodgeInput()
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
        CheckGroundDistance();

        currentGroundAngle = GroundAngle();

        // change the physics material to very slip when not grounded or maxFriction when is
        if (isGrounded && input == Vector2.zero)
            _capsuleCollider.material = maxFrictionPhysics;
        else if (isGrounded && input != Vector2.zero)
            _capsuleCollider.material = frictionPhysics;
        else
            _capsuleCollider.material = slippyPhysics;

        var magVel = (float)System.Math.Round(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z).magnitude, 2);
        magVel = Mathf.Clamp(magVel, 0, 1);

        var groundCheckDistance = groundMinDistance;
        if (magVel > 0.25f) groundCheckDistance = groundMaxDistance;

        // clear the checkground to free the character to attack on air                
        var onStep = StepOffset();

        //Debug.Log("ground dist: "+groundDistance);

        if (groundDistance <= 0.15f)
        {
            isGrounded = true;
            //if(test1)Sliding();
        }
        else
        {
            if (groundDistance >= groundCheckDistance)
            {
                isGrounded = false;
                // check vertical velocity
                verticalVelocity = _rigidbody.velocity.y;
                // apply extra gravity when falling
                if (!onStep && !isJumping)
                    _rigidbody.AddForce(transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
            }
            else if (!onStep && !isJumping)
            {
                _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
            }
        }
    }

    void CheckGroundDistance()
    {
        if (_capsuleCollider != null)
        {
            // radius of the SphereCast
            float radius = _capsuleCollider.radius * 0.9f;
            var dist = 10f;
            // position of the SphereCast origin starting at the base of the capsule
            Vector3 pos = transform.position + Vector3.up * (_capsuleCollider.radius);
            // ray for RayCast
            Ray ray1 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
            // ray for SphereCast
            Ray ray2 = new Ray(pos, -Vector3.up);
            // raycast for check the ground distance
            if (Physics.Raycast(ray1, out groundHit, colliderHeight / 2 + 2f, groundLayer))
                dist = transform.position.y - groundHit.point.y;
            // sphere cast around the base of the capsule to check the ground distance
            if (Physics.SphereCast(ray2, radius, out groundHit, _capsuleCollider.radius + 2f, groundLayer))
            {
                // check if sphereCast distance is small than the ray cast distance
                if (dist > (groundHit.distance - _capsuleCollider.radius * 0.1f))
                    dist = (groundHit.distance - _capsuleCollider.radius * 0.1f);
            }
            groundDistance = (float)System.Math.Round(dist, 2);
        }
    }

    float GroundAngle()
    {
        float groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
       
        return groundAngle;
    }

    void Sliding()
    {
        var onStep = StepOffset();
        var groundAngleTwo = 0f;
        RaycastHit hitinfo;
        Ray ray = new Ray(transform.position, -transform.up);

        if (Physics.Raycast(ray, out hitinfo, 1f, groundLayer))
        {
            groundAngleTwo = Vector3.Angle(Vector3.up, hitinfo.normal);
        }
        
        //Debug.Log("ground angle : " + currentGroundAngle + " groundAngle2 :"+groundAngleTwo+ " GroundDistance: "+groundDistance);

         if (GroundAngle() > slopeLimit  && GroundAngle() <= 85 &&
             groundAngleTwo > slopeLimit && groundAngleTwo <= 85 &&
             groundDistance <= 0.05f && !onStep)
        //if (currentGroundAngle > slopeLimit &&      groundDistance <= 0.15f && !onStep)
        {
            isSliding = true;
            isGrounded = false;
            var slideVelocity = (currentGroundAngle - slopeLimit) * 2f;
            slideVelocity = Mathf.Clamp(slideVelocity, 0, 10);
            sliding_velocity = new Vector3(_rigidbody.velocity.x, -slideVelocity, _rigidbody.velocity.z);//doda v premikanje znotraj speddControl
            _rigidbody.velocity = sliding_velocity;
        }
        else
        {
            isSliding = false;
            isGrounded = true;
        }
    }

    bool StepOffset()
    {
        if (input.sqrMagnitude < 0.1 || !isGrounded) return false;

        var _hit = new RaycastHit();
        var _movementDirection = isStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.y).normalized : transform.forward;
        Ray rayStep = new Ray((transform.position + new Vector3(0, stepOffsetEnd, 0) + _movementDirection * ((_capsuleCollider).radius + 0.05f)), Vector3.down);

        if (Physics.Raycast(rayStep, out _hit, stepOffsetEnd - stepOffsetStart, groundLayer) && !_hit.collider.isTrigger)
        {
            if (_hit.point.y >= (transform.position.y) && _hit.point.y <= (transform.position.y + stepOffsetEnd))
            {
                var _speed = isStrafing ? Mathf.Clamp(input.magnitude, 0, 1) : speed;
                var velocityDirection = isStrafing ? (_hit.point - transform.position) : (_hit.point - transform.position).normalized;
                _rigidbody.velocity = velocityDirection * stepSmooth * (_speed * (velocity > 1 ? velocity : 1));
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Jump Methods

    protected void ControlJumpBehaviour()//klice se ob pritisku space
    {
        if (!isJumping) return;

        jumpCounter -= Time.deltaTime;
        if (jumpCounter <= 0)
        {
            jumpCounter = 0;
            isJumping = false;
        }
        // apply extra force to the jump height   
        var vel = _rigidbody.velocity;
        vel.y = jumpHeight;
        _rigidbody.velocity = vel;
    }

    public void AirControl()
    {
        if (isGrounded) return;
        if (!jumpFwdCondition) return;//??

        //var velY = transform.forward * jumpForward * speed;
        //velY.y = _rigidbody.velocity.y;
        //var velX = transform.right * jumpForward * direction;
        //velX.x = _rigidbody.velocity.x;

        //if (jumpAirControl)
        //{
        //    var vel = transform.forward * (jumpForward * speed);
        //    _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
        //}
        //else
        //{
        //var vel = _rigidbody.velocity;//transform.forward * (jumpForward);
        //    _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
        //}
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


    #endregion

    #region Locomotion 


    void ControlLocomotion()
    {
            FreeMovement();     // free directional movement
    }

    void StrafeMovement()
    {
        var _speed = Mathf.Clamp(input.y, -1f, 1f);
        var _direction = Mathf.Clamp(input.x, -1f, 1f);
        speed = _speed;
        direction = _direction;
        if (isSprinting) speed += 0.5f;
        if (direction >= 0.7 || direction <= -0.7 || speed <= 0.1) isSprinting = false;
    }

    public virtual void FreeMovement()
    {
        if (input != Vector2.zero && targetDirection.magnitude > 0.1f &&!isDodging)
        {
            Vector3 lookDirection = targetDirection.normalized;
            freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
            var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
            var eulerY = transform.eulerAngles.y;

            // apply free directional rotation while not turning180 animations
            if (isGrounded || (!isGrounded && jumpAirControl))
            {
                if (diferenceRotation < 0 || diferenceRotation > 0) eulerY = freeRotation.eulerAngles.y;
                var euler = new Vector3(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), freeRotationSpeed * Time.deltaTime);
            }
        }

    }
    protected void ControlSpeed()
    {
        //Debug.Log("control_speed");
        if (Time.deltaTime == 0) return;

        if (!isDodging)
        {
            // set speed to both vertical and horizontal inputs
            speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
            speed = Mathf.Clamp(speed, 0, 1f);

            float new_speed = speed * freeWalkSpeed;

            if (isSprinting) new_speed = speed * freeRunningSpeed;
            if (isCrouched) new_speed = speed * freeCrouchedSpeed;

            Vector3 velY = transform.forward * velocity * speed;

            velY.y = _rigidbody.velocity.y;

            Vector3 v = (transform.TransformDirection(new Vector3(input.x, 0, input.y)));
            //Debug.Log(  Vector3.Angle(transform.forward, v));
            float angle_from_forward = Vector3.Angle(transform.forward, v);
            float angle_penalty = 1.0f;
            if (!isCrouched)
            {
                if (angle_from_forward >= 0 && angle_from_forward < 60)
                {//diagonala naprej
                    angle_penalty = 0.85f;
                }
                else if (angle_from_forward >= 60 && angle_from_forward < 110)
                {
                    angle_penalty = 0.75f;
                }
                else if (angle_from_forward >= 110)
                {
                    angle_penalty = 0.5f;
                }
            }
            v = Vector3.Normalize(v) * new_speed * angle_penalty;
            v.y = _rigidbody.velocity.y;

            //_rigidbody.velocity = velY;
            //_rigidbody.AddForce(v * Time.deltaTime, ForceMode.VelocityChange);
            
            if (GroundAngle()<=slopeLimit && GetAngle_MovementVector_and_groundNormal(v) < (90 + slopeLimit))//prva preveri kje stojimo, druga preveri ce se premikamo v hrib al ce gremo dol
                _rigidbody.velocity = v;
            else 
                _rigidbody.velocity = new Vector3(0, extraGravity, 0);


            // Vector3 prejsnje_stanje = _rigidbody.velocity;
            //   if (currentGroundAngle >= slopeLimit)//TODO: pogledat v ktero smer se premikamo in ce se premikamo dol po slopu mu omogocmo spreminjanje velocity-a
            //   {
            //       v.x = 0;
            //       v.z = 0;

                //       prejsnje_stanje.x = 0;
                //       prejsnje_stanje.z = 0;
                //   }

                //  if(isSliding)
                //      v = v + sliding_velocity;

                //_rigidbody.velocity = Vector3.Lerp(prejsnje_stanje, v, 20f * Time.deltaTime);
        }
        else {//is dodging
            //Debug.Log("dodging - controlSpeed");
            float new_speed = dodgeSpeed;

            //Vector3 velY = transform.forward * velocity * speed;

            //velY.y = _rigidbody.velocity.y;

            Vector3 v = dodgeVector;
            //Debug.Log(  Vector3.Angle(transform.forward, v));
            v =v * new_speed;
            v.y = _rigidbody.velocity.y;


            if ( GetAngle_MovementVector_and_groundNormal(v) < (90 + slopeLimit))//preveri ce se premikamo v hrib al ce gremo dol
                _rigidbody.velocity = v;
            else
                _rigidbody.velocity = new Vector3(0, extraGravity, 0);

            //if (isSliding)
            //   v = v + sliding_velocity;
            //_rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);

        }
        //}
       // else {
            //Vector3 v = transform.InverseTransformVector(this.jump_vector_start);

           // Debug.Log(this.jump_vector_start+"  ||  "+v+" -- NOT GROUNDED");

           // v = Vector3.Normalize(v) * new_speed;
            
            //v.y = _rigidbody.velocity.y;
            //_rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);
       // }
      //  }
      //  else
      //  {
        //    _rigidbody.velocity = velY;
       //     _rigidbody.AddForce(transform.forward * (velocity * speed) * Time.deltaTime, ForceMode.VelocityChange);
      //  }
        
    }

    /// <summary>
    /// raycastej z sredine collkiderja dol. dobis tocko na terenu/objektu, dobis normalo, zracunas kot med normalo in vektorjem premikanja, ce je manjsi od 90 gremo dol, sicer gremo po hribu gor
    /// </summary>
    /// <returns></returns>
    private float GetAngle_MovementVector_and_groundNormal(Vector3 v) {
        Vector3 movement_direction = new Vector3(v.x,0,v.z).normalized;
        RaycastHit hitinfo;
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out hitinfo, 1f, groundLayer))
        {
            //groundAngleTwo = Vector3.Angle(Vector3.up, hitinfo.normal);
            float angle = Vector3.Angle(movement_direction, hitinfo.normal.normalized);
            //Debug.Log("check downhill : " + angle);

            Debug.DrawRay(transform.position,movement_direction, Color.black);
            Debug.DrawRay(transform.position,- transform.up, Color.blue);
            Debug.DrawRay(hitinfo.point,hitinfo.normal, Color.red);

            return angle;
        }
        Debug.LogWarning("Couldnt get data for ground slope");
        return 0;
    }


    #endregion

    #region Dodge



    public override void setDodge(RpcArgs args)//mrde preimenovat v player death pa izpisat ksno stvar playerjim. mogoce gor desno kdo je koga ubiu alk pa kej dunno.
    {
        int smer = args.GetNext<int>();

        /*
         if(networkObject.isServer){
        //anticheat al pa kej. glede na visino od tal pa take fore 
        }
         */


        //stats.inDodge = true;
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
        //stats.inDodge = false;
        Debug.Log("Dodge end");
        isDodging = false;
        //if(networkObject.IsOwner)
            //lockMovement = false;
        //Debug.Log("dodge parameters cleared");
        GetComponent<NetworkPlayerAnimationLogic>().handle_dodge_end();
    }

    #endregion
}