using UnityEngine;

namespace UI
{
    public class SignController : MonoBehaviour
    {
        [TextArea] public string dialogueText;
        [SerializeField] private Color dialogueColor = Color.black;
        private void OnTriggerEnter2D(Collider2D other)
        {
            DialogueManager.DialogueStarted(dialogueText, dialogueColor);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            DialogueManager.DialogueEnded();
        }
    }
}
