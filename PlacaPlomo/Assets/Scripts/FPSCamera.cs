using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    // Sensibilidad del mouse para la rotación de la cámara.
    public float mouseSensitivity = 100f;
    // El Transform del personaje 2D al que la cámara seguirá.
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

        // Actualiza la rotación vertical de la cámara.
        xRotation -= mouseY;
        // Limita la rotación vertical para evitar que la cámara gire 360 grados.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplica la rotación a la cámara.
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
    

