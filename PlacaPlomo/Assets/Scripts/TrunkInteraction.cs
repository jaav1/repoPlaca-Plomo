using UnityEngine;
using TMPro; // Asegúrate de tener esta librería
using UnityEngine.UI; // Y esta tambien

public class TrunkInteraction : MonoBehaviour
{
    // Referencias a los scripts que gestionan el carro y el inventario
    public TrunkLock trunkLock;
    public PlayerMovement playerMovement;

    // Referencias a tu UI
    public GameObject interactionPanel;
    public TMP_Text interactionText;

    // El ID de la llave que necesitas
    public string requiredKeyId = "Llave de coche";

    private bool canInteract = false;

    // --- MÉTODOS DE LÓGICA ---
    void Update()
    {
        // Si el jugador está en el área de interacción y presiona E
        if (canInteract && Input.GetKeyDown(KeyCode.R))
        {
            // Oculta el panel de interacción
            interactionPanel.SetActive(false);

            // Llama a la lógica para desbloquear el maletero
            if (trunkLock != null)
            {
                trunkLock.UnlockTrunk();
            }
        }
    }

    // --- MÉTODOS DE DETECCIÓN DE JUGADOR (TRIGGER) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            ShowInteractionUI("Presiona R para abrir el maletero\nPresiona E para conducir\nPresiona F para inspeccionar");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            HideInteractionUI();
        }
    }

    // --- MÉTODOS DE GESTIÓN DE UI ---
    public void ShowInteractionUI(string message)
    {
        if (interactionText != null)
        {
            interactionText.text = message;
        }
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);
        }
    }

    public void HideInteractionUI()
    {
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
    }
}