using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    // Sensibilidad del mouse para la rotaci�n de la c�mara.
    public float mouseSensitivity = 100f;
    // El Transform del personaje 2D al que la c�mara seguir�.
    public Transform playerBody;

    float xRotation = 0f;

    void Start()
    {
        // Bloquea el cursor en el centro de la pantalla y lo hace invisible.
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Obtiene la entrada del mouse en los ejes X e Y.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rota el cuerpo del jugador horizontalmente (hacia la izquierda y derecha).
        playerBody.Rotate(Vector3.up * mouseX);

        // Actualiza la rotaci�n vertical de la c�mara.
        xRotation -= mouseY;
        // Limita la rotaci�n vertical para evitar que la c�mara gire 360 grados.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplica la rotaci�n a la c�mara.
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
    

