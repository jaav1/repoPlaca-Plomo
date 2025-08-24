using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Configuración del ítem")]
    public string itemName = "Objeto";      // Nombre del ítem a recoger
    public string itemPage = "General";     // Página o categoría del inventario donde se guardará

    [Header("Configuración de Audio")]
    public AudioClip pickupSound;           // Sonido que se reproduce al recoger el ítem
    private AudioSource audioSource;        // Componente que reproducirá el sonido

    private bool canPickup = false;         // Indica si el jugador está dentro del área de recogida
    private PickupTextUI pickupTextUI;      // Referencia al UI que muestra el mensaje "Presiona E..."
    private RadialInventoryManager inventoryManager; // Referencia al sistema de inventario radial

    void Start()
    {
        // Buscar las referencias automáticamente al iniciar
        pickupTextUI = FindFirstObjectByType<PickupTextUI>();
        inventoryManager = FindFirstObjectByType<RadialInventoryManager>();
        audioSource = GetComponent<AudioSource>();

        // Si no encuentra el UI, muestra un error
        if (!pickupTextUI)
            Debug.LogError("[ItemPickup] pickupTextUI no se encontró en la escena.");

        // Si no encuentra el inventario radial, muestra un error
        if (!inventoryManager)
            Debug.LogError("[ItemPickup] RadialInventoryManager no se encontró en la escena.");

        // Si no hay AudioSource en este objeto, se crea uno automáticamente
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Evitar que suene solo al iniciar
        }
    }

    void Update()
    {
        // Si el jugador está en rango y presiona E
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Intentando recoger: {itemName} ({itemPage})");

            // Oculta el texto de "Presiona E..."
            pickupTextUI?.HideText();

            // Verifica que el ítem tenga nombre válido y que el inventario exista
            if (!string.IsNullOrEmpty(itemName) && inventoryManager != null)
            {
                // Intenta añadir el ítem al inventario
                bool added = inventoryManager.TryAddItem(itemName, itemPage);

                if (added)
                {
                    // Si existe el GameManager, agrega el ítem y guarda
                    if (GameManager.instancia != null)
                    {
                        GameManager.instancia.AddItem(itemName);
                        GameManager.instancia.GuardarAhora();
                    }
                    else
                    {
                        // Si no hay GameManager, solo muestra advertencia (no crashea)
                        Debug.LogWarning("[ItemPickup] No hay GameManager en la escena. El ítem no se guardará.");
                    }

                    // Reproducir sonido de recogida si existe
                    if (pickupSound != null && audioSource != null)
                        audioSource.PlayOneShot(pickupSound);

                    // Destruir el objeto recogido tras 1 segundo
                    Destroy(gameObject, 1f);
                }
                else
                {
                    // Si el inventario está lleno, muestra mensaje
                    Debug.Log($"{itemName} no se recogió porque el inventario está lleno.");
                }
            }
        }
    }

  /*  void LateUpdate()
    {
        // Efecto "billboard": hacer que el objeto mire hacia la cámara
        if (Camera.main != null)
        {
            Vector3 lookPos = transform.position + Camera.main.transform.forward;
            transform.LookAt(lookPos);
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    } */

    void OnTriggerEnter(Collider other)
    {
        // Cuando el jugador entra en el área de recogida
        if (other.CompareTag("Player"))
        {
            // Muestra el texto de interacción
            pickupTextUI?.ShowText($"Presiona E para recoger {itemName}");
            canPickup = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Cuando el jugador sale del área de recogida
        if (other.CompareTag("Player"))
        {
            // Oculta el texto de interacción
            pickupTextUI?.HideText();
            canPickup = false;
        }
    }
}
