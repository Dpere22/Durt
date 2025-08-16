using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SoundController : MonoBehaviour
{
public GameObject spotlightObject;
    public Light2D spotLight; 
    public LayerMask detectionLayer;
    public LayerMask ignoreLayer;
    public GameObject parentObject;
    private bool _looking;
    public bool _foundThisLoop;
    private bool _foundPreviousLoop;
    public int lookers;
    [SerializeField] public AudioSource audioSource;

    private void Start()
    {
        detectionLayer = LayerMask.GetMask("Enemy", "ladders", "signs");
        ignoreLayer = ~detectionLayer;
        if (spotlightObject != null)
        {
            spotLight = spotlightObject.GetComponent<Light2D>();
        }
        else
        {
            Debug.LogError("Spotlight object not assigned!");
        }
    }

    void OnEnable()
    {
        Actions.OnPlayerSpotted += CountLookers;
        Actions.OnPlayerNotSpotted += CountUnlookers;
    }

    void OnDisable()
    {
        Actions.OnPlayerSpotted -= CountLookers;
        Actions.OnPlayerNotSpotted -= CountUnlookers;
    }

    private void FixedUpdate()
    {
        if (spotLight)
        {
            CastRays();
        }
    }

    void OnDestroy()
    {
        if (_looking) 
        {
            Actions.OnPlayerNotSpotted?.Invoke(parentObject);
        }

        if (lookers - 1 <= 0)
        {
            TimerManager.multiply = false;
        }
    }

    void CastRays()
    {
        _foundThisLoop = false;
        float angleStep = spotLight.pointLightInnerAngle / 10f;
        float halfAngle = spotLight.pointLightInnerAngle / 2f;
        float radius = spotLight.pointLightOuterRadius;
        GameObject player = null;

        Vector2 origin = transform.position;
        float lightRotation = spotlightObject.transform.eulerAngles.z;

        for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep)
        {
            float rayAngle = lightRotation + angle;
            Vector2 rayDirection = Quaternion.Euler(parentObject.transform.eulerAngles.x, parentObject.transform.eulerAngles.y, rayAngle) * Vector2.up;

            RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, radius, ignoreLayer);

            Debug.DrawRay(origin, rayDirection * radius, Color.red);

            if (hit.collider && hit.collider.CompareTag("Player"))
            {
                _foundThisLoop = true;
                player = hit.collider.gameObject;
            }
        }
        
        CheckSound();

        switch (_foundThisLoop)
        {
            case true when !_foundPreviousLoop:
                _looking = true;
                TimerManager.multiply = true;
                Actions.OnPlayerSpotted?.Invoke(player, parentObject);
                _foundPreviousLoop = true;
                break;
            case false when _foundPreviousLoop:
            {
                _looking = false;
                Actions.OnPlayerNotSpotted?.Invoke(parentObject);
                _foundPreviousLoop = false;
                if (lookers <= 0)
                {
                    TimerManager.multiply = false;
                }

                break;
            }
        }
    }

    private void CountLookers(GameObject gameObject, GameObject sender) 
    {
        lookers++;
    }

    private void CountUnlookers(GameObject sender) 
    {
        lookers--;
    }

    private void CheckSound()
    {
        if (PauseScreen.isPaused)
        {
            audioSource.Stop();
        }
        else if (_foundThisLoop && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
