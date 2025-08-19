using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FusePuzzle : MonoBehaviour
{
    [Header("Sockets")]
    public Button[] socketButtons;

    [Header("Fusibles")]
    public Button[] fuseButtons;

    [Header("Correct Combination")]
    public string[] correctFuses = { "A", "B", "C" };

    private string[] currentFuses;

    [Header("Puzzle Feedback")]
    public AudioSource successAudio;
    public GameObject puzzlePanel;
    public CarController carController;

    private string selectedFuse = "";

    void Start()
    {
        currentFuses = new string[socketButtons.Length];

        for (int i = 0; i < fuseButtons.Length; i++)
        {
            int index = i;
            fuseButtons[i].onClick.AddListener(() => SelectFuse(index));
        }

        for (int i = 0; i < socketButtons.Length; i++)
        {
            int index = i;
            socketButtons[i].onClick.AddListener(() => PlaceFuse(index));
        }
    }

    void SelectFuse(int fuseIndex)
    {
        selectedFuse = fuseButtons[fuseIndex].name;
        Debug.Log("Selected: " + selectedFuse);
    }

    void PlaceFuse(int socketIndex)
    {
        if (string.IsNullOrEmpty(selectedFuse)) return;
        if (socketIndex < 0 || socketIndex >= socketButtons.Length) return;

        currentFuses[socketIndex] = selectedFuse.Substring(4);

        var textComponent = socketButtons[socketIndex].GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = currentFuses[socketIndex];
        }
        else
        {
            Debug.LogWarning("?? No se encontró TMP_Text en el botón del socket.");
        }

        selectedFuse = "";

        CheckCombination();
    }

    void CheckCombination()
    {
        for (int i = 0; i < correctFuses.Length; i++)
        {
            if (currentFuses[i] != correctFuses[i]) return;
        }

        Debug.Log("? Puzzle Solved!");

        if (successAudio != null) successAudio.Play();
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (carController != null) carController.EnableDriving();
    }
}