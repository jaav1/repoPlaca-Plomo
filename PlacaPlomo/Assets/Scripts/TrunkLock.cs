using UnityEngine;
using TMPro;
using System.Collections;

public class TrunkLock : MonoBehaviour
{
    // El objeto del coche con el maletero cerrado
    public GameObject closedTrunkObject;

    // El objeto del coche con el maletero abierto y la pista adentro
    public GameObject openTrunkObject;

    // La referencia a tu sistema de inventario
    public RadialInventoryManager inventoryManager;

    // La referencia al ID de la llave que necesitas
    public string keyID = "Llave de coche";

    public CameraManager cameraManager;
    public GameObject inspectionPanel;

    [Header("Referencias de UI")]
    public GameObject messagePanel;
    public TMP_Text messageText;

    void Start()
    {
        // El maletero abierto debe estar desactivado al iniciar el juego
        if (openTrunkObject != null)
        {
            openTrunkObject.SetActive(false);
        }

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    public void UnlockTrunk()
    {
        // Verifica si el jugador tiene la llave en su inventario
        // Ahora usamos la variable keyID
        if (inventoryManager != null && inventoryManager.HasItem(keyID))
        {
            Debug.Log("Maletero desbloqueado.");

            // Desactiva el modelo de maletero cerrado
            if (closedTrunkObject != null)
            {
                closedTrunkObject.SetActive(false);
            }

            // Activa el modelo de maletero abierto
            if (openTrunkObject != null)
            {
                openTrunkObject.SetActive(true);
            }

            if (cameraManager != null)
            {
                cameraManager.SwitchToTrunkInspectionCamera();
            }
            if (inspectionPanel != null)
            {
                inspectionPanel.SetActive(true);
            }
        }
        else
        {
            // Muestra el mensaje en la pantalla
            messageText.text = "Necesitas la llave del coche para abrir el maletero.";
            messagePanel.SetActive(true);
            Invoke("HideMessage", 3f);
        }
    }

    // M�todo para ocultar el mensaje
    private void HideMessage()
    {
        messagePanel.SetActive(false);
    }

    public void ExitTrunkInspection()
    {
        // Ocultamos la UI de inspecci�n
        if (inspectionPanel != null)
        {
            inspectionPanel.SetActive(false);
        }

        // Devolvemos el control a la c�mara principal
        if (cameraManager != null)
        {
            cameraManager.SwitchToPlayerCamera();
        }
    }

    // M�todo para ocultar el maletero abierto y mostrar el cerrado.
    public void CloseTrunk()
    {
        // Aseg�rate de que el modelo abierto est� desactivado
        if (openTrunkObject != null)
        {
            openTrunkObject.SetActive(false);
        }

        // Aseg�rate de que el modelo cerrado est� activado
        if (closedTrunkObject != null)
        {
            closedTrunkObject.SetActive(true);
        }
        Debug.Log("Maletero cerrado.");
    }
}