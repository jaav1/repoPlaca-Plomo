using UnityEngine;

public class InspectionCameraControl : MonoBehaviour
{
    [Header("Configuraci�n de la C�mara de Inspecci�n")]
    [Tooltip("La velocidad de rotaci�n de la c�mara")]
    public float sensitivity = 2.0f;
    [Tooltip("El l�mite de rotaci�n vertical (arriba y abajo)")]
    public float verticalClamp = 85.0f; // Para evitar que la c�mara gire 360 grados

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Update()
    {
        // Solo rotamos la c�mara si el cursor est� visible
        if (Cursor.visible)
        {
            // Obtener el input del mouse
            rotationX += Input.GetAxis("Mouse X") * sensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * sensitivity;

            // Limitar la rotaci�n vertical
            rotationY = Mathf.Clamp(rotationY, -verticalClamp, verticalClamp);

            // Aplicar la rotaci�n a la c�mara
            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0.0f);
        }
    }
}