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

    [Header("Inspección de Fotos")]
    public GameObject photoInspectionPanel;
    public Image inspectedPhotoImage;

    [Header("Sistemas Externos")]
    public PhotoManager photoManager;
    private SistemaGuardado sistemaGuardado;

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
    public Sprite camaraSprite;
    public Sprite fotoSprite;
    public GameObject llavePrefab;
    public GameObject carkeyPrefab;
    public GameObject carpetaPrefab;
    public GameObject camaraPrefab;

    // =================================================================================
    // VARIABLES INTERNAS Y PRIVADAS
    // =================================================================================

    private bool isInspecting = false;
    private bool isCameraActive = false;
    private string lastSelectedItemId;
    private GameObject inspectedItemInstance;
    private string pendingItemToRemove;

    // Aquí guardaremos los ítems. Las fotos se guardan en la misma estructura.
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

    string CurrentPage => pageOrder.Count > 0 ? pageOrder[currentPageIndex] : "";

    // =================================================================================
    // MÉTODOS DE CICLO DE VIDA DE UNITY
    // =================================================================================

        void Start()
        {
            InitializeItemData();
            sistemaGuardado = SistemaGuardado.instancia;
            confirmDeletePanel.SetActive(false);
            if (inspectButton != null) inspectButton.onClick.AddListener(HandleButtonClick);
            /*if (saveButton != null) saveButton.onClick.AddListener(SavePhoto);
            if (discardButton != null) discardButton.onClick.AddListener(DiscardPhoto);*/
            if (infoPanel != null) infoPanel.SetActive(false);
            if (radialPanel != null) radialPanel.SetActive(false);
        }

    void Update()
    {
        HandleInput();
    }

    // =================================================================================
    // LÓGICA PRINCIPAL DE LA CÁMARA
    // =================================================================================
    /*  
    public void StartCameraMode()
    {
        radialPanel.SetActive(false);
        infoPanel.SetActive(false);

        if (playerMovement != null) playerMovement.controlsEnabled = true;

        instructionText.gameObject.SetActive(true);
        isCameraActive = true;
    }*/

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
    /*
    public void SavePhoto()
    {
        Sprite photo = photoPreviewImage.sprite;
        if (photo == null || photoManager == null) return;

        // Llama al PhotoManager para guardar la foto en el disco.
        string photoFileName = photoManager.SavePhoto(photo);

        // Usa el nombre del archivo como identificador del ítem.
        AddItem(photoFileName, "Fotos");

        // Crea una descripción para la foto con un nombre más amigable.
        itemDescriptions[photoFileName] = "Foto capturada";

        // Asigna el sprite al ítem.
        itemIcons[photoFileName] = photo;

        photoPreviewPanel.SetActive(false);
        radialPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerMovement != null) playerMovement.controlsEnabled = false;

        // Aquí es donde sales del modo de cámara, por lo que isCameraActive debe ser false
        isCameraActive = false;
        photoPreviewImage.sprite = null;
    }
    */
    public void DiscardPhoto()
    {
        // Oculta el panel de previsualización y las instrucciones
        photoPreviewPanel.SetActive(false);
        instructionText.gameObject.SetActive(true);

        // Limpia el sprite de la foto.
        photoPreviewImage.sprite = null;

        // isCameraActive debe seguir siendo true, ya que estamos volviendo al modo de cámara.
        isCameraActive = true;
    }

    // =================================================================================
    // LÓGICA DE INSPECCIÓN DE ÍTEMS
    // =================================================================================

    public void HandleButtonClick()
    {
        if (lastSelectedItemId == "Camara Fotografica")
        {
            if (photoManager != null)
            {
                // Oculta solo los paneles del inventario
                radialPanel.SetActive(false);
                infoPanel.SetActive(false);

                // Llama al PhotoManager para que se encargue del resto
                photoManager.StartCameraView();
            }
        }
        else if (lastSelectedItemId.EndsWith(".png"))
        {
            StartPhotoInspecting();
        }
        else
        {
            StartInspecting();
        }
    }
    public void StartPhotoInspecting()
    {
        Sprite photo = photoManager.LoadPhoto(lastSelectedItemId);
        if (photo == null) return;
        inspectedPhotoImage.sprite = photo;
        photoInspectionPanel.SetActive(true);
        radialPanel.SetActive(false);
        infoPanel.SetActive(false);
        if (playerMovement != null) playerMovement.controlsEnabled = false;
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
        if (playerMovement != null) playerMovement.controlsEnabled = false;
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
        if (playerMovement != null) playerMovement.controlsEnabled = true;
        inspectionCamera.SetActive(false);
        radialPanel.SetActive(true);
        infoPanel.SetActive(true);
        isInspecting = false;
    }

    public void StopPhotoInspecting()
    {
        photoInspectionPanel.SetActive(false);
        inspectedPhotoImage.sprite = null;
        radialPanel.SetActive(true);
        infoPanel.SetActive(true);
        if (playerMovement != null) playerMovement.controlsEnabled = true;
    }

    // =================================================================================
    // GESTIÓN DE INVENTARIO Y UI
    // =================================================================================

    // Este método es llamado por el GameManager para obtener los datos del inventario para guardar.
    public List<InventarioPaginaDatos> GetInventoryDataForSave()
    {
        Debug.Log("[GUARDADO] Preparando datos del inventario para guardar...");
        List<InventarioPaginaDatos> paginasAGuardar = new List<InventarioPaginaDatos>();

        foreach (var page in pagedItems)
        {
            InventarioPaginaDatos paginaDatos = new InventarioPaginaDatos();
            paginaDatos.pageName = page.Key;
            paginaDatos.items = new List<string>();

            foreach (string item in page.Value)
            {
                // Solo guarda ítems que no sean fotos
                if (!item.EndsWith(".png"))
                {
                    paginaDatos.items.Add(item);
                }
            }
            if (paginaDatos.items.Count > 0)
            {
                paginasAGuardar.Add(paginaDatos);
            }
        }

        Debug.Log($"[GUARDADO] {paginasAGuardar.Count} páginas con ítems guardadas.");
        return paginasAGuardar;
    }

    public void SetInventoryDataFromLoad(List<InventarioPaginaDatos> savedData)
    {
        Debug.Log("[CARGADO] La información que se recibe del archivo es:");

        // Primero, carga los datos de los ítems predeterminados
        InitializeItemData();
        // Luego, limpia el inventario del juego (excepto las fotos)
        pagedItems.Clear();
        pageOrder.Clear();

        // Carga las fotos primero
        if (photoManager != null)
        {
            photoManager.LoadPhotosToInventory();
        }

        if (savedData != null)
        {
            foreach (var pagina in savedData)
            {
                if (!pagedItems.ContainsKey(pagina.pageName))
                {
                    pagedItems.Add(pagina.pageName, new List<string>());
                    pageOrder.Add(pagina.pageName);
                }
                foreach (string item in pagina.items)
                {
                    pagedItems[pagina.pageName].Add(item);
                }
            }
        }

        Debug.Log($"[CARGADO] Inventario restaurado con {pagedItems.Count} páginas y {itemIcons.Count} íconos.");
        RefreshInventoryUI();
    }

    // === MÉTODOS OBSOLETOS QUE YA NO SE USAN POR EL GAMEMANAGER ===

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!isCameraActive && !photoPreviewPanel.activeSelf && !photoInspectionPanel.activeSelf && !isInspecting)
            {
                if (playerMovement != null) playerMovement.controlsEnabled = false;
                radialPanel.SetActive(true);
                infoPanel.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                RefreshInventoryUI();
            }
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (!isInspecting && !isCameraActive && !photoPreviewPanel.activeSelf && !photoInspectionPanel.activeSelf)
            {
                radialPanel.SetActive(false);
                infoPanel.SetActive(false);
                confirmDeletePanel.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                if (playerMovement != null) playerMovement.controlsEnabled = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (photoPreviewPanel.activeSelf) DiscardPhoto();
            else if (isInspecting) StopInspecting();
            else if (photoInspectionPanel.activeSelf) StopPhotoInspecting();
        }
        if (radialPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E)) NextPage();
            if (Input.GetKeyDown(KeyCode.Q)) PreviousPage();
        }
        if (isCameraActive && Input.GetKeyDown(KeyCode.Space)) StartCoroutine(TakeScreenshotAndPreview());
    }

    private void InitializeItemData()
    {
        // Si los íconos ya han sido añadidos, salimos del método
        if (itemIcons.Count > 0)
        {
            Debug.LogWarning("InitializeItemData ya ha sido llamado. Saliendo.");
            return;
        }

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

    public void AddItem(string itemName, string pageName = "General")
    {
        string targetPageName = FindAvailablePage(pageName);
        if (targetPageName == null) targetPageName = CreateNewPage(pageName);
        if (targetPageName == null) return;
        var pageList = pagedItems[targetPageName];
        if (pageList.Contains(itemName)) return;
        if (pageList.Count < maxSlots) pageList.Add(itemName);
    }

    // método para encontrar una página disponible.
    private string FindAvailablePage(string categoryName)
    {
        foreach (var page in pagedItems)
        {
            // Si la página pertenece a la categoría y no está llena, la devolvemos.
            if (page.Key.StartsWith(categoryName) && page.Value.Count < maxSlots)
            {
                return page.Key;
            }
        }
        return null; // No se encontró ninguna página disponible.
    }

    // método para crear una nueva página.
    private string CreateNewPage(string categoryName)
    {
        char pageLetter = 'A';
        string newPageName = categoryName;

        while (pagedItems.ContainsKey(newPageName))
        {
            pageLetter++;
            if (pageLetter > 'Z')
            {
                Debug.LogError("Se ha alcanzado el límite de páginas con letras. No se puede crear más.");
                return null;
            }
            newPageName = categoryName + " " + pageLetter;
        }

        pagedItems.Add(newPageName, new List<string>());
        pageOrder.Add(newPageName); 

        return newPageName;
    }

    void ShowInventoryFullMessage(string itemName, string pageName)
    {
        infoPanel.SetActive(true);
        itemNameText.text = "Inventario lleno";
        itemDescriptionText.text = $"No puedes guardar \"{itemName}\". La pagina \"{pageName}\" del inventario está al máximo ({maxSlots}).";
    }

    public void RemoveItem(string itemName)
    {
        if (pagedItems.ContainsKey(CurrentPage) && pagedItems[CurrentPage].Contains(itemName))
        {
            if (itemName.EndsWith(".png"))
            {
                if (photoManager != null) photoManager.DeletePhoto(itemName);
            }
            else
            {
                if (worldItemPrefabs.ContainsKey(itemName))
                {
                    Vector3 dropPos = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;
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
            }
            pagedItems[CurrentPage].Remove(itemName);
            RefreshInventoryUI();
            if (rubberHoseDeleteSound != null) audioSource.PlayOneShot(rubberHoseDeleteSound);
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

    public void AddItemIcon(string itemName, Sprite icon)
    {
        if (!itemIcons.ContainsKey(itemName))
        {
            itemIcons.Add(itemName, icon);
        }
    }

    public void AddItemDescription(string itemName, string description)
    {
        if (!itemDescriptions.ContainsKey(itemName))
        {
            itemDescriptions.Add(itemName, description);
        }
    }

    void RefreshInventoryUI()
    {
        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        if (!pagedItems.ContainsKey(CurrentPage)) return;
        lastSelectedItemId = null;
        List<string> items = pagedItems[CurrentPage];
        if (items.Count == 0) return;
        if (pageNameText != null) pageNameText.text = CurrentPage;
        for (int i = 0; i < items.Count && i < slotPositions.Length; i++)
        {
            string itemName = items[i];
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.name = itemName;
            Image icon = slot.GetComponentInChildren<Image>();
            if (itemIcons.ContainsKey(itemName)) icon.sprite = itemIcons[itemName];
            else if (itemName.EndsWith(".png"))
            {
                Sprite photo = photoManager.LoadPhoto(itemName);
                if (photo != null) icon.sprite = photo;
            }
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = slotPositions[i];
            int index = i;
            slot.GetComponent<Button>().onClick.AddListener(() => SelectItem(index));
            EventTrigger trigger = slot.AddComponent<EventTrigger>();
            EventTrigger.Entry rightClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
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

        // Lógica para cambiar el texto del botón y la información.
        if (selected == "Camara Fotografica")
        {
            inspectButton.gameObject.SetActive(true);
            inspectButton.GetComponentInChildren<TMP_Text>().text = "Usar";
            itemNameText.text = selected;
            if (itemDescriptions.ContainsKey(selected))
                itemDescriptionText.text = itemDescriptions[selected];
            else
                itemDescriptionText.text = "Sin descripción disponible.";
        }
        else if (selected.EndsWith(".png"))
        {
            inspectButton.gameObject.SetActive(true);
            inspectButton.GetComponentInChildren<TMP_Text>().text = "Ver";
            // Muestra un nombre de visualización más limpio para las fotos.
            itemNameText.text = "Foto";
            itemDescriptionText.text = "Una foto capturada. Puedes verla con el botón 'Ver'.";
        }
        else
        {
            inspectButton.gameObject.SetActive(true);
            inspectButton.GetComponentInChildren<TMP_Text>().text = "Inspeccionar";
            itemNameText.text = selected;
            if (itemDescriptions.ContainsKey(selected))
                itemDescriptionText.text = itemDescriptions[selected];
            else
                itemDescriptionText.text = "Sin descripción disponible.";
        }
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