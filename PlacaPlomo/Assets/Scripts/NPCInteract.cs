using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    public Dialogue defaultDialogue;
    public Dialogue itemDialogue;
    public DialogueManager dialogueManager;
    public bool jugadorTieneItem = false;

    public GameObject mensajeInteraccion; // UI "Presiona E para hablar"

    private bool jugadorCerca = false;

    private void Start()
    {
        // Oculta el mensaje si está asignado
        if (mensajeInteraccion != null)
            mensajeInteraccion.SetActive(false);

        if (dialogueManager == null)
            Debug.LogWarning("No asignaste DialogueManager en NPCInteract.");
    }

    private void Update()
    {
        // Solo escuchar la tecla E si el jugador está cerca y no hay diálogo activo
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueManager == null)
            {
                Debug.LogWarning("DialogueManager no asignado.");
                return;
            }

            // Iniciar diálogo dependiendo si el jugador tiene item o no
            if (jugadorTieneItem && itemDialogue != null)
            {
                Debug.Log("Iniciando diálogo con item.");
                dialogueManager.StartDialogue(itemDialogue);
            }
            else if (defaultDialogue != null)
            {
                Debug.Log("Iniciando diálogo por defecto.");
                dialogueManager.StartDialogue(defaultDialogue);
            }
            else
            {
                Debug.LogWarning("No hay diálogo asignado para este NPC.");
            }

            // Ocultar el mensaje cuando empieza el diálogo
            if (mensajeInteraccion != null)
                mensajeInteraccion.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            // Mostrar mensaje de interacción
            if (mensajeInteraccion != null)
                mensajeInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;

            // Cerrar diálogo solo si está activo
            if (dialogueManager != null && dialogueManager.IsDialogueActive())
                dialogueManager.EndDialogue();

            // Ocultar mensaje de interacción
            if (mensajeInteraccion != null)
                mensajeInteraccion.SetActive(false);
        }
    }
}
