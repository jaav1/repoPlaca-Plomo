using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public float maxDistance = 100f;
    public LayerMask hitLayers; // Asegúrate de que la lámpara esté en esta capa
    public GameObject muzzleFlashPrefab;
    public AudioClip shotSound;
    public Transform shootPoint; // El punto de origen del Raycast (ej: la punta del arma/cámara)

    [Tooltip("El ID del arma que debe usarse para reportar al MissionManager.")]
    public string weaponId = "Arma"; // Usado para el requisito de M2-07A (Arma)

    // Referencia al MissionManager para verificar si se permite disparar
    private MissionManager missionManager;

    void Start()
    {
        missionManager = MissionManager.I;
        if (shootPoint == null) shootPoint = transform; // Usa la propia posición si no se asigna
    }

    void Update()
    {
        // Verifica si el jugador presiona el botón de disparo (ej: botón izquierdo del ratón)
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Opcional: Podrías añadir lógica aquí para asegurarte de que solo puedas disparar
        // si la misión está en M2-07A. Por ahora, asumimos que el jugador puede disparar.

        // Efectos Visuales y Sonoros
        if (muzzleFlashPrefab != null)
        {
            Instantiate(muzzleFlashPrefab, shootPoint.position, shootPoint.rotation);
        }
        if (shotSound != null)
        {
            AudioSource.PlayClipAtPoint(shotSound, shootPoint.position);
        }

        RaycastHit hit;

        // Lanza un rayo desde el punto de disparo
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, maxDistance, hitLayers))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Intenta obtener el componente DestructibleTarget
            DestructibleTarget target = hit.collider.GetComponent<DestructibleTarget>();

            if (target != null)
            {
                // Si es un objetivo destructible (nuestra lámpara), lo golpeamos.
                target.HitByBullet();
            }

            // Opcional: Crear un efecto de impacto en la superficie (decal o partículas)
        }
    }
}