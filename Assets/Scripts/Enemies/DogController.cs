using System.Collections.Generic;
using UnityEngine;
using GOSM;
using System;
using System.Collections;
using UnityEngine.Serialization;


public class DogController : MonoBehaviour
{
    //DIRT
    public bool dontLookRight;
    public bool dontLookLeft;
    
    [SerializeField] private GameObject deathSoundPlayer;
    public Rigidbody2D rb;

    [SerializeField] private bool playerHasBeenSpotted;
    [SerializeField] private AudioSource deathSoundSource;
    
    public GameObject lightHolder;
    public GameObject parent;
    public bool canJump;
    public float timeBetweenJumps = 2f;
    [FormerlySerializedAs("_jumpTimer")] public float jumpTimer;
    public bool isJumping;
    public float jumpForce = 5f;
    
    private readonly float _jumpLength = 0.5f;
    private float _jumpAnimationTimer;

    private bool IsPlayerSpotted { get; set; }
    public float rightOffset = 90;
    private GameObject _player;

    //GOSM Stuff
    private Fail _emptyFail;
    private Step _emptyStep;
    private Goal _emptyGoal;
    private readonly LinkedList<Step> _emptyGoalStepList = new LinkedList<Step>();
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
    private readonly LinkedList<Step> _lookUpAndDownStepList = new();

    //Raycast for ground detection
    public float groundDetectionRayCastLength;

    // Walk parameters
    public float walkSpeed;
    private string _lastDirectionMoved = "left";
    public float runSpeed;

    [FormerlySerializedAs("_directionFacing")] public string directionFacing = "left";

    // Move left
    private Goal _moveLeft;
    private Step _turnLeft;
    private Step _goLeft;
    private readonly LinkedList<Step> _moveLeftStepList = new();

    //Move right
    private Goal _moveRight;
    private Step _turnRight;
    private Step _goRight;
    private readonly LinkedList<Step> _moveRightStepList = new();

    //Chase player
    public bool moving;
    public float lockOnSpeed;
    public float distanceTillStop = 1f;
    private Goal _followPlayer;
    private Step _lockOnToPlayer;
    private Step _moveTowardsPlayer;
    private readonly LinkedList<Step> _followPlayerStepList = new();
    
    private Vector2 _respawnPoint;
    private string _respawnDirection;
    private Goal _respawnGoal;
    private bool _respawnIsMoving;

    //Goal list
    private List<Goal> _chasePlayerGoalList;

    public GameObject prefab;

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
        _lookUpAndDown = new Goal(_lookUpAndDownStepList, LookCyclePreChecks, 1, false, false, false);

        // Go left goal
        _turnLeft = new Step(TurnLeft, _emptyFail);
        _goLeft = new Step(GoLeft, _emptyFail);
        _moveLeftStepList.AddLast(_turnLeft);
        _moveLeftStepList.AddLast(_goLeft);
        _moveLeft = new Goal(_moveLeftStepList, GoLeftPreChecks, 2, false, false, false);

        // Go right goal
        _turnRight = new Step(TurnRight, _emptyFail);
        _goRight = new Step(GoRight, _emptyFail);
        _moveRightStepList.AddLast(_turnRight);
        _moveRightStepList.AddLast(_goRight);
        _moveRight = new Goal(_moveRightStepList, GoRightPreChecks, 2, false, false, false);

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
        // ConstructGosmObjects();
        // _layerMask = LayerMask.GetMask("Ground", "boulderLayer");
        // lightHolder.transform.Rotate(new Vector3(0, 0, centerTargetAngle));
    }

    private void HandleRespawn()
    {
        if (gameObject == null) return;
        gameObject.SetActive(true);
        _lastDirectionMoved = "left";
        directionFacing = _respawnDirection;
        moving = true;
        ConstructGosmObjects();
        playerHasBeenSpotted = false;
        IsPlayerSpotted = false;
        transform.position = _respawnPoint;
        gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        completedLookCycle = true;
        jumpTimer = 0f;
        //gameObject.SetActive(true);
    }
    private void Start() 
    {
        ConstructGosmObjects();
        _layerMask = LayerMask.GetMask("Ground", "boulderLayer");
        lightHolder.transform.rotation = Quaternion.Euler(0f, 0f, 80f);
        if (dontLookLeft) _moveLeft.offline = true;
        if (dontLookRight) _moveRight.offline = true;
        _respawnPoint = transform.position;
        _respawnDirection = directionFacing;
        _respawnGoal = _stateManager.currentGoal;
        _respawnIsMoving = moving;
        LoseManager.OnRespawn += HandleRespawn;
    }

    private void Update()
    {
        _stateManager.Execute();
        jumpTimer += Time.deltaTime;
        JumpTimer();
    }

    private int EmptyFail()
    {
        return 1;
    }

    bool LookCyclePreChecks() 
    {
        if(dontLookLeft && dontLookRight) 
        {
            return true;
        }
        return !completedLookCycle;
    }

    private int LookUp()
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

        float signOperator = Mathf.Sign(angleDifference);
        lightHolder.transform.Rotate(new Vector3(0, 0, signOperator * rotationAmount));
        return -1;
    }

    private int LookDown()
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

        float signOperator = Mathf.Sign(angleDifference);
        lightHolder.transform.Rotate(new Vector3(0, 0, signOperator * rotationAmount));
        return -1;
    }

    private int LookCenter()
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

    private bool GoLeftPreChecks()
    {
        bool result = completedLookCycle &&  (_lastDirectionMoved != "left" || dontLookRight);
        return result;
    }

    private bool GoRightPreChecks()
    {
        bool result = completedLookCycle && (_lastDirectionMoved != "right" || dontLookLeft);
        return result;
    }

    private int TurnLeft()
    {
        if (directionFacing == "left") return 1;
        parent.transform.Rotate(new Vector3(0, 180, 0));
        directionFacing = "left";
        return 1;
    }

    private int GoLeft()
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

        completedLookCycle = false;
        _lastDirectionMoved = "left";
        return 1;
    }

    private int TurnRight()
    {
        if (directionFacing == "right") return 1;
        parent.transform.Rotate(new Vector3(0, 180, 0));
        directionFacing = "right";
        return 1;
    }

    private void JumpTimer() 
    {
        _jumpAnimationTimer += Time.deltaTime;  
        if(_jumpAnimationTimer > _jumpLength) 
        {
            isJumping = false;
        }
    }

    private int GoRight()
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

        _lastDirectionMoved = "right";
        completedLookCycle = false;
        return 1;
    }

    private int LockOnToPlayer()
    {
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

        return 1;
    }

    private float RotationNeeded(float p1X, float p1Y, float p2X, float p2Y)
    {
        return (float)(Math.Atan2(p1X - p2X, p2Y - p1Y) * 180.0 / Math.PI + 630) % 360.0f;
    }

    private void MoveTowardsPlayer()
    {
        Vector3 currentPosition = parent.transform.position;
        Vector3 targetPosition = _player.transform.position;
        Vector2 direction = (targetPosition - currentPosition).normalized;

        // Determine movement direction 
        Vector2 horizontalDirection = direction.x > 0 ? Vector2.right : Vector2.left;

        // Raycast for ground and obstacle detection
        RaycastHit2D obstacleHit = Physics2D.Raycast(currentPosition, horizontalDirection, groundDetectionRayCastLength, _layerMask);

        Debug.DrawRay(currentPosition + (Vector3)(horizontalDirection * 0.5f), Vector2.down * groundDetectionRayCastLength, Color.red);
        Debug.DrawRay(currentPosition, horizontalDirection * groundDetectionRayCastLength, Color.blue);

        if (!obstacleHit.collider)
        {
            if (Vector3.Distance(currentPosition, targetPosition) < distanceTillStop)
            {
                moving = false;
                return;
            }
            moving = true;
            parent.transform.position = Vector3.MoveTowards(currentPosition, new Vector3(targetPosition.x, currentPosition.y, currentPosition.z), runSpeed * Time.deltaTime);
        }
        else if (obstacleHit.collider)
        {
            if (!(jumpTimer > timeBetweenJumps)) return;
            Jump();
        }
    }

    private void Jump() 
    {
        isJumping = true;
        _jumpAnimationTimer = 0;
        Vector2 jumpForceVector = new Vector2(0, jumpForce);
        rb.AddForce(jumpForceVector, ForceMode2D.Impulse);
        jumpTimer = 0;
    }

    private void SpottedPlayer(GameObject player, GameObject sender)
    {
        if (!sender.Equals(parent)) return;
        IsPlayerSpotted = true;
        playerHasBeenSpotted = true;
        _player = player;
        _stateManager.ResetGoals();
        _stateManager.Execute();
    }

    private void PlayerNotSpotted(GameObject sender)
    {
        if (!sender.Equals(parent)) return;
        if (playerHasBeenSpotted) return;
        IsPlayerSpotted = false;
        _player = null;
    }

    private void KillPlayer()
    {
        PlayKillSound();
        gameObject.SetActive(false);
        LoseManager.LoseGame();
    }

    private void PlayKillSound()
    {
        AudioSource audioSource = Instantiate(deathSoundSource, transform.position, Quaternion.identity);
        var audioSourceClip = audioSource.clip;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSourceClip.length);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var collided = collision.gameObject;
        if (collided.CompareTag("Player") && !PauseScreen.isPaused)
        {
            KillPlayer();
        }
        if (!collided.CompareTag("Bullet")) return;
        var deathSoundObject = Instantiate(deathSoundPlayer, new Vector3(transform.position.x, transform.position.y - 2, transform.position.z), Quaternion.identity);
        Destroy(deathSoundObject, deathSoundPlayer.GetComponent<AudioSource>().clip.length);
        gameObject.SetActive(false);
        //Destroy(parent);
    }
}
