using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 2f; // Velocidad de movimiento
    public float jumpForce = 5f; // Fuerza del salto
    public float mouseSensitivity = 10f; // Sensibilidad del mouse
    public Transform playerCamera; // C�mara del jugador
    private float verticalRotation = 0f; // Rotaci�n vertical
    private Rigidbody rb; // Referencia al Rigidbody
    private bool isGrounded; // Verificar si est� en el suelo

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Obtener el Rigidbody del jugador
        Cursor.lockState = CursorLockMode.Locked; // Bloquear el cursor
    }

    void Update()
    {
        // Movimiento horizontal
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        // Rotaci�n con el mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) // Salta si est� en el suelo
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Detectar si est� tocando el suelo
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Detectar si dej� de tocar el suelo
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}