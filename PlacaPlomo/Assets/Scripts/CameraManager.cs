using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Arrastra tus cámaras a estos campos en el Inspector
    [Header("Referencias de Cámaras")]
    public Camera playerCamera;
    public Camera carCamera;
    public Camera inspectionCamera;
    public Camera bgInspectionCamera;
    public Camera trunkInspectionCamera; 
    public Camera photoCamera;

    // Métodos para cambiar de cámara
    public void SwitchToPlayerCamera()
    {
        playerCamera.gameObject.SetActive(true);
        carCamera.gameObject.SetActive(false);
        inspectionCamera.gameObject.SetActive(false);
        bgInspectionCamera.gameObject.SetActive(false);
        trunkInspectionCamera.gameObject.SetActive(false);
        photoCamera.gameObject.SetActive(false);

        // Activa el AudioListener en la cámara del jugador
        playerCamera.GetComponent<AudioListener>().enabled = true;
        carCamera.GetComponent<AudioListener>().enabled = false;
        inspectionCamera.GetComponent<AudioListener>().enabled = false;
        bgInspectionCamera.GetComponent<AudioListener>().enabled = false;
        trunkInspectionCamera.GetComponent<AudioListener>().enabled = false;
        photoCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void SwitchToCarCamera()
    {
        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(true);
        inspectionCamera.gameObject.SetActive(false);
        bgInspectionCamera.gameObject.SetActive(false);
        trunkInspectionCamera.gameObject.SetActive(false);
        photoCamera.gameObject.SetActive(false);

        // Activa el AudioListener en la cámara del coche
        playerCamera.GetComponent<AudioListener>().enabled = false;
        carCamera.GetComponent<AudioListener>().enabled = true;
        inspectionCamera.GetComponent<AudioListener>().enabled = false;
        bgInspectionCamera.GetComponent<AudioListener>().enabled = false;
        trunkInspectionCamera.GetComponent<AudioListener>().enabled = false;
        photoCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void SwitchToInspectionCamera()
    {
        // La cámara de inspección (la que renderiza a la textura) y la de fondo están activas
        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(false);
        inspectionCamera.gameObject.SetActive(true);
        bgInspectionCamera.gameObject.SetActive(true);
        trunkInspectionCamera.gameObject.SetActive(false);
        photoCamera.gameObject.SetActive(false);

        // Activa el AudioListener en la cámara de inspección
        playerCamera.GetComponent<AudioListener>().enabled = false;
        carCamera.GetComponent<AudioListener>().enabled = false;
        inspectionCamera.GetComponent<AudioListener>().enabled = true;
        bgInspectionCamera.GetComponent<AudioListener>().enabled = false;
        trunkInspectionCamera.GetComponent<AudioListener>().enabled = false;
        photoCamera.GetComponent<AudioListener>().enabled = true;
    }

    // --- ¡NUEVO MÉTODO! ---
    public void SwitchToTrunkInspectionCamera()
    {
        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(false);
        inspectionCamera.gameObject.SetActive(false);
        trunkInspectionCamera.gameObject.SetActive(true);
        photoCamera.gameObject.SetActive(false);
        bgInspectionCamera.gameObject.SetActive(true);

        playerCamera.GetComponent<AudioListener>().enabled = false;
        carCamera.GetComponent<AudioListener>().enabled = false;
        inspectionCamera.GetComponent<AudioListener>().enabled = false;
        bgInspectionCamera.GetComponent<AudioListener>().enabled = false;
        trunkInspectionCamera.GetComponent<AudioListener>().enabled = true;
        photoCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void SwitchToPhotoCamera()
    {
        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(false);
        inspectionCamera.gameObject.SetActive(false);
        trunkInspectionCamera.gameObject.SetActive(false);
        photoCamera.gameObject.SetActive(true);
        bgInspectionCamera.gameObject.SetActive(true);

        playerCamera.GetComponent<AudioListener>().enabled = true;
        carCamera.GetComponent<AudioListener>().enabled = false;
        inspectionCamera.GetComponent<AudioListener>().enabled = false;
        bgInspectionCamera.GetComponent<AudioListener>().enabled = false;
        trunkInspectionCamera.GetComponent<AudioListener>().enabled = false;
        photoCamera.GetComponent<AudioListener>().enabled = true;
    }
}