using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [Header("Diálogos")]
    public Dialogue defaultDialogue;
    public Dialogue itemDialogue;
    public Dialogue desconfianzaDialogue;
    public Dialogue confianzaAltaDialogue;
    public DialogueManager dialogueManager;

    [Header("Interacción UI")]
    public GameObject mensajeInteraccion;

    [Header("Ítem requerido (nombre EXACTO)")]
    public string nombreItemRequerido = "Carpeta";

    [Header("Gestos / Animaciones")]
    public Animator animator;
    public SpriteRenderer portraitRenderer; // Sprite del NPC
    public Sprite portraitNeutral;
    public Sprite portraitHappy;
    public Sprite portraitAngry;

    private bool jugadorCerca = false;

    private void Awake()
    {
        if (mensajeInteraccion) mensajeInteraccion.SetActive(false);
        if (!dialogueManager) dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void Update()
    {
        if (!jugadorCerca) return;

        // Mostrar mensaje solo si no hay diálogo activo
        if (mensajeInteraccion && (!dialogueManager || !dialogueManager.IsDialogueActive()))
            mensajeInteraccion.SetActive(true);

        // Actualizar sprite y animación según relación
        UpdateExpression();

        // Detectar E para iniciar diálogo
        if (Input.GetKeyDown(KeyCode.E) && dialogueManager)
        {
            IniciarDialogoSegunRelacion();
        }
    }

    private void UpdateExpression()
    {
        if (portraitRenderer == null || dialogueManager == null) return;

        float relacionActual = dialogueManager.relacion; // usar propiedad pública de DialogueManager
        if (relacionActual <= dialogueManager.bajoThreshold)
            SetExpression(2); // enojado
        else if (relacionActual >= dialogueManager.altoThreshold)
            SetExpression(1); // feliz
        else
            SetExpression(0); // neutral
    }

    private void IniciarDialogoSegunRelacion()
    {
        bool tieneItem = false;
        if (!string.IsNullOrEmpty(nombreItemRequerido) && TryGetComponent(out RadialInventoryManager inventarioJugador))
            tieneItem = inventarioJugador.HasItem(nombreItemRequerido);

        float relacion = dialogueManager.relacion;

        // Determinar diálogo y expresión según la relación
        if (relacion <= dialogueManager.bajoThreshold && desconfianzaDialogue != null)
        {
            SetExpression(2); // enojado
            dialogueManager.StartDialogue(desconfianzaDialogue);
        }
        else if (relacion >= dialogueManager.altoThreshold && confianzaAltaDialogue != null)
        {
            SetExpression(1); // feliz
            dialogueManager.StartDialogue(confianzaAltaDialogue);
        }
        else if (tieneItem && itemDialogue != null)
        {
            SetExpression(0); // neutral
            dialogueManager.StartDialogue(itemDialogue);
        }
        else if (defaultDialogue != null)
        {
            SetExpression(0); // neutral
            dialogueManager.StartDialogue(defaultDialogue);
        }

        if (mensajeInteraccion) mensajeInteraccion.SetActive(false);
    }

    private void SetExpression(int expr)
    {
        // Cambiar animación
        if (animator != null)
            animator.SetInteger("Expression", expr);

        // Cambiar sprite
        if (portraitRenderer != null)
        {
            switch (expr)
            {
                case 0: portraitRenderer.sprite = portraitNeutral; break;
                case 1: portraitRenderer.sprite = portraitHappy; break;
                case 2: portraitRenderer.sprite = portraitAngry; break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorCerca = true;
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
