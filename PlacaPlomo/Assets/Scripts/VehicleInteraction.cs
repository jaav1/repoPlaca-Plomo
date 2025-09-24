using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VehicleInteraction : MonoBehaviour
{
    // === REFERENCIAS EN EL INSPECTOR ===
    [Header("Referencias de C�maras y Puntos")]
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
    private bool isTrunkInspecting = false; // �Estado nuevo!

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
        // === L�GICA DE MANEJO DE ENTRADAS DEL JUGADOR ===
        // Esta secci�n maneja todas las entradas de teclado y rat�n de manera independiente.

        // Manejar los clics del rat�n en modo de inspecci�n.
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }

        // Manejar la tecla 'F' para inspeccionar y salir de la inspecci�n.
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Si ya est�s inspeccionando, sal de la inspecci�n.
            if (isInspecting)
            {
                ExitInspection();
            }
            // Si no est�s inspeccionando y est�s cerca del veh�culo, entra en modo de inspecci�n interior.
            else if (playerNearby && !isTrunkInspecting)
            {
                EnterInspectionMode();
            }
        }

        // Manejar la tecla 'R' para abrir/cerrar el maletero.
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Si est�s inspeccionando el maletero, sal de la inspecci�n.
            if (isTrunkInspecting)
            {
                ExitInspection();
            }
            // Si no est�s inspeccionando el maletero y est�s cerca, intenta abrirlo.
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

        // Manejar la tecla 'E' para entrar al veh�culo.
        if (Input.GetKeyDown(KeyCode.E) && playerNearby)
        {
            EnterVehicle();
        }

        // Manejar la tecla 'Q' para salir del veh�culo.
        if (Input.GetKeyDown(KeyCode.Q) && isPlayerInside)
        {
            ExitVehicle();
        }

        // === L�GICA PARA MOSTRAR LA UI DE INTERACCI�N ===
        // Muestra la UI solo si el jugador est� cerca y no est� en ning�n modo especial.
        if (playerNearby && !isInspecting && !isTrunkInspecting && !isPlayerInside)
        {
            ShowInteractionPrompt(true);
        }
        else
        {
            ShowInteractionPrompt(false);
        }
    }

    // --- L�gica del Raycast para los clics ---
    public void HandleMouseClick()
    {
        Debug.Log("HandleMouseClick() ha sido llamado.");

        // Escudo para evitar clics fuera del modo de inspecci�n.
        if (!isInspecting && !isTrunkInspecting || !Cursor.visible)
        {
            Debug.Log("HandleMouseClick() - Saliendo. Estado de inspecci�n incorrecto o cursor invisible."); // <--- NUEVO LOG 2
            return;
        }

        Camera activeInspectionCamera = null;
        RawImage activeRawImage = null;

        // L�gica para el modo de inspecci�n INTERIOR.
        if (isInspecting)
        {
            activeInspectionCamera = cameraManager.inspectionCamera;
            activeRawImage = inspectionRawImage;
        }
        // L�gica para el modo de inspecci�n del MALETERO.
        else if (isTrunkInspecting)
        {
            activeInspectionCamera = cameraManager.trunkInspectionCamera;
            activeRawImage = trunkInspectionRawImage;
        }

        // Si no se asign� una c�mara o RawImage, no hacemos nada.
        if (activeInspectionCamera == null || activeRawImage == null)
        {
            Debug.LogWarning("HandleMouseClick() - Saliendo. La c�mara o la RawImage no est�n asignadas. Camera: " + activeInspectionCamera + ", RawImage: " + activeRawImage); // <--- NUEVO LOG 3
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
                    // Si hay un evento en el Inspector, ejec�talo.
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

                    // A�ADIDO: Notifica al MissionManager que un objeto ha sido recogido
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

    // --- M�todos de Interacci�n y Salida ---
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
        Debug.Log("�ExitVehicle() ha sido llamado!");

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

        // Aseg�rate de que los otros paneles de puzzle est�n desactivados
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

    // --- m�todos para la inspecci�n del maletero! ---
    public void EnterTrunkInspectionMode()
    {
        // Centraliza la llamada a UnlockTrunk aqu�
        if (trunkLock != null)
        {
            trunkLock.UnlockTrunk();
        }

        // Activa el estado de inspecci�n
        isTrunkInspecting = true;
        isInspecting = false;

        // Controla la UI y la c�mara
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

        // Resetea todos los estados de inspecci�n
        isInspecting = false;
        isTrunkInspecting = false;

        // Aseg�rate de que TODOS los paneles de inspecci�n est�n desactivados
        inspectionPanel.SetActive(false);
        if (trunkInspectionPanel != null) trunkInspectionPanel.SetActive(false);
        if (inspectionCanvas != null) inspectionCanvas.gameObject.SetActive(false);

        // ... y los paneles de puzzle
        if (cablePuzzlePanel != null) cablePuzzlePanel.SetActive(false);
        if (fusePuzzlePanel != null) fusePuzzlePanel.SetActive(false);

        // Restaura el control del jugador y la c�mara
        if (playerMovement != null) playerMovement.enabled = true;
        if (player != null) player.GetComponent<Renderer>().enabled = true;
        if (cameraManager != null) cameraManager.SwitchToPlayerCamera();

        ShowInteractionPrompt(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // --- M�todos de UI reutilizables ---
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