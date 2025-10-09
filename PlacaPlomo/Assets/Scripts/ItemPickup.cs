using UnityEngine;
using System.Collections.Generic;

public class ItemPickup : MonoBehaviour
{
    [Header("Configuración del ítem")]
    public string itemName = "Objeto";      // Nombre del ítem a recoger
    public string itemPage = "General";     // Página o categoría del inventario donde se guardará

    public string itemId;                   // ID único del ítem para el sistema de guardado

    [Header("Configuración de Audio")]
    public AudioClip pickupSound;           // Sonido que se reproduce al recoger el ítem
    private AudioSource audioSource;        // Componente que reproducirá el sonido

    private bool canPickup = false;         // Indica si el jugador está dentro del área de recogida
    private PickupTextUI pickupTextUI;      // Referencia al UI que muestra el mensaje "Presiona E..."
    private RadialInventoryManager inventoryManager; // Referencia al sistema de inventario radial
    private MissionManager missionManager;

    void Start()
    {
        // Buscar las referencias automáticamente al iniciar
        pickupTextUI = FindFirstObjectByType<PickupTextUI>();
        inventoryManager = FindFirstObjectByType<RadialInventoryManager>();

        // Asignación del Singleton de MissionManager
        missionManager = MissionManager.I;

        // Se asegura de que haya un GameManager en la escena
        if (GameManager.instancia == null)
        {
            Debug.LogError("[ItemPickup] GameManager no se encontró en la escena.");
        }

        // Si no hay AudioSource en este objeto, se crea uno automáticamente
        audioSource = GetComponent<AudioSource>();
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
            // Oculta el texto de "Presiona E..."
            pickupTextUI?.HideText();

            // Verifica que el inventario exista
            if (inventoryManager != null)
            {
                // Ahora, llamamos directamente al método AddItem que creaste.
                inventoryManager.AddItem(itemName, itemPage);

                // Muestra un mensaje de éxito
                Debug.Log($"Objeto {itemName} recogido.");

                // Reproducir sonido de recogida si existe
                if (pickupSound != null && audioSource != null)
                    audioSource.PlayOneShot(pickupSound);

                // NOTIFICAR AL MISSION MANAGER
                if (missionManager != null && !string.IsNullOrEmpty(itemId))
                {
                    // Reportamos que un objeto ha sido recogido, usando itemId como target.
                    missionManager.ReportEvent(TriggerType.Pickup, itemId);
                    Debug.Log($"[ItemPickup] Reportado al MissionManager: Trigger=Pickup, Target={itemId}");
                }

                // NOTIFICA AL GAMEMANAGER QUE EL OBJETO FUE RECOGIDO
                if (GameManager.instancia != null)
                {
                    // Usa el 'itemId' en lugar del nombre del objeto.
                    GameManager.instancia.MarcarObjetoComoRecogido(itemId);
                }

                // Destruir el objeto recogido tras 1 segundo
                Destroy(gameObject, 1f);
            }
        }
    }

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