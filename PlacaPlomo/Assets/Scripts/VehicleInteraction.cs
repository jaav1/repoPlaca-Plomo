using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleInteraction : MonoBehaviour
{
    // === REFERENCIAS EN EL INSPECTOR ===
    [Header("Referencias de Cámaras y Puntos")]
    public Transform entryPoint;
    public Transform interiorCameraPoint; // Punto central para la cámara de inspección

    [Header("Referencias de UI")]
    public TMP_Text interactionText;
    public GameObject interactionPanel; // Panel para el texto y las opciones (Presiona E/F)
    public GameObject inspectionPanel; // El panel con la RawImage de la cámara de inspección
    public Canvas inspectionCanvas; // Referencia al Canvas de inspección**
    public RawImage inspectionRawImage;

    public GameObject cablePuzzlePanel;
    public GameObject fusePuzzlePanel;

    [Header("Referencias de Otros Scripts")]
    public CarIgnition carIgnition;
    private CarController carController;
    private GameObject player;
    private PlayerMovement playerMovement;

    public RadialInventoryManager inventoryManager;

    [Header("UI de Mensajes")]
    public GameObject messagePanel;
    public TMP_Text messageText;    

    // Referencaia para controlar el UI de encendido forzado
    [SerializeField] private HotwirePromptUI hotwirePromptUI;

    public CameraManager cameraManager;

    // === ESTADOS INTERNOS ===
    private bool playerNearby = false;
    private bool isPlayerInside = false;
    private bool isInspecting = false;

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
        if (isPlayerInside)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ExitVehicle();
            }
        }
        else if (isInspecting)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ExitInspectionMode();
            }

            if (Input.GetMouseButtonDown(0))
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

                Ray ray = cameraManager.inspectionCamera.ViewportPointToRay(normalizedPoint);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    ClueObject clue = hit.collider.GetComponent<ClueObject>();

                    if (clue != null)
                    {
                        // 1. Primero, verificamos si el ClueObject tiene una acción de UnityEvent asignada
                        if (clue.OnClickAction != null && clue.OnClickAction.GetPersistentEventCount() > 0)
                        {
                            // Si la tiene, la ejecutamos y salimos del método
                            clue.OnClickAction.Invoke();

                            // Muestra un mensaje de UI, por ejemplo:
                            messageText.text = "Haz interactuado con un objeto especial. Si tenías la llave, el maletero se abrirá.";
                            messagePanel.SetActive(true);
                            Invoke("HideMessage", 3f);
                        }
                        // 2. Si no tiene un UnityEvent, asumimos que es un ítem para recoger
                        else if (!string.IsNullOrEmpty(clue.clueID))
                        {
                            // Llama a tu función de inventario
                            inventoryManager.AddItem(clue.clueID, "General");

                            // Destruye el objeto para que no se pueda volver a recoger
                            Destroy(hit.collider.gameObject);

                            // Muestra el mensaje de confirmación en la UI
                            messageText.text = "Has encontrado " + clue.clueID + " y lo has agregado a tu inventario.";
                            messagePanel.SetActive(true);
                            Invoke("HideMessage", 3f);
                        }
                        else
                        {
                            // Si no tiene ni evento ni ID, muestra un mensaje genérico
                            messageText.text = "Parece que aqui no hay nada, sigue buscando.";
                            messagePanel.SetActive(true);
                            Invoke("HideMessage", 3f);
                        }
                    }
                }
            }
        }

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
        }
    }

    public void HideMessage()
    {
        messagePanel.SetActive(false);
    }

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
            interactionText.text = "Presiona E para conducir\nPresiona F para inspeccionar";
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

        // Llamamos al Administrador de Cámaras para hacer el cambio
        if (cameraManager != null) cameraManager.SwitchToCarCamera();

        ShowInteractionPrompt(false);
        carIgnition.TryIgnite();

        carIgnition.ShowPlayerSprite(true);
    }

    public void ExitVehicle()
    {
        isPlayerInside = false;

        if (player != null)
        {
            player.transform.position = entryPoint.position;
            if (playerMovement != null) playerMovement.enabled = true;
            if (player != null) player.GetComponent<Renderer>().enabled = true;
        }

        // Llamamos al Administrador de Cámaras para volver a la cámara del jugador
        if (cameraManager != null) cameraManager.SwitchToPlayerCamera();

        interactionPanel.SetActive(false);
        if (hotwirePromptUI != null) hotwirePromptUI.Hide();
        if (cablePuzzlePanel != null) cablePuzzlePanel.SetActive(false);
        if (fusePuzzlePanel != null) fusePuzzlePanel.SetActive(false);

        carIgnition.TurnOff();

        carIgnition.ShowPlayerSprite(false);

        // Congela el cursor y lo hace invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void EnterInspectionMode()
    {
        isInspecting = true;

        if (playerMovement != null) playerMovement.enabled = false;
        if (player != null) player.GetComponent<Renderer>().enabled = false;

        ShowInteractionPrompt(false);
        if (inspectionCanvas != null)
            inspectionCanvas.gameObject.SetActive(true);

        inspectionPanel.SetActive(true);

        // Llamamos al Administrador de Cámaras para hacer el cambio
        if (cameraManager != null) cameraManager.SwitchToInspectionCamera();

        // Resto de la lógica de inspección
        if (cameraManager != null)
        {
            cameraManager.inspectionCamera.transform.position = interiorCameraPoint.position;
            cameraManager.inspectionCamera.transform.rotation = interiorCameraPoint.rotation;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitInspectionMode()
    {
        isInspecting = false;
        if (inspectionCanvas != null)
            inspectionCanvas.gameObject.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = true;
        if (player != null) player.GetComponent<Renderer>().enabled = true;

        // Llamamos al Administrador de Cámaras para volver a la cámara del jugador
        if (cameraManager != null) cameraManager.SwitchToPlayerCamera();

        ShowInteractionPrompt(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}