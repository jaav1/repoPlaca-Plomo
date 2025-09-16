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
        }
        else
        {
            // Muestra el mensaje en la pantalla
            messageText.text = "Necesitas la llave del coche para abrir el maletero.";
            messagePanel.SetActive(true);
            Invoke("HideMessage", 3f);
        }
    }

    // Método para ocultar el mensaje
    private void HideMessage()
    {
        messagePanel.SetActive(false);
    }
}