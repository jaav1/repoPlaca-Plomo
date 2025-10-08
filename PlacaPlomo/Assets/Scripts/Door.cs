using UnityEngine;

public class Door : MonoBehaviour
{
    public DoorUIText doorUIText;
    public string requiredKey = "LlavePrincipal";
    public bool isOpen = false;
    public Transform puertaVisual;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    [Header("Mission Logic")]
    [Tooltip("Flag de Misión que otorga acceso libre a esta puerta (ej: Acceso_Trasero).")]
    public string requiredMissionFlag = ""; // DEJAR VACÍO POR DEFECTO

    public AudioClip openDoor;
    public AudioClip closeDoor;

    // Referencia al MissionManager
    private MissionManager missionManager;
    private RadialInventoryManager inventory;
    private bool playerInRange = false;

    void Start()
    {
        inventory = FindFirstObjectByType<RadialInventoryManager>();
        missionManager = MissionManager.I;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (isOpen)
                CloseDoor();
            else
                TryOpenWithMissionLogic();
        }
    }

    void TryOpenWithMissionLogic()
    {
        // 1. PRIMERO: Verificar si el flag de la misión da acceso (Ruta M2-03A)
        if (!string.IsNullOrEmpty(requiredMissionFlag) && missionManager != null && missionManager.HasFlag(requiredMissionFlag))
        {
            // El flag de la misión está puesto, el acceso es libre.
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(OpenRoutine());
            return;
        }

        // 2. SEGUNDO: Si no hay flag, o si el flag no está puesto, usar la lógica normal de llave (Ruta M2-03B)
        if (string.IsNullOrEmpty(requiredKey))
        {
            // Es una puerta sin llave, acceso libre.
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(OpenRoutine());
            return;
        }

        if (inventory != null && inventory.HasItem(requiredKey))
        {
            // El jugador tiene la llave requerida.
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(OpenRoutine());
        }
        else
        {
            // Puerta cerrada por llave.
            if (doorUIText != null)
                doorUIText.ShowText($"Necesitas la llave \"{requiredKey}\".");
            else
                Debug.Log("Puerta cerrada: necesitas la llave \"" + requiredKey + "\".");
        }
    }

    void CloseDoor()
    {
        isOpen = false;
        StopAllCoroutines();
        StartCoroutine(CloseRoutine());
    }

    System.Collections.IEnumerator OpenRoutine()
    {
        Quaternion rotInicial = puertaVisual.localRotation;
        Quaternion rotFinal = Quaternion.Euler(0, openAngle, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;
            puertaVisual.localRotation = Quaternion.Slerp(rotInicial, rotFinal, t);
            yield return null;
        }

        // Reproduce el sonido de abrir la puerta una vez que la animación de apertura ha terminado.
        if (openDoor != null)
        {
            AudioSource.PlayClipAtPoint(openDoor, transform.position);
        }
    }

    System.Collections.IEnumerator CloseRoutine()
    {
        Quaternion rotInicial = puertaVisual.localRotation;
        Quaternion rotFinal = Quaternion.Euler(0, 0, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;
            puertaVisual.localRotation = Quaternion.Slerp(rotInicial, rotFinal, t);
            yield return null;
        }

        // Reproduce el sonido de cerrar la puerta una vez que la animación de cierre ha terminado.
        if (closeDoor != null)
        {
            AudioSource.PlayClipAtPoint(closeDoor, transform.position);
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
            playerInRange = false;
    }
}