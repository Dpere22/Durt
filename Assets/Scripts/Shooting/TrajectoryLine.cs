using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private Shooting _gun; //i.e. where the bullet comes from
    public GameObject player;
    private LineRenderer _lineRenderer;
    private bool _isShooting;
    private bool _needRefresh;
    private float _projectileSpeed;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>();
        _needRefresh = false;
        _isShooting = false;
        if (player != null)
        {
            player.GetComponent<PlayerMovement>();
        }

        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 3;
        _lineRenderer.enabled = false;
        

        _gun.GetComponent<BulletController>();
    }

    // Update is called once per frame 
    void Update()
    {
        if (InputManager.LeftClick) //check if the player shot the gun
        {
            _needRefresh = true; //the player needs to aim again to see the line again
            _isShooting = true;
            _lineRenderer.enabled = false; //the line should disappear on aim
        }
        else if (!InputManager.RightClickHeld)
        {
            _needRefresh = false;
            _lineRenderer.enabled = false;
        }
        else if (InputManager.RightClickHeld && !_isShooting && !_needRefresh)
        {
            _isShooting = false;
            RenderLine();
        }
        _isShooting = false;
        
    }
    private void RenderLine()
    {
        _lineRenderer.enabled = true;

        Vector2 startPos = _bulletSpawnPoint.position;

        RaycastHit2D hit = Physics2D.Raycast(startPos, transform.right, Mathf.Infinity, LayerMask.GetMask("Ground", "boulderLayer", "Enemy"));
        if (!hit.collider) return;
        
        
        Vector2 lineDirection = hit.point - startPos;
        Vector2 reflectedDirection = Vector2.Reflect(lineDirection, hit.normal);
        reflectedDirection.Normalize();
        reflectedDirection *= 1;
        Vector2 reflectedPoint = hit.point + reflectedDirection;
        switch (hit.collider.gameObject.layer)
        {
            case 6: //only should reflection on ground layer
                SetPoints(startPos, hit.point, reflectedPoint);
                break;
            default:
                SetPoints(startPos, hit.point, hit.point);
                break;
        }
    }
    void SetPoints(Vector2 newPointA, Vector2 newPointB, Vector2 newPointC)
    {
        _lineRenderer.SetPosition(0, newPointA);
        _lineRenderer.SetPosition(1, newPointB);
        _lineRenderer.SetPosition(2, newPointC);
    }
}
