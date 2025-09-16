using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Tus paneles de UI
    public GameObject optionsMenu;
    public GameObject mainMenu;

    // Panel para el men� de ayuda
    public GameObject helpMenu;

    // Funci�n para abrir el men� de opciones
    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    // Funci�n para cerrar el men� de opciones y volver al men� principal
    public void OpenMainMenuPanel()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);

        // Tambi�n aseguramos que el panel de ayuda est� cerrado
        // en caso de que lo necesites al volver al men� principal
        helpMenu.SetActive(false);
    }

    // Nueva funci�n para abrir el panel de ayuda
    public void OpenHelpPanel()
    {
        // Ocultamos el men� principal y mostramos el de ayuda
        mainMenu.SetActive(false);
        helpMenu.SetActive(true);
    }

    // Nueva funci�n para cerrar el panel de ayuda
    public void CloseHelpPanel()
    {
        // Ocultamos el men� de ayuda y mostramos el men� principal
        helpMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    // Tus otras funciones de juego
    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("PrincipalScene");
    }
}
