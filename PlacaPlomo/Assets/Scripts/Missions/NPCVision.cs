using UnityEngine;

public class NPCVision : MonoBehaviour
{
    public float viewAngle = 90f;
    public float viewDistance = 10f;
    public LayerMask obstructionMask; // Capa para obstáculos (paredes)

    // Referencia al jugador (asume que el jugador tiene el tag "Player")
    private Transform playerTarget;
    private MissionManager missionManager;

    void Start()
    {
        missionManager = MissionManager.I;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTarget = player.transform;
    }

    void Update()
    {
        if (playerTarget == null) return;

        // Solo revisar si la misión está activa en M2-07B
        //if (missionManager != null && missionManager.CurrentStepId != "M2-07B") return;

        // 1. Calcular la dirección y distancia al jugador
        Vector3 directionToTarget = (playerTarget.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, playerTarget.position);

        // 2. Comprobar si está dentro del cono de visión y distancia
        if (distanceToTarget < viewDistance && Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
        {
            // 3. Comprobar si no hay obstáculos (Raycast)
            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
            {
                // ¡JUGADOR DETECTADO!
                PlayerDetected();
            }
        }
    }

    void PlayerDetected()
    {
        Debug.Log("¡El jugador ha sido detectado por " + gameObject.name + "!");

        // 1. Notificar al MissionManager el fallo del sigilo
        if (missionManager != null)
        {
            // El fallo de sigilo fuerza una transición a una versión de M2-07A (o un reinicio)
            // Aquí forzamos la opción de 'Escape tenso' como penalización.
            // Para fines de esta misión, simplificaremos a un simple reinicio o fallo de paso.

            // Notificación al MissionManager: fallo de sigilo
            missionManager.ReportEvent(TriggerType.Stealth, "DETECCION_FALLIDA");
            // Podrías programar MissionManager para que al recibir DETECCION_FALLIDA,
            // reinicie el paso M2-07B o fuerce el avance a la ruta M2-07A (como penalización).
        }

        // Bloquea el NPC para que solo reporte una vez.
        enabled = false;
    }
}