using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    public Dialogue defaultDialogue;
    public Dialogue itemDialogue;
    public DialogueManager dialogueManager;
    public bool jugadorTieneItem = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (jugadorTieneItem && itemDialogue != null)
                dialogueManager.StartDialogue(itemDialogue);
            else
                dialogueManager.StartDialogue(defaultDialogue);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueManager.CloseDialogue(); // Cierra al alejarse
        }
    }
}
