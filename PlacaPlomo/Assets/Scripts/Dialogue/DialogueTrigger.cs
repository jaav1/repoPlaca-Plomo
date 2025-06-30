using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue startingDialogue;
    public DialogueManager dialogueManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueManager.StartDialogue(startingDialogue);
        }
    }
}
