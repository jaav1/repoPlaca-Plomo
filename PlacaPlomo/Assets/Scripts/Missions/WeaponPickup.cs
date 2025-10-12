using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Configuración del Arma")]
    [Tooltip("ID del arma, útil si también es una evidencia (ej: ARMA_Misteriosa).")]
    public string weaponItemId = "Weapon_Pistol";

    [Tooltip("El Prefab del arma que se mostrará en la vista del jugador (FP View).")]
    public GameObject fpWeaponPrefab;

    [Header("Referencias de la Escena")]
    // Objeto vacío en la jerarquía del jugador donde se instanciará el arma de FP
    public Transform weaponHoldPoint;

    [Header("Configuración de Transformación")]
    [Tooltip("Escala LOCAL deseada para el arma una vez equipada.")]
    public Vector3 equippedLocalScale = new Vector3(20f, 20f, 3f);

    // Referencia al PlayerShooting.cs para activarlo
    private PlayerShooting playerShootingScript;

    private bool playerInRange = false;
    private PickupTextUI pickupTextUI;

    void Start()
    {
        // Buscar las referencias esenciales
        pickupTextUI = FindFirstObjectByType<PickupTextUI>();

        // Asumiendo que PlayerShooting.cs está en la cámara o el objeto padre del jugador
        playerShootingScript = FindFirstObjectByType<PlayerShooting>();

        if (playerShootingScript == null)
        {
            Debug.LogError("WeaponPickup: No se encontró el script PlayerShooting en la escena.");
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

        // 1. Instanciar el modelo 3D sin padre aún
        GameObject newWeapon = Instantiate(fpWeaponPrefab);

        // 2. Asignar el punto de agarre como padre
        newWeapon.transform.SetParent(weaponHoldPoint);

        // 3. Resetear la posición, rotación y ESCALA LOCAL
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        // La línea que soluciona el problema de la deformación:
        newWeapon.transform.localScale = equippedLocalScale;

        // 4. Activar la funcionalidad de disparo (asumiendo que está inactiva por defecto)
        playerShootingScript.enabled = true;

        // 5. Notificar a la Misión si el arma es un objetivo (Opcional, pero bueno para misiones)
        // MissionManager.I?.ReportEvent(TriggerType.Pickup, weaponItemId);

        // 6. Ocultar UI y destruir el objeto de recogida en la escena
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