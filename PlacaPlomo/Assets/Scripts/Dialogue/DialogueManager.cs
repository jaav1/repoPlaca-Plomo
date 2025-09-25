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

    // CORRECCIÓN: relación pública accesible
    public float relacion;
    private float _targetIconX = 0f;

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
            Canvas.ForceUpdateCanvases();
        }

        AjustarIconoTamanio();
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

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Escape))
            EndDialogue();

        // Mover icono suavemente
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
    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null || dialoguePanel == null) return;

        currentDialogue = dialogue;
        isDialogueActive = true;

        if (cameraControlScript != null) cameraControlScript.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        dialoguePanel.SetActive(true);
        foreach (Transform child in dialoguePanel.transform) child.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        speakerText.text = dialogue.speakerName ?? "Desconocido";
        dialogueText.text = dialogue.dialogueText ?? "";

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

        if (selected.nextDialogue != null)
            StartDialogue(selected.nextDialogue);
        else
            EndDialogue(currentDialogue.targetMissionID);
    }

    public void EndDialogue(string completedNpcId = null)
    {
        // ------------------ CONEXIÓN CON EL SISTEMA DE MISIONES ------------------
        // Reportamos el evento solo si se proporcion un ID de NPC al finalizar
        if (!string.IsNullOrEmpty(completedNpcId))
        {
            // Enva el evento: TriggerType.Dialogue + Target ID (ej: NPC_DonTomas)
            MissionManager.I?.ReportEvent(TriggerType.Dialogue, completedNpcId);
            Debug.Log($"[Mission] Dilogo terminado. Reportando evento: {TriggerType.Dialogue} / {completedNpcId}");
        }
        // -------------------------------------------------------------------------


        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        isDialogueActive = false;
        currentDialogue = null;

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
        Debug.Log($"Relación actual: {relacion} (delta: {delta})");
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
        float h = barH * 1.2f;
        iconoIndicador.sizeDelta = new Vector2(h, h);
    }

    void OnValidate()
    {
        if (relacionBarra != null) relacionBarra.maxValue = maxValor;
        if (inicioRelacion < 0) inicioRelacion = 0;
        if (inicioRelacion > maxValor) inicioRelacion = maxValor;
    }
}
