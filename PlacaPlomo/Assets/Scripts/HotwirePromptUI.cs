using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotwirePromptUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panel;          // Panel contenedor
    [SerializeField] private TextMeshProUGUI messageText;          // Texto del mensaje
    [SerializeField] private Button startButton;        // Botón “Encendido forzado”

    [Header("Opcional (para el siguiente paso)")]
    [SerializeField] private HotwirePuzzleManager puzzleManager; // Lo asignaremos luego

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonPressed);
    }

    public void Show(string message)
    {
        if (messageText != null)
            messageText.text = message + "\n\nPulsa el botón para intentar encenderlo sin llave.";

        if (panel != null)
            panel.SetActive(true);

        // ?? Habilitamos cursor para interactuar con UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }


    // Llamado al pulsar el botón
    public void OnStartButtonPressed()
    {
        // En el siguiente paso conectaremos esto con el puzzle real
        if (puzzleManager != null)
        {
            puzzleManager.StartPuzzle();
        }
        else
        {
            Debug.Log("HotwirePromptUI: Inicio de encendido forzado solicitado (manager no asignado aún).");
        }

        Hide();
    }
}