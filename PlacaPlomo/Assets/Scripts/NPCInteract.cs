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
    [SerializeField] private string nombreItemRequerido = "Carpeta";

    [Header("Gestos / Animaciones")]
    public Animator animator;
    public SpriteRenderer portraitRenderer;
    public Sprite portraitNeutral;
    public Sprite portraitHappy;
    public Sprite portraitAngry;

    [Header("Audio del NPC")]
    public AudioSource npcAudioSource;
    public AudioClip[] dialogueClips; // 0 = línea inicial, 1..n = respuestas

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

        float relacionActual = dialogueManager.relacion;
        if (relacionActual <= dialogueManager.bajoThreshold)
            SetExpression(2);
        else if (relacionActual >= dialogueManager.altoThreshold)
            SetExpression(1);
        else
            SetExpression(0);
    }

    private void IniciarDialogoSegunRelacion()
    {
        bool tieneItem = false;
        if (!string.IsNullOrEmpty(nombreItemRequerido) && TryGetComponent(out RadialInventoryManager inventarioJugador))
            tieneItem = inventarioJugador.HasItem(nombreItemRequerido);

        float relacion = dialogueManager.relacion;

        Dialogue dialogueToStart = null;

        if (relacion <= dialogueManager.bajoThreshold && desconfianzaDialogue != null)
        {
            SetExpression(2);
            dialogueToStart = desconfianzaDialogue;
        }
        else if (relacion >= dialogueManager.altoThreshold && confianzaAltaDialogue != null)
        {
            SetExpression(1);
            dialogueToStart = confianzaAltaDialogue;
        }
        else if (tieneItem && itemDialogue != null)
        {
            SetExpression(0);
            dialogueToStart = itemDialogue;
        }
        else if (defaultDialogue != null)
        {
            SetExpression(0);
            dialogueToStart = defaultDialogue;
        }

        if (dialogueToStart != null)
        {
            // Reproducir audio de línea inicial
            PlayDialogueClip(0);
            // Iniciar diálogo pasando la referencia al NPC y el índice de audio inicial
            dialogueManager.StartDialogue(dialogueToStart, this, 0);
        }

        if (mensajeInteraccion) mensajeInteraccion.SetActive(false);
    }

    private void SetExpression(int expr)
    {
        if (animator != null)
            animator.SetInteger("Expression", expr);

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

    public void PlayDialogueClip(int index)
    {
        if (npcAudioSource == null || dialogueClips == null || dialogueClips.Length == 0) return;
        if (index < 0 || index >= dialogueClips.Length) return;

        npcAudioSource.Stop();
        npcAudioSource.clip = dialogueClips[index];
        npcAudioSource.loop = false;
        npcAudioSource.Play();
    }
}
