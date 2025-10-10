using UnityEngine;
using UnityEngine.SceneManagement; // Necesario si quieres reiniciar/salir
using TMPro; // Si usas TextMeshPro

public class EndGamePanelController : MonoBehaviour
{
    [Tooltip("El Panel UI que contiene la imagen, el logo, y el mensaje de 'FIN'.")]
    public GameObject endScreenPanel;

    [Header("Opciones")]
    public float delayBeforeDisplay = 1.0f; // Peque�o retraso antes de que aparezca

    void Start()
    {
        // Aseg�rate de que el panel est� oculto al iniciar el juego
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }
    }

    public void DisplayEndScreen()
    {
        // Opcional: Pausar el juego
        Time.timeScale = 0f;

        // Usamos una Coroutine para el retraso
        StartCoroutine(DisplayAfterDelay(delayBeforeDisplay));
    }

    System.Collections.IEnumerator DisplayAfterDelay(float delay)
    {
        // Espera el tiempo especificado
        yield return new WaitForSecondsRealtime(delay); // Usamos Realtime ya que Time.timeScale=0

        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
        }

        // Opcional: Mostrar el cursor del rat�n
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // M�todo que puedes adjuntar a un bot�n en el panel de cierre
    public void QuitGame()
    {
        Debug.Log("Cerrando el Prototipo.");
        Application.Quit(); // Solo funciona en versiones compiladas
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Detiene la ejecuci�n en el editor
#endif
    }
}