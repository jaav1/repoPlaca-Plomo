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

    // Método ahora público para ser llamado por VehicleInteraction
    public void TryIgnite()
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

        ShowPlayerSprite(true);

        if (ignitionSound != null)
            AudioSource.PlayClipAtPoint(ignitionSound, transform.position);

        Debug.Log("?? Coche encendido correctamente");
    }

    // Método ahora público para ser llamado por VehicleInteraction
    public void TurnOff()
    {
        isOn = false;
        carController.ignitionAuthorized = false;
        carController.StopDriving();

        Debug.Log("?? Coche apagado.");
    }

    #endregion
}
