using System;
using UnityEngine;
public class DialogueManager : MonoBehaviour
{
    // Example events
    public static event Action<string, Color> OnDialogueStarted;
    public static event Action OnDialogueEnded;

    // Raise events (called by managers or gameplay systems)
    public static void DialogueStarted(string dialogue, Color dialogueColor)
    {
        OnDialogueStarted?.Invoke(dialogue, dialogueColor);
    }

    public static void DialogueEnded()
    {
        OnDialogueEnded?.Invoke();
    }
}
