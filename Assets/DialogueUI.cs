using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI dialogueText;
    void OnEnable()
    {
        DialogueManager.OnDialogueStarted += ShowDialogueUI;
        DialogueManager.OnDialogueEnded += HideDialogueUI;
    }

    void OnDisable()
    {
        DialogueManager.OnDialogueStarted -= ShowDialogueUI;
        DialogueManager.OnDialogueEnded -= HideDialogueUI;
    }

    void ShowDialogueUI(string dialogue, Color color)
    {
        dialogueUI.SetActive(true);
        dialogueText.text = dialogue;
        dialogueText.color = color;
    }

    void HideDialogueUI()
    {
        dialogueUI.SetActive(false);
        dialogueText.text = "";
    }
}
