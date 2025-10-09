using UnityEngine;

public class DestructibleTarget : MonoBehaviour
{
    [Tooltip("ID del objeto para notificar al MissionManager (ej: OBJ_LamparaTecho).")]
    public string targetId = "OBJ_LamparaTecho";

    [Header("Efectos")]
    public GameObject destructionEffectPrefab; // Partículas o modelo roto
    public AudioClip destructionSound;

    private MissionManager missionManager;
    private bool isDestroyed = false;

    void Start()
    {
        // Obtener la referencia al Singleton
        missionManager = MissionManager.I;
    }

    // Este método será llamado por el script de disparo (BulletController o RaycastShooting)
    public void HitByBullet()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // 1. Desactivar el componente Light de la lámpara (apagón)
        Light lightComponent = GetComponent<Light>();
        if (lightComponent != null)
        {
            lightComponent.enabled = false;
        }

        // 2. Ejecutar efectos
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, transform.rotation);
        }
        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        }

        // 3. Notificar al MissionManager
        if (missionManager != null)
        {
            // Reportamos el evento de disparo exitoso
            missionManager.ReportEvent(TriggerType.ShootTarget, targetId);
            Debug.Log($"Objetivo '{targetId}' destruido. Misión notificada.");
        }

        // Opcional: Destruir el objeto (o desactivar el renderizado)
        Destroy(gameObject.GetComponent<MeshRenderer>()); // Quita el modelo visual
        Destroy(gameObject.GetComponent<Collider>());      // Quita el colisionador
        Destroy(gameObject, 5f); // Destruye completamente el objeto después de 5 segundos
    }
}