using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleInteraction : MonoBehaviour
{
    // === REFERENCIAS EN EL INSPECTOR ===
    [Header("Referencias de Cámaras y Puntos")]
    public Camera playerCamera;
    public Camera carCamera;
    public Camera inspectionCamera; // Nueva cámara para inspección
    public Transform entryPoint;
    public Transform interiorCameraPoint; // Punto central para la cámara de inspección

    [Header("Referencias de UI")]
    public TMP_Text interactionText;
    public GameObject interactionPanel; // Panel para el texto y las opciones (Presiona E/F)
    public GameObject inspectionPanel; // El panel con la RawImage de la cámara de inspección
    public Canvas inspectionCanvas; // Referencia al Canvas de inspección**

    public GameObject cablePuzzlePanel;
    public GameObject fusePuzzlePanel;

    [Header("Referencias de Otros Scripts")]
    public CarIgnition carIgnition;
    private CarController carController;
    private GameObject player;
    private PlayerMovement playerMovement;

    // Referencaia para controlar el UI de encendido forzado
    [SerializeField] private HotwirePromptUI hotwirePromptUI;

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
        // Asegurarse de que los paneles están desactivados al inicio
        interactionPanel.SetActive(false);
        inspectionPanel.SetActive(false);
        if (inspectionCanvas != null)
            inspectionCanvas.gameObject.SetActive(false);

        // Desactiva el UI de encendido forzado al inicio
        if (hotwirePromptUI != null) hotwirePromptUI.Hide();
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerMovement = player.GetComponent<PlayerMovement>();
            playerNearby = true;
            ShowInteractionPrompt(true);
            Debug.Log("Jugador detectado. Player reference: " + (player != null));
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

        if (player != null) player.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = false;

        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(true);
        inspectionCamera.gameObject.SetActive(false);

        ShowInteractionPrompt(false);
        carIgnition.TryIgnite();
    }

    public void ExitVehicle()
    {
        isPlayerInside = false;

        if (player != null)
        {
            player.transform.position = entryPoint.position;
            player.SetActive(true);
        }
        if (playerMovement != null) playerMovement.enabled = true;

        carCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
        inspectionCamera.gameObject.SetActive(false);

        // Oculta todos los paneles de UI relacionados con el coche
        interactionPanel.SetActive(false);
        if (hotwirePromptUI != null) hotwirePromptUI.Hide();

        // **¡NUEVAS LÍNEAS!** Oculta los paneles de mini-puzzles
        if (cablePuzzlePanel != null) cablePuzzlePanel.SetActive(false);
        if (fusePuzzlePanel != null) fusePuzzlePanel.SetActive(false);

        carIgnition.TurnOff();
    }

    private void EnterInspectionMode()
    {
        isInspecting = true;

        if (player != null) player.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = false;

        ShowInteractionPrompt(false);
        if (inspectionCanvas != null)
            inspectionCanvas.gameObject.SetActive(true);

        inspectionPanel.SetActive(true);

        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(false);
        inspectionCamera.gameObject.SetActive(true);

        inspectionCamera.transform.position = interiorCameraPoint.position;
        inspectionCamera.transform.rotation = interiorCameraPoint.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitInspectionMode()
    {
        isInspecting = false;
        if (inspectionCanvas != null)
            inspectionCanvas.gameObject.SetActive(false);

        if (player != null) player.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = true;

        playerCamera.gameObject.SetActive(true);
        carCamera.gameObject.SetActive(false);
        inspectionCamera.gameObject.SetActive(false);

        ShowInteractionPrompt(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}