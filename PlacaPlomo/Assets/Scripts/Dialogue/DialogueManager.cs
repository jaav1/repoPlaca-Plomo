using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Button[] optionButtons;

    public MonoBehaviour cameraControlScript;
    public MonoBehaviour playerMovementScript;

    public float confianzaDelta;
    public float sospechaDelta;


    private Dialogue currentDialogue;
    private bool isDialogueActive = false;

    // 🔶 NUEVO: Niveles acumulativos de confianza y sospecha
    public float confianzaNPC = 0f;
    public float sospechaMercado = 0f;

    void Awake()
    {
        dialoguePanel.SetActive(false);
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

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null) return;

        currentDialogue = dialogue;
        isDialogueActive = true;

        if (cameraControlScript != null) cameraControlScript.enabled = false;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        dialoguePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        speakerText.text = dialogue.speakerName;
        dialogueText.text = dialogue.dialogueText;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < dialogue.options.Length)
            {
                optionButtons[i].gameObject.SetActive(true);
                int index = i;
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = dialogue.options[i].optionText;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectOption(int index)
    {
        DialogueOption selected = currentDialogue.options[index];

        // 🔶 SUMA los valores de confianza y sospecha de la opción seleccionada
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
    }

    public void CloseDialogue() => EndDialogue();
}
