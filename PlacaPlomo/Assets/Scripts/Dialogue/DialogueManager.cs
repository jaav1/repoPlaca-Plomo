using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Button[] optionButtons;

    [Header("Scripts to disable during dialogue")]
    public MonoBehaviour cameraControlScript;
    public MonoBehaviour playerMovementScript;

    // Variables para confianza y sospecha
    public float confianzaDelta;
    public float sospechaDelta;

    private Dialogue currentDialogue;
    private bool isDialogueActive = false;

    // Niveles acumulativos
    public float confianzaNPC = 0f;
    public float sospechaMercado = 0f;

    void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        else
            Debug.LogWarning("DialoguePanel no asignado en DialogueManager");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }


    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("Dialogue es null en StartDialogue");
            return;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("DialoguePanel no asignado en DialogueManager");
            return;
        }

        Debug.Log("StartDialogue llamado para: " + dialogue.speakerName);

        currentDialogue = dialogue;
        isDialogueActive = true;

        if (cameraControlScript != null) cameraControlScript.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        dialoguePanel.SetActive(true);

        // Depuración para verificar activación del panel y sus hijos
        Debug.Log("dialoguePanel activo? " + dialoguePanel.activeSelf);

        foreach (Transform child in dialoguePanel.transform)
        {
            Debug.Log("Hijo activo: " + child.name + " - " + child.gameObject.activeSelf);
            child.gameObject.SetActive(true); // Forzar activar hijos por si están desactivados
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        speakerText.text = dialogue.speakerName ?? "Desconocido";
        dialogueText.text = dialogue.dialogueText ?? "";

        // Manejo de opciones
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

        Debug.Log($"Diálogo iniciado: {dialogue.speakerName}, Opciones: {(dialogue.options != null ? dialogue.options.Length : 0)}");
    }

    public void SelectOption(int index)
    {
        if (currentDialogue == null || currentDialogue.options == null || index < 0 || index >= currentDialogue.options.Length)
        {
            Debug.LogWarning("Selección de opción inválida");
            return;
        }

        DialogueOption selected = currentDialogue.options[index];

        // Sumar valores
        confianzaNPC += selected.confianzaDelta;
        sospechaMercado += selected.sospechaDelta;

        Debug.Log($"Confianza actual: {confianzaNPC}, Sospecha actual: {sospechaMercado}");

        if (selected.nextDialogue != null)
        {
            StartDialogue(selected.nextDialogue);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

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

        Debug.Log("Diálogo finalizado");
    }

    public void CloseDialogue() => EndDialogue();
}
