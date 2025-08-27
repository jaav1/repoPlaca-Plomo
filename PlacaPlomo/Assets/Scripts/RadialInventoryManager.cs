using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

// Este script gestiona un sistema de inventario radial con múltiples páginas.
// Maneja la adición, eliminación y visualización de ítems, así como la interacción con la interfaz de usuario.
public class RadialInventoryManager : MonoBehaviour
{
    // =================================================================================
    // REFERENCIAS PÚBLICAS Y ENCABEZADOS
    // =================================================================================

    [Header("Sistemas Externos")]
    public PhotoManager photoManager;

    [Header("UI General")]
    public GameObject radialPanel;
    public GameObject slotPrefab;
    public Transform slotContainer;
    public GameObject infoPanel;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public GameObject confirmDeletePanel;
    public TMP_Text confirmDeleteText;
    public TMP_Text pageNameText;
    public Button inspectButton;

    [Header("UI de Cámara Fotográfica")]
    public GameObject photoPreviewPanel;
    public Image photoPreviewImage;
    public TMP_Text instructionText;
    public Button saveButton;
    public Button discardButton;

    [Header("Inspección de Ítems")]
    public GameObject inspectionCamera;
    public Transform inspectionPoint;

    [Header("Referencias de Jugador y Audio")]
    public PlayerMovement playerMovement;
    public AudioSource audioSource;
    public AudioClip rubberHoseDeleteSound;
    public AudioClip cameraShutterSound;

    [Header("Datos de Ítems y Páginas")]
    public int maxSlots = 7;
    public Sprite llaveSprite;
    public Sprite carkeySprite;
    public Sprite carpetaSprite;
    // Falta asignar el sprite de la camara y la carpeta de fotos.
    public Sprite camaraSprite;
    public Sprite fotoSprite;
    public GameObject llavePrefab;
    public GameObject carkeyPrefab;
    public GameObject carpetaPrefab;
    // Falta asignar el prefab de la camara.
    public GameObject camaraPrefab;

    // =================================================================================
    // VARIABLES INTERNAS Y PRIVADAS
    // =================================================================================

    private bool isInspecting = false;
    private bool isCameraActive = false;
    private string lastSelectedItemId;
    private GameObject inspectedItemInstance;
    private string pendingItemToRemove;

    private Dictionary<string, List<string>> pagedItems = new Dictionary<string, List<string>>();
    private List<string> pageOrder = new List<string>();
    private int currentPageIndex = 0;

    private Dictionary<string, string> itemDescriptions = new Dictionary<string, string>();
    private Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> worldItemPrefabs = new Dictionary<string, GameObject>();

    private Vector2[] slotPositions = new Vector2[]
    {
        new Vector2(-50, 91),
        new Vector2(50, 91),
        new Vector2(105, 18),
        new Vector2(99, -60),
        new Vector2(0, -100),
        new Vector2(-99, -60),
        new Vector2(-105, 18)
    };

    // Propiedad que devuelve el nombre de la página actual.
    string CurrentPage => pageOrder.Count > 0 ? pageOrder[currentPageIndex] : "";

    // =================================================================================
    // MÉTODOS DE CICLO DE VIDA DE UNITY
    // =================================================================================

    void Start()
    {
        InitializeItemData();
        confirmDeletePanel.SetActive(false);
        if (inspectButton != null)
        {
            inspectButton.onClick.AddListener(HandleButtonClick);
        }

        // Asignar listeners a los nuevos botones de la cámara
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SavePhoto);
        }
        if (discardButton != null)
        {
            discardButton.onClick.AddListener(DiscardPhoto);
        }
    }

    void Update()
    {
        HandleInput();
    }

    // =================================================================================
    // LÓGICA PRINCIPAL DE LA CÁMARA
    // =================================================================================

    public void StartCameraMode()
    {
        radialPanel.SetActive(false);
        infoPanel.SetActive(false);
        if (playerMovement != null)
        {
            playerMovement.controlsEnabled = false;
        }
        instructionText.gameObject.SetActive(true);
        isCameraActive = true;
    }

    private IEnumerator TakeScreenshotAndPreview()
    {
        instructionText.gameObject.SetActive(false);

        if (cameraShutterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cameraShutterSound);
        }

        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        Sprite photoSprite = Sprite.Create(screenshot, new Rect(0, 0, screenshot.width, screenshot.height), new Vector2(0.5f, 0.5f));

        photoPreviewImage.sprite = photoSprite;
        photoPreviewPanel.SetActive(true);

        saveButton.gameObject.SetActive(true);
        discardButton.gameObject.SetActive(true);

        isCameraActive = false;
    }

    public void SavePhoto()
    {
        Sprite photo = photoPreviewImage.sprite;
        if (photo == null || photoManager == null) return;

        // Llama al PhotoManager para guardar la foto en el disco.
        string photoFileName = photoManager.SavePhoto(photo);

        // Usa el nombre del archivo como identificador del ítem.
        AddItem(photoFileName, "Fotos");

        // En el diccionario de íconos, usa el nombre de archivo como clave y el Sprite como valor.
        // Esto es temporal; en el siguiente paso lo cargaremos al iniciar.
        itemIcons[photoFileName] = photo;

        // Oculta el panel de previsualización.
        photoPreviewPanel.SetActive(false);

        // Vuelve al inventario radial.
        radialPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Reactiva los controles del jugador.
        playerMovement.controlsEnabled = true;

        // Limpia la imagen de la previsualización para el siguiente uso.
        photoPreviewImage.sprite = null;
    }

    public void DiscardPhoto()
    {
        photoPreviewPanel.SetActive(false);
        radialPanel.SetActive(true);
        isCameraActive = true;
        instructionText.gameObject.SetActive(true);
        photoPreviewImage.sprite = null;
    }

    // =================================================================================
    // LÓGICA DE INSPECCIÓN DE ÍTEMS
    // =================================================================================

    public void HandleButtonClick()
    {
        if (lastSelectedItemId == "Camara Fotografica")
        {
            StartCameraMode();
        }
        else
        {
            StartInspecting();
        }
    }

    public void StartInspecting()
    {
        if (string.IsNullOrEmpty(lastSelectedItemId) || !worldItemPrefabs.ContainsKey(lastSelectedItemId))
        {
            Debug.LogWarning("No se puede inspeccionar. Ítem no válido o prefab no encontrado.");
            return;
        }

        inspectedItemInstance = Instantiate(worldItemPrefabs[lastSelectedItemId], inspectionPoint.position, Quaternion.identity);
        inspectedItemInstance.AddComponent<ItemInspector>();
        if (playerMovement != null)
        {
            playerMovement.controlsEnabled = false;
        }

        radialPanel.SetActive(false);
        inspectionCamera.SetActive(true);
        infoPanel.SetActive(false);
        isInspecting = true;
    }

    public void StopInspecting()
    {
        if (inspectedItemInstance != null)
        {
            Destroy(inspectedItemInstance);
            inspectedItemInstance = null;
        }
        if (playerMovement != null)
        {
            playerMovement.controlsEnabled = true;
        }

        inspectionCamera.SetActive(false);
        radialPanel.SetActive(true);
        infoPanel.SetActive(true);
        isInspecting = false;
    }

    // =================================================================================
    // GESTIÓN DE INVENTARIO Y UI
    // =================================================================================

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !isCameraActive && !photoPreviewPanel.activeSelf)
        {
            radialPanel.SetActive(true);
            infoPanel.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            RefreshInventoryUI();
        }

        if (Input.GetKeyUp(KeyCode.Tab) && !isInspecting && !isCameraActive && !photoPreviewPanel.activeSelf)
        {
            radialPanel.SetActive(false);
            infoPanel.SetActive(false);
            confirmDeletePanel.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (radialPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E)) NextPage();
            if (Input.GetKeyDown(KeyCode.Q)) PreviousPage();
        }

        if (isInspecting && Input.GetKeyDown(KeyCode.Tab))
        {
            StopInspecting();
        }

        if (isCameraActive && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(TakeScreenshotAndPreview());
        }

        if (photoPreviewPanel.activeSelf && Input.GetKeyDown(KeyCode.Tab))
        {
            DiscardPhoto();
        }
    }

    private void InitializeItemData()
    {
        itemIcons.Add("Llave", llaveSprite);
        itemIcons.Add("Llave de coche", carkeySprite);
        itemIcons.Add("Carpeta", carpetaSprite);
        itemIcons.Add("Camara Fotografica", camaraSprite);

        itemDescriptions.Add("Llave", "Una llave de bronce. Parece encajar en una cerradura antigua.");
        itemDescriptions.Add("Llave de coche", "Sin esta llave no podras conducir.");
        itemDescriptions.Add("Carpeta", "Al parecer hay mucha información importante aquí.");
        itemDescriptions.Add("Camara Fotografica", "Una vieja cámara que puede capturar imágenes.");

        worldItemPrefabs.Add("Llave", llavePrefab);
        worldItemPrefabs.Add("Llave de coche", carkeyPrefab);
        worldItemPrefabs.Add("Carpeta", carpetaPrefab);
        worldItemPrefabs.Add("Camara Fotografica", camaraPrefab);

        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public bool TryAddItem(string itemName, string pageName = "General")
    {
        if (!pagedItems.ContainsKey(pageName))
        {
            pagedItems.Add(pageName, new List<string>());
            pageOrder.Add(pageName);
        }

        var pageList = pagedItems[pageName];

        if (pageList.Count >= maxSlots)
        {
            ShowInventoryFullMessage(itemName, pageName);
            return false;
        }

        pageList.Add(itemName);
        RefreshInventoryUI();
        return true;
    }

    public void AddItem(string itemName, string pageName = "General")
    {
        if (!pagedItems.ContainsKey(pageName))
        {
            pagedItems.Add(pageName, new List<string>());
            pageOrder.Add(pageName);
        }

        var pageList = pagedItems[pageName];

        if (pageList.Count >= maxSlots)
        {
            ShowInventoryFullMessage(itemName, pageName);
            return;
        }

        pageList.Add(itemName);
        RefreshInventoryUI();
    }

    void ShowInventoryFullMessage(string itemName, string pageName)
    {
        infoPanel.SetActive(true);
        itemNameText.text = "Inventario lleno";
        itemDescriptionText.text = $"No puedes guardar \"{itemName}\". La pagina \"{pageName}\" del inventario está al máximo ({maxSlots}).";
        Debug.Log($"Inventario lleno en página {pageName}. No se puede agregar: {itemName}");
    }

    public void RemoveItem(string itemName)
    {
        if (pagedItems.ContainsKey(CurrentPage) && pagedItems[CurrentPage].Contains(itemName))
        {
            pagedItems[CurrentPage].Remove(itemName);
            RefreshInventoryUI();
            Debug.Log("Objeto eliminado: " + itemName);

            if (rubberHoseDeleteSound != null)
                audioSource.PlayOneShot(rubberHoseDeleteSound);

            if (worldItemPrefabs.ContainsKey(itemName))
            {
                Vector3 dropPos = Camera.main.transform.position
                               + Camera.main.transform.forward * 2.0f;
                dropPos.y = 0.5f;
                GameObject dropped = Instantiate(worldItemPrefabs[itemName], dropPos, Quaternion.identity);

                Rigidbody rb = dropped.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    Vector3 throwDir = Camera.main.transform.forward + Vector3.up * 0.3f;
                    rb.AddForce(throwDir.normalized * 4f, ForceMode.Impulse);
                }
            }
            else
            {
                Debug.LogWarning("No hay prefab asignado para: " + itemName);
            }
        }
    }

    public void ConfirmDeletion()
    {
        if (!string.IsNullOrEmpty(pendingItemToRemove))
            RemoveItem(pendingItemToRemove);
        confirmDeletePanel.SetActive(false);
        pendingItemToRemove = null;
    }

    public void CancelDeletion()
    {
        confirmDeletePanel.SetActive(false);
        pendingItemToRemove = null;
    }

    void RefreshInventoryUI()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        if (!pagedItems.ContainsKey(CurrentPage)) return;
        lastSelectedItemId = null;
        List<string> items = pagedItems[CurrentPage];
        if (items.Count == 0) return;
        if (pageNameText != null)
            pageNameText.text = CurrentPage;
        for (int i = 0; i < items.Count && i < slotPositions.Length; i++)
        {
            string itemName = items[i];
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.name = itemName;
            Image icon = slot.GetComponentInChildren<Image>();
            if (itemIcons.ContainsKey(itemName))
                icon.sprite = itemIcons[itemName];
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = slotPositions[i];
            int index = i;
            slot.GetComponent<Button>().onClick.AddListener(() => SelectItem(index));
            EventTrigger trigger = slot.AddComponent<EventTrigger>();
            EventTrigger.Entry rightClick = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            rightClick.callback.AddListener((data) =>
            {
                PointerEventData ped = (PointerEventData)data;
                if (ped.button == PointerEventData.InputButton.Right)
                {
                    pendingItemToRemove = itemName;
                    confirmDeleteText.text = $"¿Deseas eliminar \"{itemName}\"?";
                    confirmDeletePanel.SetActive(true);
                }
            });
            trigger.triggers.Add(rightClick);
        }
    }

    void SelectItem(int index)
    {
        if (!pagedItems.ContainsKey(CurrentPage)) return;
        List<string> items = pagedItems[CurrentPage];
        if (index < 0 || index >= items.Count) return;
        string selected = items[index];
        lastSelectedItemId = selected;
        infoPanel.SetActive(true);
        itemNameText.text = selected;
        if (selected == "Camara Fotografica")
        {
            inspectButton.gameObject.SetActive(true);
            inspectButton.GetComponentInChildren<TMP_Text>().text = "Usar";
        }
        else
        {
            inspectButton.gameObject.SetActive(true);
            inspectButton.GetComponentInChildren<TMP_Text>().text = "Inspeccionar";
        }
        if (itemDescriptions.ContainsKey(selected))
            itemDescriptionText.text = itemDescriptions[selected];
        else
            itemDescriptionText.text = "Sin descripción disponible.";
    }

    public void NextPage()
    {
        if (pageOrder.Count == 0) return;
        currentPageIndex = (currentPageIndex + 1) % pageOrder.Count;
        RefreshInventoryUI();
    }

    public void PreviousPage()
    {
        if (pageOrder.Count == 0) return;
        currentPageIndex = (currentPageIndex - 1 + pageOrder.Count) % pageOrder.Count;
        RefreshInventoryUI();
    }

    public bool HasItem(string itemName)
    {
        foreach (var list in pagedItems.Values)
            if (list.Contains(itemName))
                return true;
        return false;
    }
}