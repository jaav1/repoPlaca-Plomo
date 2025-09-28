using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue startingDialogue;
    public DialogueManager dialogueManager;
    public NPCInteract npc; // Referencia al NPC que inicia el di�logo
    public int startingAudioIndex = 0; // �ndice del audio que se reproduce al inicio

    // M�todo p�blico que otros scripts pueden llamar (por ejemplo, NPCInteract)
    public void TriggerDialogue()
    {
        if (dialogueManager != null && startingDialogue != null && npc != null)
        {
            dialogueManager.StartDialogue(startingDialogue, npc, startingAudioIndex);
        }
        else
        {
            Debug.LogWarning("Faltan referencias en DialogueTrigger: DialogueManager, Dialogue o NPC no asignados.");
        }
    }
}
