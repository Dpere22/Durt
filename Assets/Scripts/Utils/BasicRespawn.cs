using UnityEngine;

public class BasicRespawn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LoseManager.OnRespawn += HandleRespawn;
    }

    private void OnDestroy()
    {
        LoseManager.OnRespawn -= HandleRespawn;
    }
    // Update is called once per frame
    private void HandleRespawn()
    {
        // Loop through each child Transform
        foreach (Transform child in transform)
        {
            // Enable the GameObject of each child
            var gObj = child.gameObject;
            if(gObj != null) gObj.SetActive(true);
        }
    }
}
