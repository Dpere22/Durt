using UnityEngine;

public class LadderMovement : MonoBehaviour
{
    // Start is called before the first frame update
    private float _vertical;
    private bool _isLadder;

    private PlayerMovement _playerMovement;
    [SerializeField] GameObject player;

    void Start()
    {
        _playerMovement = player.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        _vertical = Input.GetAxis("Vertical");
    }
    private void FixedUpdate()
    {
        if(_isLadder && Mathf.Abs(_vertical) > 0f)
        {
            _playerMovement.isClimbing = true;
        }
        else
        {
            _playerMovement.isClimbing = false; 
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            _isLadder = true; //I do this because we must always be able to climb while in a ladders collider
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            _isLadder = false;
        }
    }
}
