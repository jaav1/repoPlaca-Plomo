using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia { get; private set; }

    [Header("Estado jugador")]
    public List<string> objetosRecogidos = new List<string>();
    public float empatia = 0f;
    public float sospecha = 0f;
    public string escenaActual;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        escenaActual = SceneManager.GetActiveScene().name;
        CargarAhora();
    }

    public void AddItem(string id)
    {
        if (!objetosRecogidos.Contains(id)) objetosRecogidos.Add(id);
    }

    public void RemoveItem(string id)
    {
        objetosRecogidos.Remove(id);
    }

    // Método que ligarás al botón "Guardar ahora"
    public void GuardarAhora()
    {
        if (SistemaGuardado.instancia == null)
        {
            Debug.LogWarning("SistemaGuardado no está inicializado.");
            return;
        }

        DatosJugador datos = new DatosJugador
        {
            objetosRecogidos = new List<string>(objetosRecogidos),
            empatia = empatia,
            sospecha = sospecha,
            escenaActual = escenaActual
        };

        SistemaGuardado.instancia.GuardarDatos(datos);
    }

    // Carga estado desde archivo y aplica. Si la escena guardada es distinta, la carga.
    public void CargarAhora()
    {
        if (SistemaGuardado.instancia == null)
        {
            // Si SistemaGuardado no existía aún, reintenta ligeramente después
            Invoke(nameof(CargarAhora), 0.2f);
            return;
        }

        DatosJugador datos = SistemaGuardado.instancia.CargarDatos();
        if (datos == null) return;

        objetosRecogidos = new List<string>(datos.objetosRecogidos);
        empatia = datos.empatia;
        sospecha = datos.sospecha;

        if (!string.IsNullOrEmpty(datos.escenaActual) && datos.escenaActual != SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene(datos.escenaActual);
        }

        Debug.Log("Estado restaurado desde guardado.");
    }
}
