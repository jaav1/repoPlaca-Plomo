using UnityEngine;
using System.Collections.Generic;

public class CablePuzzle : MonoBehaviour
{
    [Header("Configuración del puzzle")]
    [SerializeField] private List<int> correctOrder = new List<int> { 12, 21 }; // Ejemplo: A1+B2 = 12, A2+B1 = 21

    private List<int> playerOrder = new List<int>();
    private int selectedLeft = -1;
    private bool failed = false;

    [Header("Referencia al manager")]
    [SerializeField] private HotwirePuzzleManager puzzleManager;
    [SerializeField] private PuzzleManager PuzzleManager;

    public void SelectLeft(int id)
    {
        if (failed || playerOrder.Count >= correctOrder.Count) return;

        selectedLeft = id;
        Debug.Log($"?? Seleccionado extremo izquierdo: {id}");
    }

    public void SelectRight(int id)
    {
        if (failed || selectedLeft == -1) return;

        int connectionId = selectedLeft * 10 + id;
        playerOrder.Add(connectionId);
        selectedLeft = -1;

        Debug.Log($"?? Conexión registrada: {connectionId}");

        if (playerOrder.Count == correctOrder.Count)
        {
            ValidatePuzzle();
        }
    }

    private void ValidatePuzzle()
    {
        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (playerOrder[i] != correctOrder[i])
            {
                failed = true;
                Debug.Log("? Conexión incorrecta");
                puzzleManager.CheckPuzzleStatus();
                return;
            }
        }

        Debug.Log("? Cable puzzle resuelto");
        puzzleManager.CheckPuzzleStatus();

        // Activar el puzzle de fusibles
        PuzzleManager.OnCablePuzzleComplete();
    }

    public bool IsSolved()
    {
        // Comparar si el orden del jugador coincide con el correcto
        if (playerOrder.Count != correctOrder.Count) return false;

        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (playerOrder[i] != correctOrder[i])
                return false;
        }

        return true;
    }

    public bool HasFailed() => failed;

    public void ClearFailureState()
    {
        failed = false;
    }

    public void ResetPuzzle()
    {
        playerOrder.Clear();
        selectedLeft = -1;
        failed = false;
    }
}