using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [Header("Di�logos")]
    public Dialogue defaultDialogue;      // Di�logo por defecto
    public Dialogue itemDialogue;         // Di�logo si el jugador tiene el �tem
    public DialogueManager dialogueManager;

    [Header("Interacci�n UI")]
    public GameObject mensajeInteraccion; // "Presiona E para hablar"

    [Header("�tem requerido (nombre EXACTO como en tu inventario)")]
    public string nombreItemRequerido = "Carpeta";

    [Tooltip("Asigna el inventario del jugador aqu�. Si lo dejas vac�o, el script lo buscar� autom�ticamente.")]
    public RadialInventoryManager inventarioJugador;

    private bool jugadorCerca = false;

    private void Awake()
    {
        if (mensajeInteraccion) mensajeInteraccion.SetActive(false);
        if (!dialogueManager) dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void Start()
    {
        // Respaldo: si no est� asignado, intenta encontrar uno en la escena
        if (!inventarioJugador)
        {
            inventarioJugador = FindFirstObjectByType<RadialInventoryManager>();
            if (!inventarioJugador)
                Debug.LogWarning("[NPCInteract] No se encontr� RadialInventoryManager en la escena. Puedes asignarlo en el inspector.");
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
        Debug.Log($"[NPCInteract] Revisi�n de �tem -> requerido: '{nombreItemRequerido}' | tieneItem={tieneItem} | inventario={(inventarioJugador ? "OK" : "NULL")}");

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
            Debug.LogWarning("[NPCInteract] No hay di�logos asignados.");
        }

        if (mensajeInteraccion) mensajeInteraccion.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorCerca = true;

        // Captura el inventario desde el propio Player que entr� al trigger
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
