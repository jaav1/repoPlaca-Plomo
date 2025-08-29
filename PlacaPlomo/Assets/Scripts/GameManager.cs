using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia { get; private set; }

    [Header("Estado del Jugador")]
    public float empatia = 0f;
    public float sospecha = 0f;
    public string escenaActual;
    public int confianzaMendoza = 0;
    public int rayEmpatia = 0;
    public int rayFrialdad = 0;
    public float tension = 0f;

    private RadialInventoryManager inventario;

    // Nueva lista para los objetos recogidos
    private List<string> objetosRecogidos = new List<string>();

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);

            // Suscribirse al evento de carga de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar errores y fugas de memoria
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // MÉTODO que se ejecuta cada vez que una escena se carga
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Solo ejecuta la lógica de carga si la escena es la del juego principal
        if (scene.name == "PrincipalScene")
        {
            escenaActual = scene.name;
            Debug.Log("Escena del juego cargada. Iniciando carga de datos...");

            // El GameManager encuentra el HUD dinámicamente
            hudPanel = GameObject.FindWithTag("HUD");
            if (hudPanel == null)
            {
                Debug.LogError("No se encontró el objeto con la etiqueta 'HUD'.");
            }

            CargarAhora();
        }
        else
        {
            // Limpiamos la lista de objetos recogidos al salir de la escena de juego.
            if (objetosRecogidos != null)
            {
                objetosRecogidos.Clear();
            }
        }
    }

    public GameObject hudPanel;

    public void ActivarHud(bool activo)
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(activo);
        }
        else
        {
            Debug.LogError("El objeto del HUD no está asignado. No se puede activar o desactivar.");
        }
    }

    public void AjustarEstado(string tipo, int valor)
    {
        switch (tipo)
        {
            case "Empatia": rayEmpatia += valor; break;
            case "Frialdad": rayFrialdad += valor; break;
            case "Tension": tension = Mathf.Clamp(tension + valor, 0, 100); break;
            case "ConfianzaMendoza": confianzaMendoza += valor; break;
        }
    }

    // MÉTODO para que ItemPickup le avise al GameManager
    public void MarcarObjetoComoRecogido(string nombreDelObjeto)
    {
        if (!objetosRecogidos.Contains(nombreDelObjeto))
        {
            objetosRecogidos.Add(nombreDelObjeto);
        }
    }

    public void GuardarAhora()
    {
        inventario = FindFirstObjectByType<RadialInventoryManager>();

        if (inventario == null)
        {
            Debug.LogWarning("No se encontró el inventario en la escena. No se pudo guardar.");
            return;
        }

        DatosJugador datosJugador = new DatosJugador
        {
            empatia = this.empatia,
            sospecha = this.sospecha,
            escenaActual = this.escenaActual,
            confianzaMendoza = this.confianzaMendoza,
            rayEmpatia = this.rayEmpatia,
            rayFrialdad = this.rayFrialdad,
            tension = this.tension,
            objetosRecogidos = this.objetosRecogidos,
            // Ahora se guarda la lista de páginas
            inventario = inventario.GetInventoryDataForSave()
        };

        SistemaGuardado.instancia.GuardarDatos(datosJugador);
        Debug.Log("Datos guardados correctamente.");
    }

    public void CargarAhora()
    {
        DatosJugador datosGuardados = SistemaGuardado.instancia.CargarDatos();

        if (datosGuardados != null)
        {
            this.empatia = datosGuardados.empatia;
            this.sospecha = datosGuardados.sospecha;
            this.escenaActual = datosGuardados.escenaActual;
            this.confianzaMendoza = datosGuardados.confianzaMendoza;
            this.rayEmpatia = datosGuardados.rayEmpatia;
            this.rayFrialdad = datosGuardados.rayFrialdad;
            this.tension = datosGuardados.tension;
            this.objetosRecogidos = datosGuardados.objetosRecogidos;

            inventario = FindFirstObjectByType<RadialInventoryManager>();
            if (inventario != null)
            {
                // Ahora se carga la nueva lista de datos del inventario.
                inventario.SetInventoryDataFromLoad(datosGuardados.inventario);
            }

            EliminarObjetosRecogidos();
        }
    }

    // MÉTODO para eliminar los objetos de la escena
    private void EliminarObjetosRecogidos()
    {
        Debug.Log("Buscando objetos en la escena para eliminar...");
        // Ahora usa FindObjectsByType, que es más moderno y eficiente.
        ItemPickup[] itemsEnEscena = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);

        foreach (ItemPickup item in itemsEnEscena)
        {
            if (objetosRecogidos.Contains(item.itemId))
            {
                Debug.Log($"Eliminando objeto: {item.gameObject.name} (ID: {item.itemId})");
                Destroy(item.gameObject);
            }
        }
        Debug.Log("Proceso de eliminación de objetos completado.");
    }
}