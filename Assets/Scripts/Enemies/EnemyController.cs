using System.Collections.Generic;
using UnityEngine;
using GOSM;
using System;
using UnityEngine.Serialization;


public class EnemyController : MonoBehaviour
{
    private static readonly int XVelocity = Animator.StringToHash("XVelocity");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    //DIRT
    public bool dontLookRight;
    public bool dontLookLeft;
    public bool canDie;
    
    [SerializeField] private GameObject deathSoundPlayer;
    public Rigidbody2D rb;

    public GameObject lightHolder;
    public GameObject parent;
    public bool canJump;
    public float timeBetweenJumps = 2f;
    private float _jumpTimer;
    private bool _canJump;
    public bool isJumping;
    public float jumpForce = 5f;

    
    private Animator _anim;
    
    
    private float _jumpLength = 0.5f;
    private float _jumpAnimationTimer;

    private bool IsPlayerSpotted { get; set; }
    public float rightOffset = 90;
    private GameObject _player;

    //GOSM Stuff
    [SerializeField]
    public string stateManagerCurrentGoal;
    public bool goalStatus;
    private Fail _emptyFail;
    private Step _emptyStep;
    private Goal _emptyGoal;
    private LinkedList<Step> _emptyGoalStepList = new LinkedList<Step>();
    private StateManager _stateManager;
    private LayerMask _layerMask;

    // Look up and down
    public float lookUpTargetAngle = -22f;
    public float lookDownTargetAngle = 44f;
    public float centerTargetAngle;
    public float lookTimeMult = 1f;

    [FormerlySerializedAs("_completedLookCycle")] [SerializeField]
    public bool completedLookCycle = true;
    private Goal _lookUpAndDown;
    private Step _lookUp;
    private Step _lookDown;
    private Step _lookCenter;
    private LinkedList<Step> _lookUpAndDownStepList = new LinkedList<Step>();

    //Raycast for ground detection
    public float groundDetectionRayCastLength;

    // Walk parameters
    public float walkSpeed;
    private string _lastDirectionMoved = "left";

    [FormerlySerializedAs("_directionFacing")] public string directionFacing = "left";

    // Move left
    private Goal _moveLeft;
    private Step _turnLeft;
    private Step _goLeft;
    private LinkedList<Step> _moveLeftStepList = new LinkedList<Step>();

    //Move right
    private Goal _moveRight;
    private Step _turnRight;
    private Step _goRight;
    private LinkedList<Step> _moveRightStepList = new LinkedList<Step>();

    //Chase player
    public bool moving;
    public float lockOnSpeed;
    public bool lockedOn;
    public float distanceTillStop = 1f;
    private Goal _followPlayer;
    private Step _lockOnToPlayer;
    private Step _moveTowardsPlayer;
    private LinkedList<Step> _followPlayerStepList = new LinkedList<Step>();

    private bool _respawnMoving;
    private string _directionFacing;
    private Goal _respawnGoal;
    private Quaternion _respawnRotation;

    
    private Vector2 _respawnPoint;
    //Goal list
    private List<Goal> _chasePlayerGoalList;

    private void ConstructGosmObjects()
    {
        //Fail
        _emptyFail = new Fail(EmptyFail);
        _emptyStep = new Step(EmptyFail, _emptyFail);
        _emptyGoalStepList.AddLast(_emptyStep);
        _emptyGoal = new Goal(_emptyGoalStepList, () => true, 0, false, false, false);

        //Look up and down goal
        _lookUp = new Step(LookUp, _emptyFail);
        _lookDown = new Step(LookDown, _emptyFail);
        _lookCenter = new Step(LookCenter, _emptyFail);
        _lookUpAndDownStepList.AddLast(_lookUp);
        _lookUpAndDownStepList.AddLast(_lookDown);
        _lookUpAndDownStepList.AddLast(_lookCenter);
        _lookUpAndDown = new Goal(_lookUpAndDownStepList, () => LookCyclePrereqs(), 1, false, false, false);

        // Go left goal
        _turnLeft = new Step(TurnLeft, _emptyFail);
        _goLeft = new Step(GoLeft, _emptyFail);
        _moveLeftStepList.AddLast(_turnLeft);
        _moveLeftStepList.AddLast(_goLeft);
        _moveLeft = new Goal(_moveLeftStepList, () => GoLeftPrereqs(), 2, false, false, false);

        // Go right goal
        _turnRight = new Step(TurnRight, _emptyFail);
        _goRight = new Step(GoRight, _emptyFail);
        _moveRightStepList.AddLast(_turnRight);
        _moveRightStepList.AddLast(_goRight);
        _moveRight = new Goal(_moveRightStepList, () => GoRightPrereqs(), 2, false, false, false);

        // Follow player goal
        _lockOnToPlayer = new Step(LockOnToPlayer, _emptyFail);
        //_moveTowardsPlayer = new Step(MoveTowardsPlayer, _emptyFail);
        _followPlayerStepList.AddLast(_lockOnToPlayer);
        //_followPlayerStepList.AddLast(_moveTowardsPlayer);
        _followPlayer = new Goal(_followPlayerStepList, () => IsPlayerSpotted, 5, true, true, false);

        //Goal list
        _chasePlayerGoalList = new List<Goal>() { _moveLeft, _moveRight, _followPlayer, _lookUpAndDown };
        _stateManager = new StateManager(_chasePlayerGoalList, _emptyGoal);
    }



    private void OnEnable()
    {
        Actions.OnPlayerSpotted += SpottedPlayer;
        Actions.OnPlayerNotSpotted += PlayerNotSpotted;
        LoseManager.OnRespawn += HandleRespawn;
    }

    private void OnDisable()
    {
        Actions.OnPlayerSpotted -= SpottedPlayer;
        Actions.OnPlayerNotSpotted -= PlayerNotSpotted;
    }

    private void OnDestroy()
    {
        LoseManager.OnRespawn -= HandleRespawn;
    }

    private void Awake()
    {
        ConstructGosmObjects();
        if (dontLookLeft) _moveLeft.offline = true;
        if (dontLookRight) _moveRight.offline = true;
        _layerMask = LayerMask.GetMask("Ground", "boulderLayer");
        lightHolder.transform.Rotate(new Vector3(0, 0, centerTargetAngle));
        if(canJump) _canJump = true;
    }

    private void Start() 
    {
        _anim = parent.GetComponent<Animator>();
        canDie = true;
        _respawnMoving = moving;
        _directionFacing = directionFacing;
        _respawnGoal = _stateManager.currentGoal;
        _respawnPoint = parent.transform.position;
        _respawnRotation = parent.gameObject.transform.rotation;
        if (dontLookLeft) _moveLeft.offline = true;
        if (dontLookRight) _moveRight.offline = true;
    }

    private void Update()
    {
        _stateManager.Execute();
        _jumpTimer += Time.deltaTime;
        stateManagerCurrentGoal = _stateManager.CurrentlyInvoking;
        JumpTimer();
    }

    private void FixedUpdate()
    {
        _anim.SetFloat(XVelocity, Math.Abs(rb.velocity.x));
        _anim.SetBool(IsMoving, moving);
    }

    private int EmptyFail()
    {
        return 1;
    }
    private void HandleRespawn()
    {
        if (parent.gameObject == null) return;
        parent.gameObject.SetActive(true);
        directionFacing = _directionFacing;
        moving = _respawnMoving;
        ConstructGosmObjects();
        IsPlayerSpotted = false;
        parent.transform.position = _respawnPoint;
        Vector3 eulerAngles = _respawnRotation.eulerAngles;
        parent.gameObject.transform.rotation = Quaternion.Euler(0, eulerAngles.y, 0);
        if (dontLookLeft) _moveLeft.offline = true;
        if (dontLookRight) _moveRight.offline = true;
        //parent.gameObject.SetActive(true);
    }

    bool LookCyclePrereqs() 
    {
        if(dontLookLeft && dontLookRight) 
        {
            return true;
        }
        if (completedLookCycle)
        {
            return false;
        }

        return true;
    }

    public int LookUp()
    {
        moving = false;
        if (IsPlayerSpotted)
        {
            return 0;
        }

        float currentRotation = lightHolder.transform.rotation.eulerAngles.z;
        float rotationAmount = Time.deltaTime * lookTimeMult;

        float angleDifference = Mathf.DeltaAngle(currentRotation, lookUpTargetAngle);

        if (Mathf.Abs(angleDifference) <= 1f)
        {
            return 1;
        }
        else
        {
            float signOperator = Mathf.Sign(angleDifference);
            lightHolder.transform.Rotate(new Vector3(0, 0, signOperator * rotationAmount));
            return -1;
        }
    }

    public int LookDown()
    {
        moving = false;
        if (IsPlayerSpotted)
        {
            return 0;
        }

        float currentRotation = lightHolder.transform.rotation.eulerAngles.z;
        float rotationAmount = Time.deltaTime * lookTimeMult;

        float angleDifference = Mathf.DeltaAngle(currentRotation, lookDownTargetAngle);

        if (Mathf.Abs(angleDifference) <= 1f)
        {
            return 1;
        }
        else
        {
            float signOperator = Mathf.Sign(angleDifference);
            lightHolder.transform.Rotate(new Vector3(0, 0, signOperator * rotationAmount));
            return -1;
        }
    }

    public int LookCenter()
    {
        moving = false;
        if (IsPlayerSpotted)
        {
            return 0;
        }

        float currentRotation = lightHolder.transform.rotation.eulerAngles.z;
        float rotationAmount = Time.deltaTime * lookTimeMult;

        float angleDifference = Mathf.DeltaAngle(currentRotation, centerTargetAngle);

        if (Mathf.Abs(angleDifference) <= 10f)
        {
            completedLookCycle = true;
            return 1;
        }
        else
        {
            float signOperator = Mathf.Sign(angleDifference);
            lightHolder.transform.Rotate(new Vector3(0, 0, signOperator * rotationAmount));
            return -1;
        }
    }

    public bool GoLeftPrereqs()
    {
        bool result = completedLookCycle &&  (_lastDirectionMoved != "left" || dontLookRight);
        return result;
    }

    public bool GoRightPrereqs()
    {
        bool result = completedLookCycle && (_lastDirectionMoved != "right" || dontLookLeft);
        return result;
    }

    public int TurnLeft()
    {
        if (directionFacing != "left")
        {
            parent.transform.Rotate(new Vector3(0, 180, 0));
            directionFacing = "left";
        }
        return 1;
    }

    public int GoLeft()
    {
        if (IsPlayerSpotted)
        {
            _lastDirectionMoved = "left";
            completedLookCycle = false;
            return 0;
        }

        Vector2 origin = parent.transform.position;

        // Cast a ray downwards in front of the character to detect ground
        RaycastHit2D groundHit = Physics2D.Raycast(origin + Vector2.left * 1f, Vector2.down, groundDetectionRayCastLength, _layerMask);

        // Cast a ray directly to the left to detect obstacles
        RaycastHit2D obstacleHit = Physics2D.Raycast(origin, Vector2.left, groundDetectionRayCastLength, _layerMask);

        Debug.DrawRay(origin + Vector2.left * 0.5f, Vector2.down * groundDetectionRayCastLength, Color.red);
        Debug.DrawRay(origin, Vector2.left * groundDetectionRayCastLength, Color.blue);

        if (groundHit.collider && !obstacleHit.collider)
        {
            moving = true;
            parent.transform.Translate(new Vector3(-1 * (Time.deltaTime * walkSpeed), 0, 0));
            return -1; 
        }
        else
        {
            completedLookCycle = false;
            _lastDirectionMoved = "left";
            return 1; 
        }
    }

    public int TurnRight()
    {
        if (directionFacing != "right")
        {
            parent.transform.Rotate(new Vector3(0, 180, 0));
            directionFacing = "right"; 
        }
        return 1;
    }

    public void JumpTimer() 
    {
        _jumpAnimationTimer += Time.deltaTime;  
        if(_jumpAnimationTimer > _jumpLength) 
        {
            isJumping = false;
        }
    }

    public int GoRight()
    {
        if (IsPlayerSpotted)
        {
            completedLookCycle = false;
            return 0;
        }

        Vector2 origin = parent.transform.position;

        RaycastHit2D groundHit = Physics2D.Raycast(origin + Vector2.right * 1f, Vector2.down, groundDetectionRayCastLength, _layerMask);

        RaycastHit2D obstacleHit = Physics2D.Raycast(origin, Vector2.right, groundDetectionRayCastLength, _layerMask);

        Debug.DrawRay(origin + Vector2.right * 1f, Vector2.down * groundDetectionRayCastLength, Color.red);
        Debug.DrawRay(origin, Vector2.right * groundDetectionRayCastLength, Color.blue);

        if (groundHit.collider && !obstacleHit.collider)
        {
            moving = true;
            parent.transform.Translate(new Vector3(-1 * Time.deltaTime * walkSpeed, 0, 0));
            return -1;
        }
        else
        {
            _lastDirectionMoved = "right";
            completedLookCycle = false;
            return 1;
        }
    }

    public int LockOnToPlayer()
    {
        if (!IsPlayerSpotted || _player == null)
        {
            return 0;
        }

        MoveTowardsPlayer();

        float currentRotation = lightHolder.transform.eulerAngles.z;

        if(directionFacing == "left" && currentRotation > 180 ) 
        {
            TurnRight();
        }
        else if (directionFacing == "right" && currentRotation > 180)
        {
            TurnLeft();
        }

        float rotationNeeded;
        if (directionFacing == "left")
        {
            rotationNeeded = (RotationNeeded(transform.position.x, transform.position.y, _player.transform.position.x, _player.transform.position.y) - 270) % 360;
        }
        else
        {
            rotationNeeded = (RotationNeeded(transform.position.x, transform.position.y, _player.transform.position.x, _player.transform.position.y) + rightOffset) % 360;
            rotationNeeded *= -1;
        }

        if (rotationNeeded < 0) rotationNeeded += 360;
        float angleDifference = Mathf.DeltaAngle(currentRotation, rotationNeeded);

        Vector3 directionToPlayer = (_player.transform.position - lightHolder.transform.position).normalized;
        Debug.DrawRay(lightHolder.transform.position, directionToPlayer * 10, Color.green);

        Vector3 targetDirection = Quaternion.Euler(0, 0, rotationNeeded) * Vector3.up;
        Debug.DrawRay(lightHolder.transform.position, targetDirection * 10, Color.blue);

        if (Mathf.Abs(angleDifference) > 10f)
        {
            float signOperator = Mathf.Sign(angleDifference);
            lightHolder.transform.Rotate(new Vector3(0, 0, signOperator * Time.deltaTime * lockOnSpeed));
            return -1; 
        }
        else
        {
            lockedOn = true;
            return 1;
        }
    }

    private float RotationNeeded(float p1X, float p1Y, float p2X, float p2Y)
    {
        return (float)(Math.Atan2(p1X - p2X, p2Y - p1Y) * 180.0 / Math.PI + 630) % 360.0f;
    }

    public int MoveTowardsPlayer()
    {
        if (!IsPlayerSpotted || _player == null)
        {
            Debug.Log("Failing checks");
            return 0;
        }

        Vector3 currentPosition = parent.transform.position;
        Vector3 targetPosition = _player.transform.position;
        Vector2 direction = (targetPosition - currentPosition).normalized;

        // Determine movement direction 
        Vector2 horizontalDirection = direction.x > 0 ? Vector2.right : Vector2.left;

        // Raycast for ground and obstacle detection
        RaycastHit2D groundHit = Physics2D.Raycast(currentPosition + (Vector3)(horizontalDirection * 0.5f), Vector2.down, groundDetectionRayCastLength, _layerMask);
        RaycastHit2D obstacleHit = Physics2D.Raycast(currentPosition, horizontalDirection, groundDetectionRayCastLength, _layerMask);

        Debug.DrawRay(currentPosition + (Vector3)(horizontalDirection * 0.5f), Vector2.down * groundDetectionRayCastLength, Color.red);
        Debug.DrawRay(currentPosition, horizontalDirection * groundDetectionRayCastLength, Color.blue);

        if (_canJump && _player && _player.GetComponentInParent<PlayerMovement>().isJumping && _jumpTimer > timeBetweenJumps)
        {
            Jump(direction.x);
        }

        if (groundHit.collider && !obstacleHit.collider)
        {
            if (Vector3.Distance(currentPosition, targetPosition) < distanceTillStop)
            {
                moving = false;
                return 1; 
            }
            moving = true;
            parent.transform.position = Vector3.MoveTowards(currentPosition, new Vector3(targetPosition.x, currentPosition.y, currentPosition.z), walkSpeed * Time.deltaTime);
            return -1; 
        }
        else if (_canJump && _jumpTimer > timeBetweenJumps)
        {
            Jump(direction.x);
            return -1;  
        }
        else
        {
            return 1;  
        }
    }

    public void Jump(float xDirection) 
    {
        isJumping = true;
        _jumpAnimationTimer = 0;
        Vector2 jumpForceVector = new Vector2(xDirection * jumpForce * 0.7f, jumpForce);
        rb.AddForce(jumpForceVector, ForceMode2D.Impulse);
        _canJump = false;
        _jumpTimer = 0;
    }

    public void SpottedPlayer(GameObject player, GameObject sender)
    {
        if (sender.Equals(parent))
        {
            IsPlayerSpotted = true;
            canDie = false;
            _player = player;
            _stateManager.ResetGoals();
            _stateManager.Execute();
        }
    }

    public void PlayerNotSpotted(GameObject sender)
    {
        if (sender.Equals(parent))
        {
            canDie = true;
            IsPlayerSpotted = false;
            _player = null;
            lockedOn = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var collided = collision.gameObject;
        if (!collided.CompareTag("Bullet") || !canDie) return;
        var deathSoundObject = Instantiate(deathSoundPlayer, new Vector3(transform.position.x, transform.position.y - 2, transform.position.z), Quaternion.identity);
        Destroy(deathSoundObject, deathSoundPlayer.GetComponent<AudioSource>().clip.length);
        parent.SetActive(false);
    }
}
