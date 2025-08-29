using UnityEngine;
using System.IO;

public class SistemaGuardado : MonoBehaviour
{
    public static SistemaGuardado instancia;

    private string rutaArchivo;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            // Asegúrate de que la carpeta de guardado existe
            string rutaCarpeta = Path.Combine(Application.persistentDataPath, "DatosPartida");
            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }
            rutaArchivo = Path.Combine(rutaCarpeta, "datosJugador.json");
            Debug.Log($"[Guardar] La ruta del archivo es: {rutaArchivo}");
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
            Debug.Log("[Guardar] OK -> " + rutaArchivo);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Guardar] Error al guardar el archivo: {e.Message}");
        }
    }

    public DatosJugador CargarDatos()
    {
        if (File.Exists(rutaArchivo))
        {
            try
            {
                string json = File.ReadAllText(rutaArchivo);
                DatosJugador datos = JsonUtility.FromJson<DatosJugador>(json);
                Debug.Log("[Cargar] OK -> " + rutaArchivo);
                return datos;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Cargar] Error al cargar el archivo: {e.Message}");
                return null;
            }
        }
        else
        {
            Debug.LogWarning("[Cargar] Archivo de guardado no encontrado en: " + rutaArchivo);
            return null;
        }
    }
}