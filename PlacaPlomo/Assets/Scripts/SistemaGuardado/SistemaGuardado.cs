using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class DatosJugador
{
    public List<string> objetosRecogidos = new List<string>();
    public float empatia;
    public float sospecha;
    public string escenaActual;
}

public class SistemaGuardado : MonoBehaviour
{
    public static SistemaGuardado instancia { get; private set; }
    private string rutaArchivo;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            rutaArchivo = Path.Combine(Application.persistentDataPath, "datosJugador.json");

            // Mostrar la ruta al iniciar
            Debug.Log($"[Ruta de guardado] {rutaArchivo}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GuardarDatos(DatosJugador datos)
    {
        try
        {
            string json = JsonUtility.ToJson(datos, true);
            File.WriteAllText(rutaArchivo, json);
            Debug.Log($"[Guardar] OK -> {rutaArchivo}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Guardar] ERROR: " + ex.Message);
        }
    }

    public DatosJugador CargarDatos()
    {
        try
        {
            if (!File.Exists(rutaArchivo))
            {
                Debug.LogWarning("[Cargar] No existe archivo, creando guardado inicial...");

                // Crear datos iniciales
                DatosJugador datosIniciales = new DatosJugador
                {
                    objetosRecogidos = new List<string>(),
                    empatia = 0,
                    sospecha = 0,
                    escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                };

                GuardarDatos(datosIniciales);
                return datosIniciales;
            }

            string json = File.ReadAllText(rutaArchivo);
            DatosJugador datos = JsonUtility.FromJson<DatosJugador>(json);
            Debug.Log("[Cargar] OK");
            return datos;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Cargar] ERROR: " + ex.Message);
            return null;
        }
    }
}
