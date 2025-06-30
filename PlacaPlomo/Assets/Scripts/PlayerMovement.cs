using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 2f; // Velocidad de movimiento
    public float jumpForce = 5f; // Fuerza del salto
    public float mouseSensitivity = 10f; // Sensibilidad del mouse
    public Transform playerCamera; // Cmara del jugador

    // Variables públicas para la detección de suelo (configurar en el Inspector de Unity)
    public Transform groundCheck; // Objeto vacío que indica la posición para chequear el suelo
    public float groundCheckRadius = 0.2f; // Radio de la esfera para chequear el suelo
    public LayerMask groundLayer; // Capa de los objetos que consideramos "suelo"

    private float verticalRotation = 0f; // Rotacin vertical
    private Rigidbody rb; // Referencia al Rigidbody
    private bool isGrounded; // Verdadero si el jugador está en el suelo
    private Camera cam; // Referencia a la cmara principal
    private Animator animator; // Referencia al componente Animator

    void Start()
    {
        // Obtener referencias a los componentes necesarios
        rb = GetComponent<Rigidbody>();
        cam = Camera.main; // Busca la cámara con el tag "MainCamera"
        animator = GetComponent<Animator>();

        // Validaciones para asegurar que los componentes existen
        if (rb == null)
            Debug.LogError("Rigidbody no encontrado en el objeto Player.");

        if (cam == null)
            Debug.LogError("Cámara principal no encontrada. Asegúrate de que tenga el tag 'MainCamera'.");

        if (animator == null)
            Debug.LogError("Animator no encontrado en el objeto Player.");

        // Bloquear el cursor en el centro de la pantalla y hacerlo invisible para una experiencia de FPS
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- Movimiento del Jugador ---
        // Obtener inputs de movimiento
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calcular el vector de movimiento basado en la dirección del jugador
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        
        // Mover al jugador
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        // --- Rotación de Cámara y Jugador (Mirada) ---
        // Obtener inputs del mouse para la rotación
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Calcular la rotación vertical de la cámara (arriba/abajo)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Limitar la rotación para evitar voltear la cámara

        // Aplicar la rotación vertical a la cámara del jugador
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        
        // Aplicar la rotación horizontal al cuerpo del jugador
        transform.Rotate(Vector3.up * mouseX);

        // --- Detección de Suelo y Animaciones ---
        // Usar Physics.CheckSphere para determinar si el jugador está en el suelo
        // Esto crea una pequeña esfera en 'groundCheck' para ver si colisiona con la 'groundLayer'
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Actualizar parámetros del Animator
        animator.SetBool("isGrounded", isGrounded); // Indica al Animator si el jugador está en el suelo
        animator.SetFloat("VelocidadY", rb.linearVelocity.y); // Envía la velocidad vertical para animaciones de caída/salto

        // Calcular la magnitud del movimiento para el parámetro "Walk" del Animator
        // Esto ayuda a activar animaciones de caminar/correr
        Vector3 currentMoveDir = (transform.forward * vertical + transform.right * horizontal).normalized;
        animator.SetFloat("Walk", currentMoveDir.magnitude);

        // --- Salto ---
        // Detectar input de salto y si el jugador está en el suelo
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Aplicar una fuerza hacia arriba para el salto
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    // Los métodos OnCollisionEnter y OnCollisionExit se han eliminado
    // porque Physics.CheckSphere maneja la detección de suelo de manera más consistente
    // para este tipo de controlador de personaje.
}