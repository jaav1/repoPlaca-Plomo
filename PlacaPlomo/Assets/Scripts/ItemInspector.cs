using UnityEngine;

public class ItemInspector : MonoBehaviour
{
    // Variables para la rotación del objeto
    [SerializeField] private float rotationSpeed = 10f;
    private Quaternion initialRotation;

    void Start()
    {
        // Guardar la rotación inicial del objeto al ser creado.
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Solo rotar el objeto cuando el jugador presiona y mantiene presionado el botón izquierdo del mouse.
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            // Rotar el objeto alrededor del eje Y (horizontal) y el eje X (vertical).
            transform.Rotate(Vector3.up, -mouseX, Space.World);
            transform.Rotate(Vector3.right, mouseY, Space.World);
        }
    }

    // Método para resetear la rotación a su estado inicial.
    public void ResetRotation()
    {
        transform.rotation = initialRotation;
    }
}