using System.Collections.Generic;
using UnityEngine;

public class SignController : MonoBehaviour
{
    [SerializeField] private Canvas canvas; //the prefab
    private Canvas _spawnedCanvas;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canvas != null && _spawnedCanvas == null)
        {
            _spawnedCanvas = Instantiate(canvas, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || _spawnedCanvas == null) return;
        Destroy(_spawnedCanvas.gameObject);
        _spawnedCanvas = null;
    }
}
