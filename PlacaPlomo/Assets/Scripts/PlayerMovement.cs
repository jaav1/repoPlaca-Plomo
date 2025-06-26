using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float rotationSmoothSpeed = 3f;

    private Rigidbody rb;
    private Camera cam;
    private bool isGrounded;

    [Header("Animacion")]

    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        animator = GetComponent<Animator>();

        if (rb == null)
            Debug.LogError("Rigidbody no encontrado en el objeto Player");

        if (cam == null)
            Debug.LogError("Camara principal no encontrada. Asegurate de que tenga el tag 'MainCamera'");
    }

    void Update()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);

        animator.SetFloat("VelocidadY", rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.transform.right;

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        animator.SetFloat("Walk", moveDir.magnitude);

        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        // Este bloque evita rotaciones bruscas hacia atrás
        if (moveDir != Vector3.zero)
        {
            float dot = Vector3.Dot(moveDir, transform.forward);
            if (dot > -0.5f) // Más estricto para no rotar si va en dirección contraria
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSmoothSpeed);
            }
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
