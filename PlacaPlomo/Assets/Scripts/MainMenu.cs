using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Tus paneles de UI
    public GameObject optionsMenu;
    public GameObject mainMenu;

    // Panel para el menú de ayuda
    public GameObject helpMenu;

    // Función para abrir el menú de opciones
    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    // Función para cerrar el menú de opciones y volver al menú principal
    public void OpenMainMenuPanel()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);

        // También aseguramos que el panel de ayuda esté cerrado
        // en caso de que lo necesites al volver al menú principal
        helpMenu.SetActive(false);
    }

    // Nueva función para abrir el panel de ayuda
    public void OpenHelpPanel()
    {
        // Ocultamos el menú principal y mostramos el de ayuda
        mainMenu.SetActive(false);
        helpMenu.SetActive(true);
    }

    // Nueva función para cerrar el panel de ayuda
    public void CloseHelpPanel()
    {
        // Ocultamos el menú de ayuda y mostramos el menú principal
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
