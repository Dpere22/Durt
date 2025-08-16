using System.Collections;
using UnityEngine;

public class TNTController : MonoBehaviour
{
    public LayerMask boulderLayer;
    public float explosionRadius = 5f;
    [SerializeField] public ParticleSystem explodeEffect;
    [SerializeField] public AudioSource explodeSound;


    private void Start()
    {
        LoseManager.OnRespawn += HandleRespawn;
    }

    private void OnDestroy()
    {
        LoseManager.OnRespawn -= HandleRespawn;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var collided = other.gameObject;
        if (!collided.CompareTag("Bullet")) return;
        Explode();
    }

    private void Explode()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, boulderLayer);
        foreach (var col in colliders)
        {
            col.gameObject.SetActive(false);
        }
        HandleDeath();
        explodeEffect.Play();
        explodeSound.Play();
    }

    private void HandleDeath()
    {
        Renderer tntRenderer = GetComponent<Renderer>();
        tntRenderer.enabled = false;
        Collider2D tntCollider = GetComponent<Collider2D>();
        if (tntCollider != null)
        {
            tntCollider.enabled = false; // Disable the collider
        }
    }

    private void HandleRespawn()
    {
        gameObject.SetActive(true);
        Renderer tntRenderer = GetComponent<Renderer>();
        tntRenderer.enabled = true;
        Collider2D tntCollider = GetComponent<Collider2D>();
        if (tntCollider != null)
        {
            tntCollider.enabled = true; // Disable the collider
        }
    }

    /// <summary>
    /// This method allows visual changing of the explosion radius in engine
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
