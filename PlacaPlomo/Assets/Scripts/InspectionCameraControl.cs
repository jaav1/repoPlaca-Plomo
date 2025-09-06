using UnityEngine;

public class InspectionCameraControl : MonoBehaviour
{
    [Header("Configuración de la Cámara de Inspección")]
    [Tooltip("La velocidad de rotación de la cámara")]
    public float sensitivity = 2.0f;
    [Tooltip("El límite de rotación vertical (arriba y abajo)")]
    public float verticalClamp = 85.0f; // Para evitar que la cámara gire 360 grados

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Update()
    {
        // Solo rotamos la cámara si el cursor está visible
        if (Cursor.visible)
        {
            // Obtener el input del mouse
            rotationX += Input.GetAxis("Mouse X") * sensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * sensitivity;

            // Limitar la rotación vertical
            rotationY = Mathf.Clamp(rotationY, -verticalClamp, verticalClamp);

            // Aplicar la rotación a la cámara
            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0.0f);
        }
    }
}