using UnityEngine;

public class PursuerAI : MonoBehaviour
{
    [Header("Objetivos")]
    public Transform target;        // Asigna aquí el Transform del jugador.

    [Header("Configuración de Movimiento")]
    public float maxSpeed = 15f;    // Velocidad máxima del carro NPC
    public float acceleration = 8f; // Fuerza para acelerar
    public float rotationSpeed = 3f; // Velocidad para girar hacia el objetivo
    public float captureDistance = 2f; // Distancia para que el perseguidor te atrape (AJUSTADO a 2m).

    private Rigidbody rb;
    private bool isChasing = false;
    private float chaseStartTime;
    [SerializeField] private float gracePeriod = 4.0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    public void StartChase()
    {
        if (target == null || rb == null) return;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isChasing = true;
        chaseStartTime = Time.time;

        Debug.Log("Perseguidor activado y en modo persecución.");
    }

    public void StopChase()
    {
        isChasing = false;
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }
        Debug.Log("Perseguidor desactivado.");
    }

    void FixedUpdate()
    {
        if (!isChasing || target == null)
            return;

        // ************ LÓGICA DE MOVIMIENTO DE COCHE ************

        Vector3 targetDirection = (target.position - transform.position).normalized;
        targetDirection.y = 0; // Solo en el plano horizontal

        // 1. Rotación (Rotar el Rigidbody hacia el objetivo)
        if (targetDirection != Vector3.zero)
        {
            // Calcula la rotación deseada
            Quaternion lookRotation = Quaternion.LookRotation(targetDirection);

            // Suaviza la rotación para un efecto más realista (coche girando)
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, lookRotation, Time.fixedDeltaTime * rotationSpeed);
            rb.MoveRotation(newRotation);
        }

        // 2. Movimiento (Aplicar fuerza hacia adelante del coche)
        // La fuerza siempre se aplica en la dirección 'transform.forward' del NPC,
        // no en la dirección cruda 'targetDirection'.
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            // Usamos transform.forward (la dirección actual del coche)
            rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
        }

        // Limita la velocidad (para evitar que acelere sin control)
        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0;
        rb.linearVelocity = Vector3.ClampMagnitude(flatVelocity, maxSpeed) + new Vector3(0, rb.linearVelocity.y, 0);


        // ************ LÓGICA DE CAPTURA CON GRACE PERIOD ************

        if (Time.time < chaseStartTime + gracePeriod)
        {
            return; // No se puede capturar durante el periodo de gracia.
        }

        // El grace period ha terminado.
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget < captureDistance)
        {
            Debug.LogWarning("¡Perseguidor te ha atrapado! Misión fallida. Distancia: " + distanceToTarget.ToString("F2") + "m");
            StopChase();
            // Llama al MissionManager para reportar el fallo.
            MissionManager.I?.ReportFailure("Perseguidor_Atrapado");
            return;
        }
    }
}