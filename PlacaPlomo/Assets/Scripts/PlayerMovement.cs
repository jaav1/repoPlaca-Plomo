using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Cámara")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float mouseSensitivity = 2f;
    private float xRotation = 0f;

    [Header("Audio")]
    [SerializeField] private AudioSource pasos;

    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpInput; // Variable para manejar la entrada del salto

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
        HandleMouseLook();
        CheckGrounded();

        // Almacenar la entrada del salto en Update
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpInput = true;
        }

        HandleFootsteps();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

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

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    private void HandleJump()
    {
        // Aplicar la fuerza de salto en FixedUpdate
        if (jumpInput)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpInput = false; // Resetear la variable
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

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}