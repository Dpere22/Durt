using UnityEngine;

public class KillBox : MonoBehaviour
{
    // Start is called before the first frame update

    // Update is called once per frame

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collided = collision.gameObject;
        
        if(collided.CompareTag("Player")) 
        {
            LoseManager.LoseGame();
        }
    }
}
