using UnityEngine;

public class CarIgnition : MonoBehaviour
{
    #region Variables

    [Header("Configuración de Llave")]
    public string requiredKey = "LlaveDelCoche";

    [Header("Componentes del Coche")]
    [SerializeField] private CarController carController;
    [SerializeField] private CarUIText carUIText;
    [SerializeField] private AudioClip ignitionSound;
    [SerializeField] private HotwirePromptUI hotwirePromptUI;

    private RadialInventoryManager inventory;
    private bool playerInRange = false;
    private bool isOn = false;
    private bool hotwireCompleted = false; // único flag de encendido forzado

    [Header("Sprite del jugador en el coche")]
    [SerializeField] private GameObject playerSpriteInCar;

    #endregion

    #region Unity Methods

    void Awake()
    {
        if (carController == null) carController = GetComponent<CarController>();
        if (carUIText == null) carUIText = GetComponent<CarUIText>();
    }

    void Start()
    {
        inventory = FindFirstObjectByType<RadialInventoryManager>();

        if (carController == null)
        {
            Debug.LogError("CarController no está asignado en CarIgnition.");
            return;
        }

        Invoke(nameof(InitializeCar), 0.1f);
    }

    private void Update()
    {
        if (playerInRange)
        {
            // Presiona E para intentar arrancar el coche
            if (Input.GetKeyDown(KeyCode.E))
            {
                ShowPlayerSprite(true); // Aparece el sprite al subirse al coche
                if (!isOn)
                    TryIgnite(); // Intenta arrancar el coche
            }

            // Presiona F para bajarse del coche, sin importar si arrancó
            if (Input.GetKeyDown(KeyCode.F))
            {
                TurnOff();               // Apaga el coche si estaba encendido
                ShowPlayerSprite(false); // Oculta el sprite siempre
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Oculta el panel de encendido forzado al salir del coche
            if (hotwirePromptUI != null)
                hotwirePromptUI.Hide();
        }
    }


    #endregion

    #region Funciones de Control del Coche

    private void ShowPlayerSprite(bool show)
    {
        if (playerSpriteInCar != null)
            playerSpriteInCar.SetActive(show);
    }

    public void EnableHotwireAccess()
    {
        hotwireCompleted = true; // el coche queda hackeado permanentemente
    }

    private void InitializeCar()
    {
        carController.StopDriving();
    }

    private void TryIgnite()
    {
        bool tieneLlave = string.IsNullOrEmpty(requiredKey) ||
                          (inventory != null && inventory.HasItem(requiredKey));

        if (!tieneLlave && !hotwireCompleted)
        {
            isOn = false;
            carController.ignitionAuthorized = false;

            string mensaje = $"Necesitas la llave \"{requiredKey}\" o encender el coche de otra manera.";

            if (hotwirePromptUI != null)
                hotwirePromptUI.Show(mensaje);
            else if (carUIText != null)
                carUIText.ShowText(mensaje);
            else
                Debug.Log(mensaje);

            return;
        }

        isOn = true;
        carController.ignitionAuthorized = true;
        carController.StartDriving();

        // Siempre activamos el sprite del jugador al encender el coche
        ShowPlayerSprite(true);

        if (ignitionSound != null)
            AudioSource.PlayClipAtPoint(ignitionSound, transform.position);

        Debug.Log("?? Coche encendido correctamente");
    }


    private void TurnOff()
    {
        isOn = false;
        carController.ignitionAuthorized = false;
        carController.StopDriving();

        // Asegura que se revalida siempre que intentes arrancar otra vez
        Debug.Log("?? Coche apagado.");
    }

    #endregion
}
