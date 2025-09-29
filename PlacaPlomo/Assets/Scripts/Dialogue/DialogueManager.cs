using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements (Diálogo)")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Button[] optionButtons;

    [Header("HUD permanente")]
    public GameObject hudRoot;
    public Slider relacionBarra;
    public RectTransform iconoIndicador;
    public Image iconoImagen;
    public Sprite iconoBajo;
    public Sprite iconoMedio;
    public Sprite iconoAlto;

    [Header("Configuración del icono")]
    [Range(0.1f, 1f)]
    public float iconScale = 0.5f;

    [Header("Configuración de relación")]
    public float maxValor = 100f;
    [Range(0, 100)]
    public float inicioRelacion = 50f;
    public float bajoThreshold = 30f;
    public float altoThreshold = 70f;
    public float iconMoveSpeed = 10f;

    [Header("Scripts a desactivar durante diálogo")]
    public MonoBehaviour cameraControlScript;
    public MonoBehaviour playerMovementScript;

    private Dialogue currentDialogue;
    private bool isDialogueActive = false;

    // relación pública accesible
    public float relacion;
    private float _targetIconX = 0f;

    // Para audio por línea
    private NPCInteract currentNPC;
    private int currentAudioIndex = 0;

    void Awake()
    {
        if (hudRoot != null) hudRoot.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        relacion = Mathf.Clamp(inicioRelacion, 0f, maxValor);

        if (relacionBarra != null)
        {
            relacionBarra.minValue = 0f;
            relacionBarra.maxValue = maxValor;
            relacionBarra.value = relacion;
        }

        CalcularTargetIconX();

        if (iconoIndicador != null)
        {
            Vector2 p = iconoIndicador.anchoredPosition;
            p.x = _targetIconX;
            p.y = 0f;
            iconoIndicador.anchoredPosition = p;
        }

        ActualizarSpriteSegunValor();
    }

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        AjustarIconoTamanio();
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Escape))
            EndDialogue();

        if (iconoIndicador != null && relacionBarra != null)
        {
            Vector2 pos = iconoIndicador.anchoredPosition;
            pos.x = Mathf.Lerp(pos.x, _targetIconX, Time.deltaTime * iconMoveSpeed);
            pos.y = 0f;

            float halfWidth = relacionBarra.GetComponent<RectTransform>().rect.width * 0.5f;
            float halfIcon = iconoIndicador.rect.width * 0.5f;
            pos.x = Mathf.Clamp(pos.x, -halfWidth + halfIcon, halfWidth - halfIcon);

            iconoIndicador.anchoredPosition = pos;
        }
    }

    public bool IsDialogueActive() => isDialogueActive;

    // ---------------- DIÁLOGO ----------------
    public void StartDialogue(Dialogue dialogue, NPCInteract npc, int audioIndex = 0)
    {
        if (dialogue == null || dialoguePanel == null) return;

        currentDialogue = dialogue;
        currentNPC = npc;
        currentAudioIndex = audioIndex;
        isDialogueActive = true;

        if (cameraControlScript != null) cameraControlScript.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        dialoguePanel.SetActive(true);
        foreach (Transform child in dialoguePanel.transform) child.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        speakerText.text = dialogue.speakerName ?? "Desconocido";
        dialogueText.text = dialogue.dialogueText ?? "";

        // Reproducir audio desde el NPC
        currentNPC?.PlayDialogueClip(currentAudioIndex);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (dialogue.options != null && i < dialogue.options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                int index = i;
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = dialogue.options[i].optionText ?? "Opción";
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
                optionButtons[i].onClick.RemoveAllListeners();
            }
        }
    }

    public void SelectOption(int index)
    {
        if (currentDialogue == null || currentDialogue.options == null) return;
        if (index < 0 || index >= currentDialogue.options.Length) return;

        DialogueOption selected = currentDialogue.options[index];

        // Cambiar relación según la opción
        float delta = selected.confianzaDelta - selected.sospechaDelta;
        AddRelacionDelta(delta);

        // --- LÓGICA DE DECISIÓN DE MISIÓN (Generalizada) ---

        // 1. Condición: El diálogo est terminando Y el paso actual del MissionManager es un paso de Elección.
        // NOTA: Para que esto funcione, el ScriptableObject Dialogue DEBE tener
        // targetMissionID igual al step_id (ej: "M1-06")

        if (selected.nextDialogue == null && MissionManager.I != null && MissionManager.I.IsCurrentStepAChoice())
        {
            // Conseguir las ramas de decisión del paso actual (ej: "ENTREGAR / GUARDAR")
            string choiceBranchRaw = MissionManager.I.GetCurrentStepChoiceBranch();

            // Si la opcin seleccionada es la primera opcin del di logo (ndice 0)
            // asumimos que es la primera rama del ChoiceBranchRaw.
            // Esto requiere que el orden de las opciones del SO Dialogue coincida con el CSV.

            // Asumimos que la rama de decisin es el texto de la opcin seleccionada.
            string choice = selected.optionText.Trim().ToUpper();

            EndDialogue(null);
            MissionManager.I?.SubmitChoice(choice);
            return;
        }
        // ---------------------------------------------

        // Lógica normal (avanzar a siguiente nodo o terminar)
        if (selected.nextDialogue != null)
        {
            currentAudioIndex++; // siguiente clip del NPC
            StartDialogue(selected.nextDialogue, currentNPC, currentAudioIndex);
        }
        else
        {
            // Si el diálogo no es la decisión de Claudia, termina y reporta el NPC ID.
            EndDialogue(currentDialogue.targetMissionID);
        }
    }

    public void EndDialogue(string completedNpcId = null)
    {
        if (!string.IsNullOrEmpty(completedNpcId))
        {
            MissionManager.I?.ReportEvent(TriggerType.Dialogue, completedNpcId);
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        isDialogueActive = false;
        currentDialogue = null;
        currentNPC = null;

        if (cameraControlScript != null) cameraControlScript.enabled = true;
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        foreach (Button btn in optionButtons)
        {
            btn.gameObject.SetActive(false);
            btn.onClick.RemoveAllListeners();
        }
    }

    public void CloseDialogue() => EndDialogue();

    // ---------------- HUD helpers ----------------
    public void SetRelacion(float value)
    {
        relacion = Mathf.Clamp(value, 0f, maxValor);
        if (relacionBarra != null) relacionBarra.value = relacion;

        CalcularTargetIconX();
        ActualizarSpriteSegunValor();
    }

    public void AddRelacionDelta(float delta)
    {
        SetRelacion(relacion + delta);
    }

    void CalcularTargetIconX()
    {
        if (relacionBarra == null) return;

        float normalized = (maxValor > 0f) ? (relacion / maxValor) : 0f;

        RectTransform sliderRect = relacionBarra.GetComponent<RectTransform>();
        float halfWidth = sliderRect.rect.width * 0.5f;
        float halfIcon = (iconoIndicador != null) ? iconoIndicador.rect.width * 0.5f : 0f;

        float left = -halfWidth + halfIcon;
        float right = halfWidth - halfIcon;

        _targetIconX = Mathf.Lerp(left, right, normalized);
    }

    void ActualizarSpriteSegunValor()
    {
        if (iconoImagen == null) return;

        if (relacion <= bajoThreshold)
        {
            if (iconoBajo != null) iconoImagen.sprite = iconoBajo;
        }
        else if (relacion >= altoThreshold)
        {
            if (iconoAlto != null) iconoImagen.sprite = iconoAlto;
        }
        else
        {
            if (iconoMedio != null) iconoImagen.sprite = iconoMedio;
        }
    }

    void AjustarIconoTamanio()
    {
        if (relacionBarra == null || iconoIndicador == null) return;

        float barH = relacionBarra.GetComponent<RectTransform>().rect.height;
        float h = barH * iconScale;
        iconoIndicador.sizeDelta = new Vector2(h, h);
    }

    void OnValidate()
    {
        if (relacionBarra != null) relacionBarra.maxValue = maxValor;
        if (inicioRelacion < 0) inicioRelacion = 0;
        if (inicioRelacion > maxValor) inicioRelacion = maxValor;
    }
}
