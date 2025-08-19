using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public Camera carCamera;
    public Transform entryPoint;
    public TMP_Text interactionText;

    private CarController carController;
    private GameObject player;
    private PlayerMovement playerMovement;
    private CameraFollow cameraFollow;

    private bool playerNearby = false;
    private bool isPlayerInside = false;

    void Start()
    {
        carController = GetComponent<CarController>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        interactionText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && !isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            EnterVehicle();
        }
        else if (isPlayerInside && Input.GetKeyDown(KeyCode.F))
        {
            ExitVehicle();
        }
    }

    void EnterVehicle()
    {
        playerMovement.enabled = false;
        player.SetActive(false);
        carController.StartDriving();

        playerCamera.gameObject.SetActive(false);
        carCamera.gameObject.SetActive(true);

        interactionText.gameObject.SetActive(false);
        isPlayerInside = true;
    }

    void ExitVehicle()
    {
        player.transform.position = entryPoint.position;
        player.SetActive(true);
        playerMovement.enabled = true;
        carController.StopDriving();

        carCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        isPlayerInside = false;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerMovement = player.GetComponent<PlayerMovement>();
            playerNearby = true;
            interactionText.text = "Presiona E para entrar al coche";
            interactionText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            interactionText.gameObject.SetActive(false);
        }
    }
}