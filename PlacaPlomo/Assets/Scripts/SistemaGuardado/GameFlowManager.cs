using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    // Singleton estático para acceso global
    public static GameFlowManager Instance { get; private set; }

    // Variable para almacenar los datos cargados temporalmente
    private DatosJugador _loadedData;

    public bool HasLoadedData()
    {
        return _loadedData != null;
    }

    private void Awake()
    {
        // Implementación del Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[GameFlowManager] Inicializado y persistente.");
    }

    // Método llamado por MainMenu para guardar los datos cargados.
    public void SetLoadedData(DatosJugador data)
    {
        _loadedData = data;
        if (data != null)
        {
            Debug.Log("[GameFlowManager] Datos cargados listos para ser aplicados.");
        }
    }

    // Método llamado por MissionManager para obtener los datos.

    public DatosJugador GetLoadedData()
    {
        if (_loadedData != null)
        {
            Debug.Log("[GameFlowManager] Entregando datos y limpiando variable temporal.");
            DatosJugador dataToReturn = _loadedData;
            _loadedData = null; // <-- ¡ESTA ES LA CLAVE! 
            return dataToReturn;
        }
        Debug.Log("[GameFlowManager] GetLoadedData llamado, pero no hay datos temporales disponibles.");
        return null;
    }
}