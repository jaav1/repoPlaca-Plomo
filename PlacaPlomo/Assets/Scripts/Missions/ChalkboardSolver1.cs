using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ChalkboardSolver1 : MonoBehaviour
{
    // === ASIGNACIONES EN EL INSPECTOR ===
    [Header("UI References")]
    [SerializeField] private GameObject chalkboardUI; // El panel principal de la pizarra
    [SerializeField] private Button buttonRecibos;
    [SerializeField] private Button buttonDocParroquia;
    [SerializeField] private Button buttonLazaro;
    [SerializeField] private Button buttonTaberna;
    [SerializeField] private TMP_Text feedbackText; // Texto para dar feedback (ej. "Conectado", "Incorrecto")

    [Header("Mission Settings")]
    [SerializeField] private string successStepId = "M2-01"; // Paso al que avanzar
    [SerializeField] private string targetTriggerID = "NODE_Recibos,NODE_DocParroquia,NODE_Lazaro,NODE_Taberna"; // Del CSV

    // === Lógica de Conexión ===
    private readonly List<string> connectionSequence = new();
    private const string CORRECT_SOLUTION = "NODE_Recibos->NODE_DocParroquia->NODE_Lazaro->NODE_Taberna";
    // ^ Asegúrate de que esta sea la secuencia correcta que el jugador debe ingresar.

    private void Awake()
    {
        // Ocultar al inicio
        chalkboardUI.SetActive(false);

        // ASIGNACIÓN DE LISTENERS CORREGIDA:
        buttonRecibos.onClick.AddListener(() => AddNodeToSequence("NODE_Recibos"));
        buttonDocParroquia.onClick.AddListener(() => AddNodeToSequence("NODE_DocParroquia"));
        buttonLazaro.onClick.AddListener(() => AddNodeToSequence("NODE_Lazaro"));
        buttonTaberna.onClick.AddListener(() => AddNodeToSequence("NODE_Taberna"));
    }

    // Método llamado por los botones
    private void AddNodeToSequence(string nodeId)
    {
        if (connectionSequence.Contains(nodeId))
        {
            feedbackText.text = "Ese nodo ya está en la secuencia. Haz clic en 'Borrar'.";
            return;
        }

        connectionSequence.Add(nodeId);
        UpdateFeedbackUI();

        // Ahora chequea por 4 nodos, ya que son 4 elementos
        if (connectionSequence.Count == 4)
        {
            CheckSolution();
        }
    }

    private void UpdateFeedbackUI()
    {
        string currentSequence = string.Join(" ? ", connectionSequence);
        feedbackText.text = $"Secuencia actual: {currentSequence}";
    }

    private void CheckSolution()
    {
        string currentPath = string.Join("->", connectionSequence);

        if (currentPath == CORRECT_SOLUTION)
        {
            feedbackText.text = "¡CONEXIÓN EXITOSA! Las pistas coinciden.";

            // 1. OBTENER LA LISTA DE NODOS INDIVIDUALMENTE
            // (targetTriggerID es "NODE_Recibos,NODE_DocParroquia,NODE_Lazaro,NODE_Taberna")
            string[] requiredNodes = targetTriggerID.Split(',');

            // 2. Reportar cada nodo de forma individual
            foreach (string nodeId in requiredNodes)
            {
                MissionManager.I?.ReportEvent(TriggerType.SolveGraph, nodeId.Trim());
            }

            // Oculta la pizarra después de un momento
            Invoke(nameof(HideChalkboard), 0.5f);

        }
        else
        {
            feedbackText.text = "Conexión incorrecta. ¡Vuelve a intentarlo!";
            Invoke(nameof(ClearSequence), 1.5f); // Limpia la secuencia tras un breve error
        }
    }

    public void ClearSequence()
    {
        connectionSequence.Clear();
        feedbackText.text = "Secuencia borrada. Conecta las 4 pistas.";
    }

    public void ShowChalkboard()
    {
        ClearSequence();
        chalkboardUI.SetActive(true);
        // Desbloquear/Mostrar cursor al abrir la UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideChalkboard()
    {
        chalkboardUI.SetActive(false);
        // Bloquear/Ocultar cursor al cerrar la UI para volver al gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}