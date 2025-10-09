using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ChalkboardSolver : MonoBehaviour
{
    // === ASIGNACIONES EN EL INSPECTOR ===
    [Header("UI References")]
    [SerializeField] private GameObject chalkboardUI; // El panel principal de la pizarra
    [SerializeField] private Button buttonPlaca;
    [SerializeField] private Button buttonPaquete;
    [SerializeField] private Button buttonNota;
    [SerializeField] private TMP_Text feedbackText; // Texto para dar feedback (ej. "Conectado", "Incorrecto")

    [Header("Mission Settings")]
    [SerializeField] private string successStepId = "M2-01"; // Paso al que avanzar
    [SerializeField] private string targetTriggerID = "NODE_Placa,NODE_Paquete,NODE_NotaNombres"; // Del CSV

    // === Lógica de Conexión ===
    private readonly List<string> connectionSequence = new();
    private const string CORRECT_SOLUTION = "NODE_Placa->NODE_Paquete->NODE_NotaNombres"; // Placa->Paquete->Nota

    private void Awake()
    {
        // Ocultar al inicio
        chalkboardUI.SetActive(false);

        // Asignar los listeners de click
        buttonPlaca.onClick.AddListener(() => AddNodeToSequence("NODE_Placa"));
        buttonPaquete.onClick.AddListener(() => AddNodeToSequence("NODE_Paquete"));
        buttonNota.onClick.AddListener(() => AddNodeToSequence("NODE_NotaNombres"));
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

        if (connectionSequence.Count == 3)
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

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // *******************************************************
            // 1. OBTENER LA LISTA DE NODOS INDIVIDUALMENTE
            //    (targetTriggerID es "NODE_Placa,NODE_Paquete,NODE_NotaNombres")
            // *******************************************************
            string[] requiredNodes = targetTriggerID.Split(',');

            // 2. Reportar cada nodo de forma individual
            foreach (string nodeId in requiredNodes)
            {
                // El MissionManager usa este ReportEvent para marcar cada requisito.
                MissionManager.I?.ReportEvent(TriggerType.SolveGraph, nodeId.Trim());
            }

            // Si todos los requisitos fueron marcados, el MissionManager
            // verificará automáticamente que _satisfiedForThisStep.Count == current.Requirements.Count
            // y avanzará la misión.

            // *******************************************************

            // Oculta la pizarra después de un momento
            Invoke(nameof(HideChalkboard), 2.0f);

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
        feedbackText.text = "Secuencia borrada. Conecta las 3 pistas.";
    }

    public void ShowChalkboard()
    {
        ClearSequence();
        chalkboardUI.SetActive(true);
        // Opcional: Deshabilitar controles de movimiento del jugador
    }

    public void HideChalkboard()
    {
        chalkboardUI.SetActive(false);
        // Opcional: Re-habilitar controles de movimiento del jugador
    }
}