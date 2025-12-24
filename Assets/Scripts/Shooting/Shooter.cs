using UI;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] GameObject player;
    public GameObject bullet;
    public Transform bulletTransform;
    public bool canFire;
    private float _timer;
    public float timeBetweenFiring;
    private TemporaryInventory _inventory;
    private PlayerMovement _playerMovement;

    public AudioSource gunSound;
    public AudioClip gunshotSound;
    public AudioClip dryFireSound;
    
    // Start is called before the first frame update
    void Start()
    {
        _inventory = player.GetComponent<TemporaryInventory>();
        _playerMovement = player.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canFire)
        {
            _timer += Time.deltaTime;
            if (_timer > timeBetweenFiring)
            {
                canFire = true;
                _timer = 0;
            }
        }

        if (!InputManager.LeftClick || !canFire || PauseScreen.IsPaused || _playerMovement.isClimbing) return;
        gunSound.PlayOneShot(gunshotSound);
        canFire = false;

        Instantiate(bullet, bulletTransform.position, Quaternion.identity);
    }
}
