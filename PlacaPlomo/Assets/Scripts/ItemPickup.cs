using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Configuración del ítem")]
    public string itemName = "Objeto";
    public string itemPage = "General";

    private bool canPickup = false;
    private PickupTextUI pickupTextUI;
    private RadialInventoryManager inventoryManager;

    void Start()
    {
        pickupTextUI = FindFirstObjectByType<PickupTextUI>();
        inventoryManager = FindFirstObjectByType<RadialInventoryManager>();

        if (!pickupTextUI)
            Debug.LogError("[ItemPickup] pickupTextUI no se encontró en la escena.");

        if (!inventoryManager)
            Debug.LogError("[ItemPickup] RadialInventoryManager no se encontró en la escena.");
    }

    void Update()
    {
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Recogiendo: {itemName} ({itemPage})");

            pickupTextUI?.HideText();

            if (!string.IsNullOrEmpty(itemName))
                inventoryManager?.AddItem(itemName, itemPage);

            Destroy(gameObject);
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