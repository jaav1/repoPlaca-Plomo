using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Configuraci�n del �tem")]
    public string itemName = "Objeto";
    public string itemPage = "General";

    [Header("Configuraci�n de Audio")]
    public AudioClip pickupSound; // Aqu� asignaremos el sonido de recogida
    private AudioSource audioSource; // Referencia al componente AudioSource

    private bool canPickup = false;
    private PickupTextUI pickupTextUI;
    private RadialInventoryManager inventoryManager;

    void Start()
    {
        pickupTextUI = FindFirstObjectByType<PickupTextUI>();
        inventoryManager = FindFirstObjectByType<RadialInventoryManager>();
        audioSource = GetComponent<AudioSource>(); // Obtenemos el componente AudioSource de este GameObject

        if (!pickupTextUI)
            Debug.LogError("[ItemPickup] pickupTextUI no se encontr� en la escena.");

        if (!inventoryManager)
            Debug.LogError("[ItemPickup] RadialInventoryManager no se encontr� en la escena.");

        // Opcional: Si el AudioSource no existe, lo creamos y configuramos.
        // Esto es �til si no quieres a�adirlo manualmente en el Inspector.
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Queremos reproducirlo cuando recojamos, no al inicio
        }
    }

    void Update()
    {
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Recogiendo: {itemName} ({itemPage})");

            pickupTextUI?.HideText();

            if (!string.IsNullOrEmpty(itemName))
                inventoryManager?.AddItem(itemName, itemPage);

            // --- NUEVO C�DIGO PARA EL SONIDO ---
            if (pickupSound != null && audioSource != null)
            {
                // Reproducimos el sonido. Si el AudioSource est� en este GameObject,
                // se destruir� con el �tem, lo cual est� bien si el sonido es corto.
                // Si el sonido es largo y quieres que termine de reproducirse,
                // necesitar�as un sistema de audio global o un AudioSource temporal.
                audioSource.PlayOneShot(pickupSound);
            }
            // --- FIN NUEVO C�DIGO ---

            // Destruimos el objeto un poco despu�s para permitir que el sonido se reproduzca si es corto
            // Si el sonido es m�s largo, podr�as necesitar otra estrategia (ver nota arriba).
            Destroy(gameObject, 1f); // Destruye el objeto 0.1 segundos despu�s
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