using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VehicleInteraction : MonoBehaviour
{
    // === REFERENCIAS EN EL INSPECTOR ===
    [Header("Referencias de Cámaras y Puntos")]
    public Transform entryPoint;
    public Transform interiorCameraPoint;

    [Header("Referencias de UI")]
    public TMP_Text interactionText;
    public GameObject interactionPanel;
    public GameObject inspectionPanel;
    public Canvas inspectionCanvas;
    public RawImage inspectionRawImage;
    public RawImage trunkInspectionRawImage;

    public GameObject trunkInspectionPanel;

    public GameObject cablePuzzlePanel;
    public GameObject fusePuzzlePanel;

    [Header("Referencias de Otros Scripts")]
    public CarIgnition carIgnition;
    private CarController carController;
    private GameObject player;
    private PlayerMovement playerMovement;

    public TrunkLock trunkLock;

    public RadialInventoryManager inventoryManager;

    [Header("UI de Mensajes")]
    public GameObject messagePanel;
    public TMP_Text messageText;

    [SerializeField] private HotwirePromptUI hotwirePromptUI;

    public CameraManager cameraManager;

    // === ESTADOS INTERNOS ===
    private bool playerNearby = false;
    private bool isPlayerInside = false;
    private bool isInspecting = false;
    private bool isTrunkInspecting = false; // ¡Estado nuevo!

    void Awake()
    {
        carController = GetComponent<CarController>();
        if (carIgnition == null) carIgnition = GetComponent<CarIgnition>();
    }

    void Start()
    {
        interactionPanel.SetActive(false);
        inspectionPanel.SetActive(false);
        if (inspectionCanvas != null)
            inspectionCanvas.gameObject.SetActive(false);

        if (hotwirePromptUI != null) hotwirePromptUI.Hide();
    }

    void Update()
    {
        // === LÓGICA DE MANEJO DE ENTRADAS DEL JUGADOR ===
        // Esta sección maneja todas las entradas de teclado y ratón de manera independiente.

        // Manejar los clics del ratón en modo de inspección.
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }

        // Manejar la tecla 'F' para inspeccionar y salir de la inspección.
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Si ya estás inspeccionando, sal de la inspección.
            if (isInspecting)
            {
                ExitInspection();
            }
            // Si no estás inspeccionando y estás cerca del vehículo, entra en modo de inspección interior.
            else if (playerNearby && !isTrunkInspecting)
            {
                EnterInspectionMode();
            }
        }

        // Manejar la tecla 'R' para abrir/cerrar el maletero.
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Si estás inspeccionando el maletero, sal de la inspección.
            if (isTrunkInspecting)
            {
                ExitInspection();
            }
            // Si no estás inspeccionando el maletero y estás cerca, intenta abrirlo.
            else if (playerNearby)
            {
                // Revisa si el jugador tiene la llave.
                if (inventoryManager != null && trunkLock != null && inventoryManager.HasItem(trunkLock.keyID))
                {
                    EnterTrunkInspectionMode();
                }
                // Si no tiene la llave, muestra un mensaje.
                else
                {
                    ShowMessage("Necesitas la llave del coche para abrir el maletero.");
                }
            }
        }

        // Manejar la tecla 'E' para entrar al vehículo.
        if (Input.GetKeyDown(KeyCode.E) && playerNearby)
        {
            EnterVehicle();
        }

        // Manejar la tecla 'Q' para salir del vehículo.
        if (Input.GetKeyDown(KeyCode.Q) && isPlayerInside)
        {
            ExitVehicle();
        }

        // === LÓGICA PARA MOSTRAR LA UI DE INTERACCIÓN ===
        // Muestra la UI solo si el jugador está cerca y no está en ningún modo especial.
        if (playerNearby && !isInspecting && !isTrunkInspecting && !isPlayerInside)
        {
            ShowInteractionPrompt(true);
        }
        else
        {
            ShowInteractionPrompt(false);
        }
    }

    // --- Lógica del Raycast para los clics ---
    public void HandleMouseClick()
    {
        Debug.Log("HandleMouseClick() ha sido llamado.");

        // Escudo para evitar clics fuera del modo de inspección.
        if (!isInspecting && !isTrunkInspecting || !Cursor.visible)
        {
            Debug.Log("HandleMouseClick() - Saliendo. Estado de inspección incorrecto o cursor invisible."); // <--- NUEVO LOG 2
            return;
        }

        Camera activeInspectionCamera = null;
        RawImage activeRawImage = null;

        // Lógica para el modo de inspección INTERIOR.
        if (isInspecting)
        {
            activeInspectionCamera = cameraManager.inspectionCamera;
            activeRawImage = inspectionRawImage;
        }
        // Lógica para el modo de inspección del MALETERO.
        else if (isTrunkInspecting)
        {
            activeInspectionCamera = cameraManager.trunkInspectionCamera;
            activeRawImage = trunkInspectionRawImage;
        }

        // Si no se asignó una cámara o RawImage, no hacemos nada.
        if (activeInspectionCamera == null || activeRawImage == null)
        {
            Debug.LogWarning("HandleMouseClick() - Saliendo. La cámara o la RawImage no están asignadas. Camera: " + activeInspectionCamera + ", RawImage: " + activeRawImage); // <--- NUEVO LOG 3
            return;
        }
        Debug.Log("HandleMouseClick() - Todos los requisitos pasados. Preparando Raycast..."); // <--- NUEVO LOG 4

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            activeRawImage.rectTransform,
            Input.mousePosition,
            null,
            out localPoint
        );

        Vector2 normalizedPoint = new Vector2(
            (localPoint.x + activeRawImage.rectTransform.rect.width / 2) / activeRawImage.rectTransform.rect.width,
            (localPoint.y + activeRawImage.rectTransform.rect.height / 2) / activeRawImage.rectTransform.rect.height
        );

        // Crea un layerMask para los objetos que quieres que el Raycast detecte.
        int layerMask = 1 << LayerMask.NameToLayer("Collectibles");

        Ray ray = activeInspectionCamera.ViewportPointToRay(normalizedPoint);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 5f);

        // Usa el layerMask en el Raycast
        if (Physics.Raycast(ray, out hit, 100f, layerMask))
        {
            ClueObject clue = hit.collider.GetComponent<ClueObject>();

            if (clue != null)
            {
                if (clue.OnClickAction != null && clue.OnClickAction.GetPersistentEventCount() > 0)
                {
                    // Si hay un evento en el Inspector, ejecútalo.
                    clue.OnClickAction.Invoke();
                    inventoryManager.AddItem(clue.clueID, "General");
                    MissionManager.I?.ReportEvent(TriggerType.Pickup, clue.clueID);

                    Destroy(hit.collider.gameObject);
                    ShowMessage("Has encontrado " + clue.clueID + " y lo has agregado a tu inventario.");
                }
                else if (!string.IsNullOrEmpty(clue.clueID))
                {
                    // Este bloque es un "fallback".
                    inventoryManager.AddItem(clue.clueID, "General");

                    // AÑADIDO: Notifica al MissionManager que un objeto ha sido recogido
                    MissionManager.I?.ReportEvent(TriggerType.Pickup, clue.clueID);

                    Destroy(hit.collider.gameObject);
                    ShowMessage("Has encontrado " + clue.clueID + " y lo has agregado a tu inventario.");
                }
                else
                {
                    ShowMessage("Parece que aqui no hay nada, sigue buscando.");
                }
            }
        }
    }

    // --- Métodos de Interacción y Salida ---
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerMovement = player.GetComponent<PlayerMovement>();
            playerNearby = true;
            ShowInteractionPrompt(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            ShowInteractionPrompt(false);
            if (hotwirePromptUI != null) hotwirePromptUI.Hide();
        }
    }

    private void ShowInteractionPrompt(bool show)
    {
        if (show)
        {
            interactionPanel.SetActive(true);
            
            interactionText.text = "Presiona E para conducir\nPresiona F para inspeccionar\nPresiona R para abrir el maletero";
        }
        else
        {
            interactionPanel.SetActive(false);
        }
    }

    void EnterVehicle()
    {
        isPlayerInside = true;

        if (playerMovement != null) playerMovement.enabled = false;
        if (player != null) player.GetComponent<Renderer>().enabled = false;

        if (cameraManager != null) cameraManager.SwitchToCarCamera();

        ShowInteractionPrompt(false);
        carIgnition.TryIgnite();
        carIgnition.ShowPlayerSprite(true);
    }

    public void ExitVehicle()
    {
        Debug.Log("¡ExitVehicle() ha sido llamado!");

        isPlayerInside = false;
        isTrunkInspecting = false; // Resetea el estado del maletero
        isInspecting = false;      // Resetea el estado del interior

        if (player != null)
        {
            player.transform.position = entryPoint.position;
            if (playerMovement != null) playerMovement.enabled = true;
            if (player != null) player.GetComponent<Renderer>().enabled = true;
        }

        if (cameraManager != null) cameraManager.SwitchToPlayerCamera();

        interactionPanel.SetActive(false);
        if (hotwirePromptUI != null) hotwirePromptUI.Hide();
        if (cablePuzzlePanel != null) cablePuzzlePanel.SetActive(false);
        if (fusePuzzlePanel != null) fusePuzzlePanel.SetActive(false);
        carIgnition.TurnOff();
        carIgnition.ShowPlayerSprite(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void EnterInspectionMode()
    {
        isInspecting = true;
        isTrunkInspecting = false;

        if (playerMovement != null) playerMovement.enabled = false;
        if (player != null) player.GetComponent<Renderer>().enabled = false;

        ShowInteractionPrompt(false);
        if (inspectionCanvas != null)
            inspectionCanvas.gameObject.SetActive(true);

        // ACTIVA el panel del interior y DESACTIVA el del maletero
        inspectionPanel.SetActive(true);
        if (trunkInspectionPanel != null) trunkInspectionPanel.SetActive(false);

        // Asegúrate de que los otros paneles de puzzle estén desactivados
        if (cablePuzzlePanel != null) cablePuzzlePanel.SetActive(false);
        if (fusePuzzlePanel != null) fusePuzzlePanel.SetActive(false);

        if (cameraManager != null) cameraManager.SwitchToInspectionCamera();

        if (cameraManager != null)
        {
            cameraManager.inspectionCamera.transform.position = interiorCameraPoint.position;
            cameraManager.inspectionCamera.transform.rotation = interiorCameraPoint.rotation;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- métodos para la inspección del maletero! ---
    public void EnterTrunkInspectionMode()
    {
        // Centraliza la llamada a UnlockTrunk aquí
        if (trunkLock != null)
        {
            trunkLock.UnlockTrunk();
        }

        // Activa el estado de inspección
        isTrunkInspecting = true;
        isInspecting = false;

        // Controla la UI y la cámara
        if (playerMovement != null) playerMovement.enabled = false;
        if (player != null) player.GetComponent<Renderer>().enabled = false;

        ShowInteractionPrompt(false);
        if (inspectionCanvas != null)
        {
            inspectionCanvas.gameObject.SetActive(true);
            trunkInspectionPanel.SetActive(true);
        }

        inspectionPanel.SetActive(false);
        if (cablePuzzlePanel != null) cablePuzzlePanel.SetActive(false);
        if (fusePuzzlePanel != null) fusePuzzlePanel.SetActive(false);

        if (cameraManager != null) cameraManager.SwitchToTrunkInspectionCamera();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitInspection()
    {
        if (trunkLock != null && isTrunkInspecting)
        {
            trunkLock.CloseTrunk();
        }

        // Resetea todos los estados de inspección
        isInspecting = false;
        isTrunkInspecting = false;

        // Asegúrate de que TODOS los paneles de inspección estén desactivados
        inspectionPanel.SetActive(false);
        if (trunkInspectionPanel != null) trunkInspectionPanel.SetActive(false);
        if (inspectionCanvas != null) inspectionCanvas.gameObject.SetActive(false);

        // ... y los paneles de puzzle
        if (cablePuzzlePanel != null) cablePuzzlePanel.SetActive(false);
        if (fusePuzzlePanel != null) fusePuzzlePanel.SetActive(false);

        // Restaura el control del jugador y la cámara
        if (playerMovement != null) playerMovement.enabled = true;
        if (player != null) player.GetComponent<Renderer>().enabled = true;
        if (cameraManager != null) cameraManager.SwitchToPlayerCamera();

        ShowInteractionPrompt(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // --- Métodos de UI reutilizables ---
    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }
        Invoke("HideMessage", 3f);
    }

    private void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
}