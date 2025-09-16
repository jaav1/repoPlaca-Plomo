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
        // Lógica para salir de la inspección del maletero
        if (isTrunkInspecting)
        {
            // Usa el nuevo método de salida unificado
            if (Input.GetKeyDown(KeyCode.R))
            {
                ExitInspection();
            }
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }
        // Lógica para salir del vehículo
        else if (isPlayerInside)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ExitVehicle();
            }
        }
        // Lógica para salir de la inspección interior
        else if (isInspecting)
        {
            // Usa el nuevo método de salida unificado
            if (Input.GetKeyDown(KeyCode.F))
            {
                ExitInspection();
            }
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }
        // Lógica para entrar a cualquier modo si el jugador está cerca
        else if (playerNearby)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                EnterVehicle();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                EnterInspectionMode();
            }
            // Lógica para entrar a la inspección del maletero con la tecla R
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (inventoryManager != null && trunkLock != null && inventoryManager.HasItem(trunkLock.keyID))
                {
                    // Solo desbloquea el maletero visualmente
                    trunkLock.UnlockTrunk();

                    // Activa el modo de inspección en este script
                    EnterTrunkInspectionMode();
                }
                else
                {
                    // Si no tiene la llave, muestra un mensaje
                    ShowMessage("Necesitas la llave del coche para abrir el maletero.");
                }
            }
        }
    }

    // --- Lógica del Raycast para los clics ---
    private void HandleMouseClick()
    {
        Camera activeInspectionCamera = null;
        if (cameraManager.inspectionCamera.gameObject.activeSelf)
        {
            activeInspectionCamera = cameraManager.inspectionCamera;
        }
        else if (cameraManager.trunkInspectionCamera.gameObject.activeSelf)
        {
            activeInspectionCamera = cameraManager.trunkInspectionCamera;
        }

        if (activeInspectionCamera != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                inspectionRawImage.rectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );

            Vector2 normalizedPoint = new Vector2(
                (localPoint.x + inspectionRawImage.rectTransform.rect.width / 2) / inspectionRawImage.rectTransform.rect.width,
                (localPoint.y + inspectionRawImage.rectTransform.rect.height / 2) / inspectionRawImage.rectTransform.rect.height
            );

            Ray ray = activeInspectionCamera.ViewportPointToRay(normalizedPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ClueObject clue = hit.collider.GetComponent<ClueObject>();

                if (clue != null)
                {
                    if (clue.OnClickAction != null && clue.OnClickAction.GetPersistentEventCount() > 0)
                    {
                        // Si hay un evento en el Inspector, ejecútalo.
                        clue.OnClickAction.Invoke();

                        // === AÑADE ESTAS LÍNEAS AQUÍ ===
                        // Ahora, también añade el ítem al inventario y destruye el objeto.
                        inventoryManager.AddItem(clue.clueID, "General");
                        Destroy(hit.collider.gameObject);
                        ShowMessage("Has encontrado " + clue.clueID + " y lo has agregado a tu inventario.");
                        // ==============================
                    }
                    else if (!string.IsNullOrEmpty(clue.clueID))
                    {
                        // Este bloque ya no es necesario si siempre configuras la llave.
                        // Pero puedes dejarlo como "fallback" si quieres.
                        inventoryManager.AddItem(clue.clueID, "General");
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

    // --- ¡Nuevos métodos para la inspección del maletero! ---
    public void EnterTrunkInspectionMode()
    {
        isTrunkInspecting = true;
        isInspecting = false;

        if (playerMovement != null) playerMovement.enabled = false;
        if (player != null) player.GetComponent<Renderer>().enabled = false;

        ShowInteractionPrompt(false);
        if (inspectionCanvas != null)
            inspectionCanvas.gameObject.SetActive(true);

        // ACTIVA el panel del maletero y DESACTIVA el del interior
        if (trunkInspectionPanel != null) trunkInspectionPanel.SetActive(true);
        inspectionPanel.SetActive(false);

        // Asegúrate de que los otros paneles de puzzle estén desactivados
        if (cablePuzzlePanel != null) cablePuzzlePanel.SetActive(false);
        if (fusePuzzlePanel != null) fusePuzzlePanel.SetActive(false);

        if (cameraManager != null) cameraManager.SwitchToTrunkInspectionCamera();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitInspection()
    {
        // Resetea todos los estados de inspección
        isInspecting = false;
        isTrunkInspecting = false;

        // Cierra el maletero si se está saliendo de su inspección
        if (trunkLock != null)
        {
            trunkLock.CloseTrunk();
        }

        // Asegúrate de que TODOS los paneles de inspección estén desactivados
        inspectionPanel.SetActive(false);
        if (trunkInspectionPanel != null) trunkInspectionPanel.SetActive(false);
        if (inspectionCanvas != null) inspectionCanvas.gameObject.SetActive(false);

        // Y los paneles de puzzle
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