using UnityEngine;

public class MissionDialogueTrigger : MonoBehaviour
{/*
    [Header("Configuracin de la Misin")]
    [Tooltip("El ID del Target que est en el CSV para este paso (ej: NPC_SrOcampo)")]
    public string missionTargetId = "NPC_SrOcampo";

    [Tooltip("El nombre del archivo de dilogo que debe iniciar.")]
    public string dialogueFileName = "M1_SrOcampo_Intro";

    // Referencia al MissionManager (debe ser un Singleton)
    // Asumimos que tienes MissionManager.I para acceder a la instancia nica.

    // ----------------------------------------------------
    // 1. Mtodo para iniciar la interaccin (llamado por el jugador al hacer clic)
    // ----------------------------------------------------
    public void StartInteraction()
    {
        // Pasa el control del final del dilogo a este script.
        DialogueManager.I?.StartDialogue(dialogueFileName, OnDialogueEnd);
        Debug.Log($"[MissionDialogueTrigger] Iniciando dilogo con {missionTargetId}.");
    }

    // ----------------------------------------------------
    // 2. Mtodo Callback: Se llama automticamente al final del dilogo.
    // ----------------------------------------------------
    private void OnDialogueEnd()
    {
        // 1. Reportar el evento de dilogo al MissionManager.
        // Esto verifica el paso M1-07A (Trigger: Dialogue, Target: NPC_SrOcampo).
        MissionManager.I?.ReportEvent(TriggerType.Dialogue, missionTargetId);

        // 2. Ejecutar la accin secundaria de la misin (opcional, si es necesaria)
        // El CSV especifica: DIA_Ocampo=1 y Marca pista: Huella_neumatico
        MissionManager.I?.AddFlag("DIA_Ocampo"); // Marca el flag de dilogo
        MissionManager.I?.AddClue("Huella_neumatico"); // Aade la pista al inventario

        Debug.Log($"[MissionDialogueTrigger] Dilogo con {missionTargetId} terminado. Evento de misin reportado. Pista 'Huella_neumatico' aada.");

        // Opcional: Desactivar ms interacciones con Ocampo para este paso.
        // gameObject.GetComponent<Collider>().enabled = false;
    }*/
}