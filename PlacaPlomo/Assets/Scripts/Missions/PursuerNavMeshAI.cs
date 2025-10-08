using UnityEngine;
using UnityEngine.AI; // ¡Importante! Necesario para usar NavMeshAgent

[RequireComponent(typeof(NavMeshAgent))]
public class PursuerNavMeshAI : MonoBehaviour
{
    [Header("Objetivos")]
    [SerializeField] private Transform playerTarget; // Referencia al jugador
    [SerializeField] private Transform carTarget;    // Referencia al coche (este mismo objeto)

    [Header("Configuración de Misión")]
    [SerializeField] private float captureDistance = 3f; // Un poco más para ser generoso
    [SerializeField] private float gracePeriod = 4.0f; // Tiempo antes de poder capturar.

    private VehicleInteraction vehicleInteractionRef;

    private NavMeshAgent agent;
    private bool isChasing = false;
    private float chaseStartTime;

    void Awake()
    {
        // Obtener el componente NavMeshAgent
        agent = GetComponent<NavMeshAgent>();

        // ¡CRUCIAL! Asegurarse de que el agente esté completamente inactivo al iniciar el juego
        if (agent != null)
        {
            agent.isStopped = true; // El agente no calcula ruta ni se mueve
            agent.velocity = Vector3.zero; // La velocidad es cero
        }

        agent.updateRotation = false;

        // Búsqueda del Target (Jugador) si no está asignado
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerTarget = player.transform; // Asignamos playerTarget
            else
                Debug.LogError("PursuerNavMeshAI: Target (Jugador) no encontrado con la etiqueta 'Player'.");
        }

        // Asignamos el target del coche a este mismo objeto (si el coche está en el mismo objeto)
        // **IMPORTANTE**: Asegúrate de que este script esté en el OBJETO del Perseguidor, no del Coche.

        // Asumiendo que el Pursuer necesita el Transform del COCHE.
        // Si el Perseguidor tiene que seguir el objeto del coche, añade una referencia a ese objeto:
        if (carTarget == null)
        {
            GameObject carObj = GameObject.Find("Car"); 
            if (carObj != null) carTarget = carObj.transform;
        }

        // Busca el Singleton de VehicleInteraction
        vehicleInteractionRef = FindFirstObjectByType<VehicleInteraction>();
        if (vehicleInteractionRef == null) Debug.LogWarning("VehicleInteraction no encontrado, el perseguidor solo seguirá al jugador.");

        // El NavMeshAgent tiene un valor por defecto que debe sobrescribirse.
        agent.stoppingDistance = captureDistance;
    }

    // Método para obtener el objetivo dinámico
    private Transform GetCurrentTarget()
    {
        // Prioridad: 1. Coche (si el jugador está dentro) 2. Jugador
        if (vehicleInteractionRef != null && vehicleInteractionRef.IsPlayerInside && carTarget != null)
        {
            // Si el jugador está dentro del coche, perseguir el coche.
            return carTarget;
        }
        else
        {
            // Si no está conduciendo, perseguir al jugador.
            return playerTarget;
        }
    }

    public void StartChase()
    {
        // CORRECCIÓN 1: Usamos GetCurrentTarget() para una verificación de null más robusta.
        if (GetCurrentTarget() == null || agent == null) return;

        agent.isStopped = false; // El agente comienza a moverse
        isChasing = true;
        chaseStartTime = Time.time;
        Debug.Log("Perseguidor activado y usando NavMesh.");
    }

    public void StopChase()
    {
        isChasing = false;
        if (agent != null)
        {
            agent.isStopped = true; // El agente se detiene inmediatamente
            agent.velocity = Vector3.zero;
        }
        Debug.Log("Perseguidor desactivado.");
    }

    void Update()
    {
        // 1. Obtiene el objetivo dinámico Coche o Jugador
        Transform currentTarget = GetCurrentTarget();

        // CORRECCIÓN 2: Reemplazamos la referencia a 'target' en el escudo.
        if (!isChasing || currentTarget == null || agent.isStopped)
            return;

        // 2. Lógica principal: Establece la ruta al objetivo dinámico
        agent.SetDestination(currentTarget.position);

        // 3. Simulación de rotación (Opcional)
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // 4. Lógica de Captura: Usa la distancia al objetivo dinámico
        if (Time.time < chaseStartTime + gracePeriod)
            return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= captureDistance)
        {
            // El perseguidor te ha atrapado.
            StopChase();

            if (MissionManager.I != null)
            {
                MissionManager.I.PlayerCaught();
            }
            else
            {
                Debug.LogError("PursuerNavMeshAI: MissionManager.I no es accesible. ¡No se pudo fallar la misión!");
            }
        }
    }
}