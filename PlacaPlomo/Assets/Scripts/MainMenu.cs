using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string GAME_SCENE_NAME = "PrincipalScene";
    
    // Tus paneles de UI
    public GameObject optionsMenu;
    public GameObject mainMenu;

    // Panel para el menú de ayuda
    public GameObject helpMenu;

    public void StartOrContinueGame()
    {
        DatosJugador loadedData = null;

        if (SistemaGuardado.instancia == null)
        {
            Debug.LogError("No se puede intentar cargar: SistemaGuardado no está disponible.");
            // En un escenario de error, continuamos con partida nueva para que no crashee.
            ProceedToGameScene(null);
            return;
        }

        // 1. Intentar cargar los datos
        loadedData = SistemaGuardado.instancia.CargarDatos();

        if (loadedData != null)
        {
            // ÉXITO: Archivo de guardado encontrado.
            Debug.Log("Datos de partida anterior cargados correctamente. Continuando juego...");
        }
        else
        {
            // FALLO: No hay archivo o la carga falló.
            // La función CargarDatos ya imprime una advertencia si no lo encuentra.
            Debug.Log("No se encontró partida guardada o la carga falló. Iniciando juego nuevo.");
        }

        // 2. Continúa hacia la escena del juego, pasando los datos (cargados o null)
        ProceedToGameScene(loadedData);
    }

    private void ProceedToGameScene(DatosJugador dataToPass)
    {
        // 1. Almacenar temporalmente los datos para que el MissionManager los use.
        // Asumiendo que has creado el GameFlowManager Singleton (ver explicación anterior).
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.SetLoadedData(dataToPass);
        }
        else
        {
            Debug.LogError("GameFlowManager no encontrado. El MissionManager no podrá restaurar el estado.");
        }

        // 2. Cargar la escena
        SceneManager.LoadScene(GAME_SCENE_NAME);
    }

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
}
