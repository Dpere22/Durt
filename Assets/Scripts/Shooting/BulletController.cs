using UnityEngine;

public class BulletController : MonoBehaviour
{
    private Vector3 _mousePos;
    private Camera _mainCamera;
    
    public float force;
    public int maxBounceNum;
    private int _currentBounce;
    private Rigidbody2D _rb;
    [SerializeField] public AudioClip ricochetSound;
    [SerializeField] public ParticleSystem explosive;
    [SerializeField] public GameObject audioSourcePrefab;
    void Start()
    {
        _currentBounce = 0;
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _rb = GetComponent<Rigidbody2D>();
        _mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = _mousePos - transform.position;
        Vector3 rotation = transform.position - _mousePos;
        _rb.velocity = new Vector2(direction.x, direction.y).normalized * force;
        float rot = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }
    private void KillBullet()
    {
        Instantiate(explosive, _rb.position, Quaternion.identity);
        _rb.velocity = new Vector2(0, 0);
        Destroy(gameObject);
    }

    private void PlayRicochetSound()
    {
        GameObject audioObject = Instantiate(audioSourcePrefab, transform.position, Quaternion.identity);
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        audioSource.clip = ricochetSound;
        audioSource.Play();
        
        Destroy(audioObject, ricochetSound.length);
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Reflector"))
        {
            //Ricochets happen in engine due to the material on the bullet
            if (_currentBounce == maxBounceNum)
            {
                KillBullet();
            }
            else
            {
                PlayRicochetSound();
                _currentBounce++;
            }
        }
        else //bullet hit something that shouldn't reflect
        {
            KillBullet();
        }
    }
}
