using UnityEngine;
using TMPro; // Aseg�rate de tener esta librer�a
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

    // --- M�TODOS DE L�GICA ---
    void Update()
    {
        // Si el jugador est� en el �rea de interacci�n y presiona E
        if (canInteract && Input.GetKeyDown(KeyCode.R))
        {
            // Oculta el panel de interacci�n
            interactionPanel.SetActive(false);

            // Llama a la l�gica para desbloquear el maletero
            if (trunkLock != null)
            {
                trunkLock.UnlockTrunk();
            }
        }
    }

    // --- M�TODOS DE DETECCI�N DE JUGADOR (TRIGGER) ---
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

    // --- M�TODOS DE GESTI�N DE UI ---
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