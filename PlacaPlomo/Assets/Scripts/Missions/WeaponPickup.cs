using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Configuraci�n del Arma")]
    [Tooltip("ID del arma, �til si tambi�n es una evidencia (ej: ARMA_Misteriosa).")]
    public string weaponItemId = "Weapon_Pistol";

    [Tooltip("El Prefab del arma que se mostrar� en la vista del jugador (FP View).")]
    public GameObject fpWeaponPrefab;

    [Header("Referencias de la Escena")]
    // Objeto vac�o en la jerarqu�a del jugador donde se instanciar� el arma de FP
    public Transform weaponHoldPoint;

    // Referencia al PlayerShooting.cs para activarlo
    private PlayerShooting playerShootingScript;

    private bool playerInRange = false;
    private PickupTextUI pickupTextUI;

    void Start()
    {
        // Buscar las referencias esenciales
        pickupTextUI = FindFirstObjectByType<PickupTextUI>();

        // Asumiendo que PlayerShooting.cs est� en la c�mara o el objeto padre del jugador
        playerShootingScript = FindFirstObjectByType<PlayerShooting>();

        if (playerShootingScript == null)
        {
            Debug.LogError("WeaponPickup: No se encontr� el script PlayerShooting en la escena.");
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            EquipWeapon();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void EquipWeapon()
    {
        if (playerShootingScript == null || fpWeaponPrefab == null || weaponHoldPoint == null)
        {
            Debug.LogError("Error al equipar el arma: Faltan referencias (Shooting Script, Prefab o Hold Point).");
            return;
        }

        // 1. Instanciar y equipar el modelo 3D
        Instantiate(fpWeaponPrefab, weaponHoldPoint.position, weaponHoldPoint.rotation, weaponHoldPoint);

        // 2. Activar la funcionalidad de disparo (asumiendo que est� inactiva por defecto)
        playerShootingScript.enabled = true;

        // 3. Notificar a la Misi�n si el arma es un objetivo (Opcional, pero bueno para misiones)
        // Aunque M2-07A no requiere 'recoger' el arma para avanzar, podr�amos a�adir la l�gica si el CSV lo necesitara:
        // MissionManager.I?.ReportEvent(TriggerType.Pickup, weaponItemId);

        // 4. Ocultar UI y destruir el objeto de recogida en la escena
        pickupTextUI?.HideText();
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickupTextUI?.ShowText($"Presiona E para recoger {weaponItemId}");
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickupTextUI?.HideText();
            playerInRange = false;
        }
    }
}