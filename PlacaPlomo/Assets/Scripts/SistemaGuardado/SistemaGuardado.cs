using System.Collections.Generic; // Para usar List<T>
using System.IO; // Para leer y escribir archivos
using UnityEngine; // Para usar las clases de Unity (MonoBehaviour, Debug, etc.)

// Clase serializable que almacena los datos del jugador
[System.Serializable]
public class DatosJugador
{
    public List<string> objetosRecogidos = new List<string>(); // Lista con los nombres de los objetos recogidos
    public float empatia;     // Nivel de empat�a del jugador
    public float sospecha;    // Nivel de sospecha del jugador
    public string escenaActual; // Nombre de la escena en la que est� el jugador
}

// Sistema de guardado principal, implementado como Singleton
public class SistemaGuardado : MonoBehaviour
{
    // Instancia est�tica �nica (Singleton)
    public static SistemaGuardado instancia { get; private set; }

    // Ruta del archivo donde se guardar�n los datos en formato JSON
    private string rutaArchivo;

    private void Awake()
    {
        // Si no existe instancia, esta se convierte en la �nica
        if (instancia == null)
        {
            instancia = this;

            // Evita que este objeto se destruya al cambiar de escena
            DontDestroyOnLoad(gameObject);

            // Define la ruta del archivo de guardado en la carpeta persistente
            rutaArchivo = Path.Combine(Application.persistentDataPath, "datosJugador.json");

            // Muestra la ruta en la consola (�til para depuraci�n)
            Debug.Log($"[Ruta de guardado] {rutaArchivo}");
        }
        else
        {
            // Si ya existe otra instancia, se destruye para mantener el Singleton
            Destroy(gameObject);
        }
    }

    // M�todo para guardar los datos del jugador en un archivo JSON
    public void GuardarDatos(DatosJugador datos)
    {
        try
        {
            // Convierte el objeto DatosJugador a JSON
            string json = JsonUtility.ToJson(datos, true);

            // Escribe el JSON en el archivo de la ruta especificada
            File.WriteAllText(rutaArchivo, json);

            Debug.Log($"[Guardar] OK -> {rutaArchivo}"); // Mensaje de �xito
        }
        catch (System.Exception ex)
        {
            // Captura y muestra cualquier error ocurrido en el guardado
            Debug.LogError("[Guardar] ERROR: " + ex.Message);
        }
    }

    // M�todo para cargar los datos del jugador desde el archivo JSON
    public DatosJugador CargarDatos()
    {
        try
        {
            // Si no existe el archivo de guardado...
            if (!File.Exists(rutaArchivo))
            {
                Debug.LogWarning("[Cargar] No existe archivo, creando guardado inicial...");

                // Crea un objeto con valores iniciales por defecto
                DatosJugador datosIniciales = new DatosJugador
                {
                    objetosRecogidos = new List<string>(), // Lista vac�a
                    empatia = 0,                          // Empat�a inicial
                    sospecha = 0,                         // Sospecha inicial
                    escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name // Escena actual
                };

                // Guarda los datos iniciales para crear el archivo
                GuardarDatos(datosIniciales);

                // Devuelve los datos iniciales al juego
                return datosIniciales;
            }

            // Si el archivo existe, lo lee y convierte de JSON a objeto DatosJugador
            string json = File.ReadAllText(rutaArchivo);
            DatosJugador datos = JsonUtility.FromJson<DatosJugador>(json);

            Debug.Log("[Cargar] OK"); // Mensaje de �xito
            return datos;
        }
        catch (System.Exception ex)
        {
            // Captura y muestra cualquier error ocurrido en la carga
            Debug.LogError("[Cargar] ERROR: " + ex.Message);
            return null; // Si hay error, devuelve null
        }
    }
}
