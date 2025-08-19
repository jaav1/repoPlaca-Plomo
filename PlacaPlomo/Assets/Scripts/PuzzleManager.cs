using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject cablePuzzleUI;
    [SerializeField] private GameObject fusePuzzleUI;

    public void OnCablePuzzleComplete()
    {
        Debug.Log("?? Transici�n: Cable ? Fusibles");

        cablePuzzleUI.SetActive(false);
        fusePuzzleUI.SetActive(true);

        // Aqu� puedes agregar efectos visuales, sonidos, etc.
    }
}