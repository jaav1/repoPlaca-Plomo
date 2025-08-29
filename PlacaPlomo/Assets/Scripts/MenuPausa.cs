using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public RadialInventoryManager inventoryManager;
    public GameObject ObjetoMenuPausa;
    public bool Pausa = false;
    public GameObject MenuSalir;

    // Necesitas una referencia al script de movimiento de la c�mara
    public PlayerMovement ScriptDelJugador;

    // Referencia para guardar los sonidos que se est�n reproduciendo
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

    // Nuevo m�todo para pausar el juego
    void PausarJuego()
    {
        Pausa = true;
        ObjetoMenuPausa.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Desactivar el script de la c�mara para que no se mueva
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

    // Nuevo m�todo para reanudar el juego
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

        // Reactivar el script de la c�mara
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
        // Llama al m�todo de guardado en GameManager, que se encarga de todo.

        if (GameManager.instancia != null)
        {
            GameManager.instancia.GuardarAhora();
        }
        else
        {
            Debug.LogWarning("El GameManager no se encontr�. No se pudo guardar el juego.");
        }

        Time.timeScale = 1;
        SceneManager.LoadScene(NombreMenu);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
