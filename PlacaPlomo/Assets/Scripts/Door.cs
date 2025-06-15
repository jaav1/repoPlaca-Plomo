using UnityEngine;

public class Door : MonoBehaviour
{
    public DoorUIText doorUIText;
    public string requiredKey = "LlavePrincipal";
    public bool isOpen = false;
    public Transform puertaVisual;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    private RadialInventoryManager inventory;
    private bool playerInRange = false;

    void Start()
    {
        inventory = FindFirstObjectByType<RadialInventoryManager>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (isOpen)
                CloseDoor();
            else
                TryOpen();
        }
    }

    void TryOpen()
    {
        if (string.IsNullOrEmpty(requiredKey))
        {
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(OpenRoutine());
            return;
        }

        if (inventory != null && inventory.HasItem(requiredKey))
        {
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(OpenRoutine());
        }
        else
        {
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