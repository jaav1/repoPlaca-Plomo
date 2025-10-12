using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public RadialInventoryManager inventoryManager;
    public GameObject ObjetoMenuPausa;
    public bool Pausa = false;
    public GameObject MenuSalir;

    // Necesitas una referencia al script de movimiento de la cámara
    public PlayerMovement ScriptDelJugador;

    // Referencia para guardar los sonidos que se están reproduciendo
    private AudioSource[] sonidosEnJuego;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Pausa)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    // Nuevo método para pausar el juego
    void PausarJuego()
    {
        Pausa = true;
        ObjetoMenuPausa.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Desactivar el script de la cámara para que no se mueva
        if (ScriptDelJugador != null)
        {
            ScriptDelJugador.enabled = false;
        }

        // Obtener todos los sonidos y pausarlos
        sonidosEnJuego = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource sonido in sonidosEnJuego)
        {
            sonido.Pause();
        }
    }

    // Nuevo método para reanudar el juego
    public void ReanudarJuego()
    {
        Pausa = false;
        ObjetoMenuPausa.SetActive(false);
        if (MenuSalir != null)
        {
            MenuSalir.SetActive(false);
        }

        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Reactivar el script de la cámara
        if (ScriptDelJugador != null)
        {
            ScriptDelJugador.enabled = true;
        }

        // Reanudar solo los sonidos que estaban sonando
        if (sonidosEnJuego != null)
        {
            foreach (AudioSource sonido in sonidosEnJuego)
            {
                sonido.UnPause();
            }
        }
    }

    public void IrAlMenu(string NombreMenu)
    {
        // Guarda el juego antes de cambiar de escena.
        // Llama al método de guardado en GameManager, que se encarga de todo.

        /// 1. Crear el objeto de datos unificado
        DatosJugador datosAGuardar = new DatosJugador();

        // 2. Recopilar datos de la misión
        if (MissionManager.I != null)
        {
            datosAGuardar.ProgresoMision = MissionManager.I.GetProgressData();
            Debug.Log("[GUARDAR] Progreso de misión recopilado.");
        }

        // 3. Recopilar otros datos (Ej: Posición) 
        /*Transform playerTransform = MissionManager.I?.GetPlayerTransform(); // Reusamos el helper
        if (playerTransform != null)
        {
            datosAGuardar.PosicionX = playerTransform.position.x;
            datosAGuardar.PosicionY = playerTransform.position.y;
            datosAGuardar.PosicionZ = playerTransform.position.z;
        }*/

        // 4. Llamar al SistemaGuardado central para escribir en disco
        if (SistemaGuardado.instancia != null)
        {
            SistemaGuardado.instancia.GuardarDatos(datosAGuardar);
        }
        else
        {
            Debug.LogWarning("SistemaGuardado no se encontró. No se pudo guardar el juego.");
        }

        // 5. Cargar la escena
        SceneManager.LoadScene(NombreMenu);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
