using System.Collections;
using UnityEngine;
using TMPro;
public class DialogController : MonoBehaviour
{
    
    public TextMeshProUGUI dialogText;
    public string[] dialogLines;

    public float textSpeed;

    private int _index;
    
    // Start is called before the first frame update
    void Start()
    {
        dialogText.text = string.Empty;
        StartDialog();
    }
    void StartDialog()
    {
        _index = 0;
        StartCoroutine(nameof(TypeLine));
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (dialogText.text == dialogLines[_index])
        {
            NextLine();
        }
        else
        {
            StopAllCoroutines();
            dialogText.text = dialogLines[_index];
        }
    }

    private IEnumerator TypeLine()
    {
        foreach (var c in dialogLines[_index].ToCharArray())
        {
            dialogText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void NextLine()
    {
        if (_index < dialogLines.Length - 1)
        {
            _index++;
            dialogText.text = string.Empty;
            StartCoroutine(nameof(TypeLine));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
