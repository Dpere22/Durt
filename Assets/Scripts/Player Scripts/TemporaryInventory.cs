using UnityEngine;

/// <summary>
/// Note: This is a temporary inventory system for playtest 1. Can be changed in the future.
/// </summary>
public class TemporaryInventory : MonoBehaviour
{
    // Start is called before the first frame update
    //public int bullet_count;
    //public int max_bullets;
    public bool has_bomb;
    [SerializeField] AudioSource inventorySound;
    void Start()
    {
        has_bomb = false;
    }
    // public void removeBullet()
    // {
    //     //deprecated
    //     if(bullet_count > 0)
    //     {
    //         bullet_count--;
    //     }
    // }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //deprecated unless bullets are added back
        /*
        if (collision.CompareTag("BulletDrop"))
        {
            inventorySound.Play();
            Destroy(collision.gameObject);
            if(!(bullet_count >= max_bullets))
                bullet_count += 1;
        }
        */
        if (collision.CompareTag("Finish") && has_bomb)
        {
            WinManager.WinGame();
        }
    }
}
