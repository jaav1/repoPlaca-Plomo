using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Configuración del ítem")]
    public string itemName = "Objeto";
    public string itemPage = "General";

    [Header("Configuración de Audio")]
    public AudioClip pickupSound;
    private AudioSource audioSource;

    private bool canPickup = false;
    private PickupTextUI pickupTextUI;
    private RadialInventoryManager inventoryManager;

    void Start()
    {
        pickupTextUI = FindFirstObjectByType<PickupTextUI>();
        inventoryManager = FindFirstObjectByType<RadialInventoryManager>();
        audioSource = GetComponent<AudioSource>();

        if (!pickupTextUI)
            Debug.LogError("[ItemPickup] pickupTextUI no se encontró en la escena.");

        if (!inventoryManager)
            Debug.LogError("[ItemPickup] RadialInventoryManager no se encontró en la escena.");

        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Intentando recoger: {itemName} ({itemPage})");
            pickupTextUI?.HideText();

            if (!string.IsNullOrEmpty(itemName) && inventoryManager != null)
            {
                bool added = inventoryManager.TryAddItem(itemName, itemPage);

                if (added)
                {
                    // Agregar a la lista de objetos recogidos en GameManager
                    GameManager.instancia.AddItem(itemName);

                    // Guardar automáticamente la partida
                    GameManager.instancia.GuardarAhora();

                    if (pickupSound != null && audioSource != null)
                        audioSource.PlayOneShot(pickupSound);

                    Destroy(gameObject, 1f); // Solo destruir si se agregó
                }
                else
                {
                    Debug.Log($"{itemName} no se recogió porque el inventario está lleno.");
                }
            }
        }
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Vector3 lookPos = transform.position + Camera.main.transform.forward;
            transform.LookAt(lookPos);
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickupTextUI?.ShowText($"Presiona E para recoger {itemName}");
            canPickup = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickupTextUI?.HideText();
            canPickup = false;
        }
    }
}
