using System.Text;
using TMPro;
using UnityEngine;

public class UIBulletCounter : MonoBehaviour
{
    [SerializeField] TMP_Text bulletText;
    private GameObject _player;
    private TemporaryInventory _inventory;
    // Start is called before the first frame update
    void Start()
    {
        //This is where it is broken if it is future me :p
        _inventory = FindObjectOfType<TemporaryInventory>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_inventory)
        {
            // StringBuilder stringBuilder = new StringBuilder();
            // stringBuilder.Append("Bullet Count: ");
            // stringBuilder.Append(_inventory.bullet_count);
            // bulletText.text = stringBuilder.ToString();
        }
    }
}
