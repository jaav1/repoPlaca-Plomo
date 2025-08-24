using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [Header("Diálogos")]
    public Dialogue defaultDialogue;      // Diálogo por defecto
    public Dialogue itemDialogue;         // Diálogo si el jugador tiene el ítem
    public DialogueManager dialogueManager;

    [Header("Interacción UI")]
    public GameObject mensajeInteraccion; // "Presiona E para hablar"

    [Header("Ítem requerido (nombre EXACTO como en tu inventario)")]
    public string nombreItemRequerido = "Carpeta";

    [Tooltip("Asigna el inventario del jugador aquí. Si lo dejas vacío, el script lo buscará automáticamente.")]
    public RadialInventoryManager inventarioJugador;

    private bool jugadorCerca = false;

    private void Awake()
    {
        if (mensajeInteraccion) mensajeInteraccion.SetActive(false);
        if (!dialogueManager) dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void Start()
    {
        // Respaldo: si no está asignado, intenta encontrar uno en la escena
        if (!inventarioJugador)
        {
            inventarioJugador = FindFirstObjectByType<RadialInventoryManager>();
            if (!inventarioJugador)
                Debug.LogWarning("[NPCInteract] No se encontró RadialInventoryManager en la escena. Puedes asignarlo en el inspector.");
        }
    }

    private void Update()
    {
        if (!jugadorCerca || !Input.GetKeyDown(KeyCode.E)) return;

        if (!dialogueManager)
        {
            Debug.LogWarning("[NPCInteract] DialogueManager no asignado.");
            return;
        }

        bool tieneItem = inventarioJugador && inventarioJugador.HasItem(nombreItemRequerido);
        Debug.Log($"[NPCInteract] Revisión de ítem -> requerido: '{nombreItemRequerido}' | tieneItem={tieneItem} | inventario={(inventarioJugador ? "OK" : "NULL")}");

        if (tieneItem && itemDialogue)
        {
            dialogueManager.StartDialogue(itemDialogue);
        }
        else if (defaultDialogue)
        {
            dialogueManager.StartDialogue(defaultDialogue);
        }
        else
        {
            Debug.LogWarning("[NPCInteract] No hay diálogos asignados.");
        }

        if (mensajeInteraccion) mensajeInteraccion.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorCerca = true;

        // Captura el inventario desde el propio Player que entró al trigger
        if (!inventarioJugador)
        {
            inventarioJugador = other.GetComponentInParent<RadialInventoryManager>();
            if (!inventarioJugador)
                inventarioJugador = other.GetComponent<RadialInventoryManager>();
        }

        if (mensajeInteraccion) mensajeInteraccion.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorCerca = false;

        if (dialogueManager && dialogueManager.IsDialogueActive())
            dialogueManager.EndDialogue();

        if (mensajeInteraccion) mensajeInteraccion.SetActive(false);
    }
}
