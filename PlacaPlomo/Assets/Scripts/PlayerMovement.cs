using UnityEngine;

// Este script controla el movimiento y la vista de un jugador en primera persona.
// Requiere un componente Rigidbody para manejar las interacciones físicas.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    // Variable para activar y desactivar los controles del jugador.
    [Header("Funcionalidad")]
    public bool controlsEnabled = true;

    // Variables ajustables en el Inspector de Unity para el movimiento del jugador.
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Variables para controlar el movimiento de la cámara con el ratón.
    [Header("Cámara")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float mouseSensitivity = 2f;
    private float xRotation = 0f;

    // Variable para la fuente de audio de los pasos.
    [Header("Audio")]
    [SerializeField] private AudioSource pasos;

    // Variables internas que el script utiliza para su funcionamiento.
    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpInput;

    // --- Métodos de ciclo de vida de Unity ---

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraHolder == null)
            Debug.LogError("CameraHolder no asignado.");
    }

    void Update()
    {
        // Si los controles no están habilitados, no hacemos nada.
        if (!controlsEnabled) return;

        HandleMouseLook();
        CheckGrounded();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpInput = true;
        }

        HandleFootsteps();
    }

    void FixedUpdate()
    {
        // Si los controles no están habilitados, no hacemos nada.
        if (!controlsEnabled) return;

        HandleMovement();
        HandleJump();
    }

    // --- Métodos de funcionalidad del jugador ---

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.forward * v + transform.right * h;
        move = move.normalized * moveSpeed;

        // Mantiene la velocidad vertical actual (la gravedad)
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    private void HandleJump()
    {
        if (jumpInput)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpInput = false;
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleFootsteps()
    {
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        if (isMoving && !pasos.isPlaying)
        {
            pasos.Play();
        }
        else if (!isMoving && pasos.isPlaying)
        {
            pasos.Pause();
        }
    }

    // === Método de la API Pública para MissionManager ===

    /// <summary>
    /// Mueve el Rigidbody del jugador a una nueva posición de forma segura.
    /// </summary>
    public void TeleportTo(Vector3 newPosition)
    {
        if (rb == null) return;

        // Detenemos la velocidad inmediatamente
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Teletransporta al jugador
        rb.MovePosition(newPosition);

        jumpInput = false;
        isGrounded = true;
    }

    public void EnableControls()
    {
        controlsEnabled = true;
    }

    public void DisableControls()
    {
        controlsEnabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Detener el movimiento inmediatamente
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Dibuja un gizmo para visualizar la esfera de detección del suelo en el Editor de Unity.
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}