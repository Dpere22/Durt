using System;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    //Animation vars
    private static readonly int XVelocity = Animator.StringToHash("xVelocity");
    private static readonly int IsClimbing = Animator.StringToHash("isClimbing");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    public GameObject body;
    private Animator _anim;

    [FormerlySerializedAs("Settings")] [Header("Dependencies")]
    public PlayerMovementSettings settings;
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private Collider2D feetCollider;
    [SerializeField] private SpriteRenderer armRenderer;

    private Rigidbody2D _rigidBody;
    
    //respawn vars
    public Vector2 respawnPoint;

    //Movement vars
    private Vector2 _velocity;
    private bool _isFacingRight;

    //Collision vars
    private RaycastHit2D _groundRaycast;
    private RaycastHit2D _headRayCast;
    private bool _grounded;
    private bool _headCollided;

    //Jump vars
    private float VerticalVelocity {  get; set; }
    [SerializeField]
    public bool isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    //apex cars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;


    //Jump buffer vars
    private float _jumpBufferTimer;
    private bool _jumpReleaseDuringBuffer;

    //coyote time vars
    private float _coyoteTimer;

    //interaction vars
    public bool isClimbing;

    //Player Made Sound
    [SerializeField] AudioSource playerWalking;

    //Turning stuff
    private float _turnTimer;
    private readonly float _minTimeBetweenTurns = 0.1f;

    /// <summary>
    /// Initializes movement variables and rigidBody component reference.
    /// </summary>
    private void Awake()
    {
        _isFacingRight = true;
        _rigidBody = GetComponent<Rigidbody2D>();
        respawnPoint = _rigidBody.transform.position;

    }

    void Start()
    {
        _anim = body.GetComponent<Animator>();
        // _rigidBody = GetComponent<Rigidbody2D>();
        // respawnPoint = _rigidBody.transform;
    }

    private void OnEnable()
    {
        Actions.TurnPlayer += TurnPlayerBasedOnCursor;
    }

    private void OnDisable()
    {
        Actions.TurnPlayer -= TurnPlayerBasedOnCursor;
    }

    /// <summary>
    /// Handles collision checks, movement, and jumping mechanics each physics step.
    /// </summary>
    private void FixedUpdate()
    {
        _anim.SetBool(IsClimbing, isClimbing);
        _anim.SetFloat(XVelocity, Math.Abs(_velocity.x));
        CollisionChecks();
        Jump();

        if (_grounded) 
        {
            Move(settings.GroundAcceleration, settings.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(settings.AirAcceleration, settings.AirDeceleration, InputManager.Movement);
        }

    }

    /// <summary>
    /// Handles non-physics-related updates like jump buffering and input handling.
    /// </summary>
    private void Update()
    {
        _turnTimer += Time.deltaTime;
        CountTimers();
        JumpChecks();
        _anim.SetBool(IsJumping, !_grounded);
        ClimbingAnimChecks();
    }

    public void Respawn()
    {
        _rigidBody.transform.position = respawnPoint;
    }

    public void SetRespawnPoint()
    {
        respawnPoint = _rigidBody.transform.position;
    }

    private void ClimbingAnimChecks()
    {
        if (PauseScreen.IsPaused)
        {
            playerWalking.Stop();
        }

        switch (isClimbing)
        {
            case true when armRenderer.enabled:
                armRenderer.enabled = false;
                break;
            case false when _grounded:
            case false when !_grounded:
                armRenderer.enabled = true;
                break;
        }
    }
    /// <summary>
    /// Draws visual representations of jump arcs in the scene view for debugging purposes.
    /// </summary>
    private void OnDrawGizmos()
    {
        if(settings.ShowWalkJumpArc)
        {
            DrawJumpArc(settings.MaxWalkSpeed, Color.yellow);
        }

        if(settings.ShowRunJumpArc)
        {
            DrawJumpArc(settings.MaxRunSpeed, Color.magenta);
        }
    }

    private void TurnPlayerBasedOnCursor(bool turnPlayer)
    {
        if (isClimbing) return;
        if (!(_turnTimer > _minTimeBetweenTurns)) return;
        
        _turnTimer = 0;
        if (!turnPlayer)
        {
            return;
        }

        Turn(!_isFacingRight);
    }

    /// <summary>
    /// Moves the player horizontally based on input, applying acceleration and deceleration.
    /// </summary>
    /// <param name="acceleration">The rate of speed increase when moving.</param>
    /// <param name="deceleration">The rate of speed decrease when stopping.</param>
    /// <param name="moveInput">The directional input for movement.</param>
    private void Move(float acceleration, float deceleration, Vector2 moveInput) 
    {
        if (moveInput != Vector2.zero && !Input.GetMouseButton(1))
        {
            if (!playerWalking.isPlaying && !isJumping)
            {
                playerWalking.Play();
            }
            
            ShouldTurn();

            Vector2 targetVelocity;
            float up = 0f;
            if (isClimbing)
            {
                up = moveInput.y;
            }
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, up) * settings.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, up) * settings.MaxWalkSpeed;
            }

            _velocity = Vector2.Lerp(_velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rigidBody.velocity = !isClimbing ? new Vector2(_velocity.x, _rigidBody.velocity.y) : new Vector2(_velocity.x, _velocity.y); //if the player is climbing, allow movement in the y direction. 
        }
        else if (moveInput == Vector2.zero || Input.GetMouseButton(1)) //check if there isn't move input or if the player is aiming
        {
            playerWalking.Stop();
            _velocity = Vector2.Lerp(_velocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rigidBody.velocity = !isClimbing ? new Vector2(_velocity.x, _rigidBody.velocity.y) : new Vector2(_velocity.x, _velocity.y);
        }

    }
    /// <summary>
    /// Checks for jump input, buffers jump actions, and handles jump initiation and cancellation logic.
    /// </summary>
    private void JumpChecks()
    {
        //when jump is pressed
        if(InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = settings.JumpBufferTime;
            _jumpReleaseDuringBuffer = false;
        }

        //when jump is released
        if (InputManager.JumpWasReleased)
        {
            if(_jumpBufferTimer > 0f)
            {
                _jumpReleaseDuringBuffer = true;
            }
            
            if(isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = settings.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    //not past apex
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //start jump
        if (_jumpBufferTimer > 0 && (!isJumping || isClimbing) && ((_grounded ||  isClimbing)|| _coyoteTimer > 0f))
        {
            StartJump(1);

            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        else if (_jumpBufferTimer > 0f && isJumping && _numberOfJumpsUsed < settings.NumberOfJumpsAllowed)
        {
            //double jump
            _isFastFalling = false;
            StartJump(1);
        }
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < settings.NumberOfJumpsAllowed - 1) 
        {
            //air jump after coyote time
            StartJump(2);
            _isFastFalling = false;
        }

        //Landed
        if ((isJumping || _isFalling) && _grounded && VerticalVelocity <= 0f)
        {
            isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    /// <summary>
    /// Initiates a jump and applies initial vertical velocity based on the number of jumps used.
    /// </summary>
    /// <param name="numJumpsUsed">The number of jumps performed so far.</param>
    private void StartJump(int numJumpsUsed)
    {
        if (!isJumping) 
        {
            isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numJumpsUsed;
        VerticalVelocity = settings.InitialJumpVelocity;
    }

    /// <summary>
    /// Updates vertical velocity and jump behavior based on jump state, gravity, and fast fall logic.
    /// </summary>
    private void Jump()
    {
        //gravity while jumping
        if(isJumping)
        {
            if(_headCollided)
            {
                _isFastFalling = true;
            }

            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(settings.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > settings.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < settings.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                else
                {
                    //gravity on ascending but not at apex
                    VerticalVelocity += settings.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            else if (!_isFastFalling)
            {
                //Gravity on descending
                VerticalVelocity += settings.Gravity * settings.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        //Jump cut
        if (_isFastFalling)
        {
            if(_fastFallTime >= settings.TimeForUpwardsCancel)
            {
                VerticalVelocity += settings.Gravity * settings.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(_fastFallTime < settings.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / settings.TimeForUpwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        //Gravity while falling
        if(!_grounded && !isJumping) 
        {
            if(!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += settings.Gravity * Time.fixedDeltaTime;
        }

        //clamp fall speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -settings.MaxFallSpeed, 50f);

        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, VerticalVelocity);
    }

    /// <summary>
    /// Determines if the player should turn based on movement input and facing direction.
    /// </summary>
    private static void ShouldTurn()
    { }

    /// <summary>
    /// Rotates the player to face left or right based on the specified direction.
    /// </summary>
    /// <param name="turnRight">True if the player should face right, false if the player should face left.</param>
    private void Turn(bool turnRight)
    {
        if(turnRight) 
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    /// <summary>
    /// Performs collision checks to determine if the player is grounded.
    /// </summary>
    private void IsGrounded() 
    {
        if(isClimbing) 
        {
            _grounded = true;
            return;
        }

        if (!feetCollider)
            return;
        
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x, settings.GroundDetectionRayLength);

        _groundRaycast = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, settings.GroundDetectionRayLength, settings.Ground);

        _grounded = _groundRaycast.collider;

        //Debug Visualization
        if(settings.Debug_ShowIsGrounded) 
        {
            var rayColor = _grounded ? Color.green : Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * settings.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * settings.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - settings.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        //End Debug
    }

    /// <summary>
    /// Performs collision checks to determine if the player has bumped their head.
    /// </summary>
    private void BumpedHead()
    {
        if (!feetCollider || !bodyCollider)
            return;
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x * settings.HeadWidth, settings.HeadDetectionRayLength);

        _headRayCast = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, settings.HeadDetectionRayLength, settings.Ground);
        _headCollided = _headRayCast.collider;

        //debug
        if (settings.Debug_ShowHeadBumpBox)
        {
            var headWidth = settings.HeadWidth;

            var rayColor = _headCollided ? Color.green : Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * settings.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * settings.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + settings.HeadDetectionRayLength), Vector2.right * (boxCastSize.x * headWidth), rayColor);
        }
    }

    /// <summary>
    /// Combines all collision detection checks into one method for ease of use in the update loop.
    /// </summary>
    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    /// <summary>
    /// Decrements timers like the jump buffer and coyote time in each frame.
    /// </summary>
    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_grounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = settings.JumpCoyoteTime;
        }
    }

    /// <summary>
    /// Draws a visual representation of the jump arc in the scene for debugging.
    /// </summary>
    /// <param name="moveSpeed">The speed at which the player is moving horizontally.</param>
    /// <param name="gizmoColor">The color of the arc line in the scene view.</param>
    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        if (feetCollider)
        {
            Vector2 startPosition = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
            Vector2 previousPosition = startPosition;
            float speed;
            if (settings.DrawRight)
            {
                speed = moveSpeed;
            }
            else
            {
                speed = -moveSpeed;
            }

            Vector2 tempVelocity = new Vector2(speed, settings.InitialJumpVelocity);

            Gizmos.color = gizmoColor;

            float timeStep = 2 * settings.TimeTillJumpApex / settings.ArcResolution;

            for (int i = 0; i < settings.VisualizationSteps; i++)
            {
                float simulationTime = i * timeStep;
                Vector2 displacement;

                if (simulationTime < settings.TimeTillJumpApex)
                {
                    displacement = tempVelocity * simulationTime +
                                   0.5f * new Vector2(0, settings.Gravity) * simulationTime * simulationTime;
                }
                else if (simulationTime < settings.TimeTillJumpApex + settings.ApexHangTime)
                {
                    float apexTime = simulationTime - settings.TimeTillJumpApex;
                    displacement = tempVelocity * settings.TimeTillJumpApex + 0.5f * new Vector2(0, settings.Gravity) *
                        settings.TimeTillJumpApex * settings.TimeTillJumpApex;
                    displacement += new Vector2(speed, 0) * apexTime;
                }
                else
                {
                    float descendTime = simulationTime - (settings.TimeTillJumpApex + settings.ApexHangTime);
                    displacement = tempVelocity * settings.TimeTillJumpApex + 0.5f * new Vector2(0, settings.Gravity) *
                        settings.TimeTillJumpApex * settings.TimeTillJumpApex;
                    displacement += new Vector2(speed, 0) * settings.ApexHangTime;
                    displacement += new Vector2(speed, 0) * descendTime +
                                    0.5f * new Vector2(0, settings.Gravity) * descendTime * descendTime;
                }

                var drawPoint = startPosition + displacement;

                if (settings.StopOnCollision)
                {
                    RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition,
                        Vector2.Distance(previousPosition, drawPoint), settings.Ground);
                    if (hit.collider != null)
                    {
                        Gizmos.DrawLine(previousPosition, hit.point);
                        break;
                    }
                }

                Gizmos.DrawLine(previousPosition, drawPoint);
                previousPosition = drawPoint;
            }
        }
    }
}

