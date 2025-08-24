using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Configuraci�n del �tem")]
    public string itemName = "Objeto";      // Nombre del �tem a recoger
    public string itemPage = "General";     // P�gina o categor�a del inventario donde se guardar�

    [Header("Configuraci�n de Audio")]
    public AudioClip pickupSound;           // Sonido que se reproduce al recoger el �tem
    private AudioSource audioSource;        // Componente que reproducir� el sonido

    private bool canPickup = false;         // Indica si el jugador est� dentro del �rea de recogida
    private PickupTextUI pickupTextUI;      // Referencia al UI que muestra el mensaje "Presiona E..."
    private RadialInventoryManager inventoryManager; // Referencia al sistema de inventario radial

    void Start()
    {
        // Buscar las referencias autom�ticamente al iniciar
        pickupTextUI = FindFirstObjectByType<PickupTextUI>();
        inventoryManager = FindFirstObjectByType<RadialInventoryManager>();
        audioSource = GetComponent<AudioSource>();

        // Si no encuentra el UI, muestra un error
        if (!pickupTextUI)
            Debug.LogError("[ItemPickup] pickupTextUI no se encontr� en la escena.");

        // Si no encuentra el inventario radial, muestra un error
        if (!inventoryManager)
            Debug.LogError("[ItemPickup] RadialInventoryManager no se encontr� en la escena.");

        // Si no hay AudioSource en este objeto, se crea uno autom�ticamente
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Evitar que suene solo al iniciar
        }
    }

    void Update()
    {
        // Si el jugador est� en rango y presiona E
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Intentando recoger: {itemName} ({itemPage})");

            // Oculta el texto de "Presiona E..."
            pickupTextUI?.HideText();

            // Verifica que el �tem tenga nombre v�lido y que el inventario exista
            if (!string.IsNullOrEmpty(itemName) && inventoryManager != null)
            {
                // Intenta a�adir el �tem al inventario
                bool added = inventoryManager.TryAddItem(itemName, itemPage);

                if (added)
                {
                    // Si existe el GameManager, agrega el �tem y guarda
                    if (GameManager.instancia != null)
                    {
                        GameManager.instancia.AddItem(itemName);
                        GameManager.instancia.GuardarAhora();
                    }
                    else
                    {
                        // Si no hay GameManager, solo muestra advertencia (no crashea)
                        Debug.LogWarning("[ItemPickup] No hay GameManager en la escena. El �tem no se guardar�.");
                    }

                    // Reproducir sonido de recogida si existe
                    if (pickupSound != null && audioSource != null)
                        audioSource.PlayOneShot(pickupSound);

                    // Destruir el objeto recogido tras 1 segundo
                    Destroy(gameObject, 1f);
                }
                else
                {
                    // Si el inventario est� lleno, muestra mensaje
                    Debug.Log($"{itemName} no se recogi� porque el inventario est� lleno.");
                }
            }
        }
    }

  /*  void LateUpdate()
    {
        // Efecto "billboard": hacer que el objeto mire hacia la c�mara
        if (Camera.main != null)
        {
            Vector3 lookPos = transform.position + Camera.main.transform.forward;
            transform.LookAt(lookPos);
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    } */

    void OnTriggerEnter(Collider other)
    {
        // Cuando el jugador entra en el �rea de recogida
        if (other.CompareTag("Player"))
        {
            // Muestra el texto de interacci�n
            pickupTextUI?.ShowText($"Presiona E para recoger {itemName}");
            canPickup = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Cuando el jugador sale del �rea de recogida
        if (other.CompareTag("Player"))
        {
            // Oculta el texto de interacci�n
            pickupTextUI?.HideText();
            canPickup = false;
        }
    }
}
