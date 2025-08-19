using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject cablePuzzleUI;
    [SerializeField] private GameObject fusePuzzleUI;

    public void OnCablePuzzleComplete()
    {
        Debug.Log("?? Transición: Cable ? Fusibles");

        cablePuzzleUI.SetActive(false);
        fusePuzzleUI.SetActive(true);

        // Aquí puedes agregar efectos visuales, sonidos, etc.
    }
}