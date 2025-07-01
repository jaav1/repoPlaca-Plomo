using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue startingDialogue;
    public DialogueManager dialogueManager;

    // M�todo p�blico que otros scripts pueden llamar (por ejemplo, NPCInteract)
    public void TriggerDialogue()
    {
        dialogueManager.StartDialogue(startingDialogue);
    }
}
