using System.Collections;
using UnityEngine;

public class HotwirePuzzleManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CablePuzzle cablePuzzle;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private AudioSource panelSound;

    [Header("Alarma")]
    [SerializeField] private AudioSource alarmSource;
    [SerializeField] private AudioClip alarmClip;

    [Header("Ignición")]
    [SerializeField] private CarIgnition carIgnition;

    [Header("Retry Automático")]
    [SerializeField, Tooltip("Segundos antes de reiniciar el puzzle tras el fallo")]
    private float retryDelay = 2f;

    public void StartPuzzle()
    {
        Debug.Log("?? HotwirePuzzleManager: Puzzle iniciado.");
        cablePuzzle.ClearFailureState();
        cablePuzzle.ResetPuzzle();

        puzzlePanel.SetActive(true);
        panelSound?.Play();
    }

    public void CheckPuzzleStatus()
    {
        if (cablePuzzle.IsSolved())
        {
            Debug.Log("? HotwirePuzzleManager: Puzzle resuelto correctamente.");
            puzzlePanel.SetActive(false);
            carIgnition?.EnableHotwireAccess();
        }
        else if (cablePuzzle.HasFailed())
        {
            Debug.Log("? HotwirePuzzleManager: Puzzle fallado.");
            if (alarmClip != null && alarmSource != null)
                alarmSource.PlayOneShot(alarmClip);

            // Inicia el reinicio automático
            StartCoroutine(AutoRetryCoroutine());
        }
    }

    private IEnumerator AutoRetryCoroutine()
    {
        // Espera antes de resetear
        yield return new WaitForSeconds(retryDelay);

        Debug.Log($"? Reiniciando puzzle tras {retryDelay} segundos...");
        cablePuzzle.ClearFailureState();
        cablePuzzle.ResetPuzzle();

        panelSound?.Play();
    }
}