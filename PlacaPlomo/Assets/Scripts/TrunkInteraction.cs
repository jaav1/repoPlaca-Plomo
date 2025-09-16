using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TrunkInteraction : MonoBehaviour
{
    public TrunkLock trunkLock;
    public PlayerMovement playerMovement;
    public RadialInventoryManager inventoryManager;

    [Header("UI References")]
    public GameObject interactionPanel;
    public TMP_Text interactionText;

    private bool canInteract = false;
    private bool isTrunkInspecting = false;

    void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.R))
        {
            if (!isTrunkInspecting)
            {
                if (inventoryManager != null && inventoryManager.HasItem(trunkLock.keyID))
                {
                    HideInteractionUI();
                    trunkLock.UnlockTrunk();
                    isTrunkInspecting = true;
                    canInteract = false;

                    // --- ¡CORRECCIÓN! ---
                    if (playerMovement != null)
                    {
                        playerMovement.enabled = false;
                    }
                }
                else
                {
                    ShowInteractionUI("Necesitas la llave del coche para abrir el maletero.");
                }
            }
        }
        else if (isTrunkInspecting && Input.GetKeyDown(KeyCode.R))
        {
            trunkLock.ExitTrunkInspection();
            isTrunkInspecting = false;
            canInteract = true;
            ShowInteractionUI("Presiona R para abrir el maletero");

            // --- ¡CORRECCIÓN! ---
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTrunkInspecting)
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