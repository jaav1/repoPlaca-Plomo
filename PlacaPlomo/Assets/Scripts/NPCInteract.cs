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
        // Oculta el mensaje si est� asignado
        if (mensajeInteraccion != null)
            mensajeInteraccion.SetActive(false);

        if (dialogueManager == null)
            Debug.LogWarning("No asignaste DialogueManager en NPCInteract.");
    }

    private void Update()
    {
        // Solo escuchar la tecla E si el jugador est� cerca y no hay di�logo activo
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueManager == null)
            {
                Debug.LogWarning("DialogueManager no asignado.");
                return;
            }

            // Iniciar di�logo dependiendo si el jugador tiene item o no
            if (jugadorTieneItem && itemDialogue != null)
            {
                Debug.Log("Iniciando di�logo con item.");
                dialogueManager.StartDialogue(itemDialogue);
            }
            else if (defaultDialogue != null)
            {
                Debug.Log("Iniciando di�logo por defecto.");
                dialogueManager.StartDialogue(defaultDialogue);
            }
            else
            {
                Debug.LogWarning("No hay di�logo asignado para este NPC.");
            }

            // Ocultar el mensaje cuando empieza el di�logo
            if (mensajeInteraccion != null)
                mensajeInteraccion.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            // Mostrar mensaje de interacci�n
            if (mensajeInteraccion != null)
                mensajeInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;

            // Cerrar di�logo solo si est� activo
            if (dialogueManager != null && dialogueManager.IsDialogueActive())
                dialogueManager.EndDialogue();

            // Ocultar mensaje de interacci�n
            if (mensajeInteraccion != null)
                mensajeInteraccion.SetActive(false);
        }
    }
}
