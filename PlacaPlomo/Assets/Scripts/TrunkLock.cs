using UnityEngine;
using TMPro;

public class TrunkLock : MonoBehaviour
{
    // El objeto del coche con el maletero cerrado
    public GameObject closedTrunkObject;

    // El objeto del coche con el maletero abierto y la pista adentro
    public GameObject openTrunkObject;

    // La referencia al ID de la llave que necesitas
    // (Aún la mantenemos aquí por si otro script la necesita, pero no la usaremos en este)
    public string keyID = "Llave de coche";

    // Mantenemos la referencia a los objetos, pero el control lo tendrá VehicleInteraction
    public GameObject messagePanel;
    public TMP_Text messageText;

    void Start()
    {
        if (openTrunkObject != null)
        {
            openTrunkObject.SetActive(false);
        }
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    // El método UnlockTrunk() ahora es simple y solo cambia los modelos 3D
    public void UnlockTrunk()
    {
        Debug.Log("Maletero desbloqueado.");

        if (closedTrunkObject != null)
        {
            closedTrunkObject.SetActive(false);
        }

        if (openTrunkObject != null)
        {
            openTrunkObject.SetActive(true);
        }
    }

    // Este método está perfecto. Lo usamos para cerrar el maletero visualmente.
    public void CloseTrunk()
    {
        if (openTrunkObject != null)
        {
            openTrunkObject.SetActive(false);
        }
        if (closedTrunkObject != null)
        {
            closedTrunkObject.SetActive(true);
        }
        Debug.Log("Maletero cerrado.");
    }
}