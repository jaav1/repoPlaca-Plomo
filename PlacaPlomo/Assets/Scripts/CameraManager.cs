using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Arrastra tus cámaras a estos campos en el Inspector
    [Header("Referencias de Cámaras")]
    public Camera playerCamera;
    public Camera carCamera;
    public Camera inspectionCamera;
    public Camera bgInspectionCamera; // ¡NUEVA CÁMARA!

    public void SwitchToPlayerCamera()
    {
        playerCamera.gameObject.SetActive(true);
        carCamera.gameObject.SetActive(false);
        inspectionCamera.gameObject.SetActive(false);
        bgInspectionCamera.gameObject.SetActive(false);
    }

    public void SwitchToCarCamera()
    {
        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(true);
        inspectionCamera.gameObject.SetActive(false);
        bgInspectionCamera.gameObject.SetActive(false);
    }

    public void SwitchToInspectionCamera()
    {
        // La cámara de inspección (la que renderiza a la textura) y la de fondo están activas
        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(false);
        inspectionCamera.gameObject.SetActive(true);
        bgInspectionCamera.gameObject.SetActive(true);
    }
}