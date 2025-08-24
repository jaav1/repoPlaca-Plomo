using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// Este script gestiona un sistema de inventario radial con múltiples páginas.
// Maneja la adición, eliminación y visualización de ítems, así como la interacción con la interfaz de usuario.
public class RadialInventoryManager : MonoBehaviour
{
    [Header("Inspección de Ítems")]
    public GameObject inspectionCamera; // La cámara que mostrará el objeto.
    public Transform inspectionPoint; // El punto en el mundo donde aparecerá el ítem.

    [Header("Referencias de UI y Player")]
    public Button inspectButton; // Referencia al nuevo botón de inspección.
    public PlayerMovement playerMovement; // Referencia al script PlayerMovement.
    public RadialInventoryManager radialInventoryManager; // Referencia a este script.

    // Variables internas para la lógica de inspección
    private GameObject inspectedItemInstance; // Almacenará la instancia del ítem que estamos inspeccionando.
    private bool isInspecting = false;

    private string lastSelectedItemId; // Almacena el nombre del último ítem seleccionado.

    // Variables de la interfaz de usuario que se asignan en el editor de Unity.
    [Header("UI Básica")]
    public GameObject radialPanel; // Panel principal del inventario radial.
    public GameObject slotPrefab; // Prefab para crear los espacios de inventario.
    public Transform slotContainer; // Contenedor que albergará los slots en la UI.

    // Variables para mostrar detalles de los ítems seleccionados.
    [Header("Ventana de Información")]
    public GameObject infoPanel; // Panel de información que muestra el nombre y la descripción del ítem.
    public TMP_Text itemNameText; // Texto para el nombre del ítem.
    public TMP_Text itemDescriptionText; // Texto para la descripción del ítem.

    // Sprites y prefabs de los ítems.
    [Header("Íconos por ítem")]
    public Sprite llaveSprite; // Icono para el ítem "Llave".
    public Sprite carkeySprite; // Icono para el ítem "Llave de coche".
    public Sprite carpetaSprite; // Icono para el ítem "Carpeta".

    // Prefabs de los ítems que se pueden soltar en el mundo del juego.
    [Header("Prefabs físicos para soltar")]
    public GameObject llavePrefab; // Prefab físico de la llave.
    public GameObject carkeyPrefab; // Prefab físico de la llave de coche.
    public GameObject carpetaPrefab; // Prefab físico de la carpeta.

    // Variables para el panel de confirmación de eliminación.
    [Header("Confirmación de eliminación")]
    public GameObject confirmDeletePanel; // Panel que pregunta al jugador si desea eliminar un ítem.
    public TMP_Text confirmDeleteText; // Texto de confirmación.

    // Texto que muestra el nombre de la página actual del inventario.
    [Header("UI de página")]
    public TMP_Text pageNameText;

    [Header("Inventario")]
    public int maxSlots = 7; // El número máximo de slots por página.

    // Sonido que se reproduce al eliminar un ítem.
    [Header("Sonido de eliminación")]
    public AudioClip rubberHoseDeleteSound;
    private AudioSource audioSource;

    // --- Variables internas para la lógica del inventario ---

    // Almacena los ítems por página usando un diccionario.
    private Dictionary<string, List<string>> pagedItems = new Dictionary<string, List<string>>();
    // Mantiene el orden de las páginas del inventario.
    private List<string> pageOrder = new List<string>();
    // Índice de la página actual.
    private int currentPageIndex = 0;
    // Almacena el ítem que se va a eliminar hasta que se confirme.
    private string pendingItemToRemove;

    // Diccionarios para almacenar descripciones, íconos y prefabs de los ítems.
    private Dictionary<string, string> itemDescriptions = new Dictionary<string, string>()
    {
        { "Llave", "Una llave de bronce. Parece encajar en una cerradura antigua." },
        { "Llave de coche", "Sin esta llave no podras conducir." },
        { "Carpeta", "Al parecer hay mucha información importante aquí." }
    };
    private Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> worldItemPrefabs = new Dictionary<string, GameObject>();

    // Propiedad que devuelve el nombre de la página actual.
    string CurrentPage => pageOrder.Count > 0 ? pageOrder[currentPageIndex] : "";

    // Posiciones predefinidas para los slots del inventario radial.
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

    // --- Métodos de ciclo de vida ---

    // Se llama al inicio para inicializar diccionarios y componentes de audio.
    void Start()
    {
        // Llena los diccionarios de ítems con los sprites y prefabs correspondientes.
        itemIcons.Add("Llave", llaveSprite);
        itemIcons.Add("Llave de coche", carkeySprite);
        itemIcons.Add("Carpeta", carpetaSprite);

        worldItemPrefabs.Add("Llave", llavePrefab);
        worldItemPrefabs.Add("Llave de coche", carkeyPrefab);
        worldItemPrefabs.Add("Carpeta", carpetaPrefab);

        // Agrega un componente AudioSource al objeto del juego.
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Desactiva el panel de confirmación de eliminación al inicio.
        confirmDeletePanel.SetActive(false);

        // Agrega el listener para el nuevo botón.
        inspectButton.onClick.AddListener(StartInspecting);
    }

    // Se ejecuta en cada fotograma para manejar la entrada del teclado que abre y cierra el inventario.
    void Update()
    {
        // Activa el panel del inventario al presionar Tab.
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            radialPanel.SetActive(true);
            infoPanel.SetActive(false);
            // Muestra y desbloquea el cursor para la interacción con la UI.
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            RefreshInventoryUI();
        }

        // Desactiva el panel del inventario al soltar Tab.
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            radialPanel.SetActive(false);
            infoPanel.SetActive(false);
            confirmDeletePanel.SetActive(false);
            // Oculta y bloquea el cursor para volver al control del juego.
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Maneja la salida del modo de inspección con la tecla Esc.
        if (isInspecting && Input.GetKeyDown(KeyCode.Tab))
        {
            StopInspecting();
        }

        // Permite cambiar de página si el panel radial está activo.
        if (radialPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E)) NextPage();
            if (Input.GetKeyDown(KeyCode.Q)) PreviousPage();
        }
    }

    // --- Métodos de gestión del inventario ---

    // Se llama cuando el jugador hace clic en el botón "Inspeccionar".
    public void StartInspecting()
    {
        // Asegurarse de que hay un ítem seleccionado para inspeccionar.
        if (string.IsNullOrEmpty(lastSelectedItemId))
        {
            Debug.LogWarning("No se puede inspeccionar. Ningún ítem seleccionado.");
            return;
        }

        // Asegurarse de que el prefab exista.
        if (!worldItemPrefabs.ContainsKey(lastSelectedItemId))
        {
            Debug.LogWarning("El prefab físico para el ítem '" + lastSelectedItemId + "' no se encontró.");
            return;
        }

        // Instancia el ítem y le añade el script de rotación.
        inspectedItemInstance = Instantiate(worldItemPrefabs[lastSelectedItemId], inspectionPoint.position, Quaternion.identity);
        inspectedItemInstance.AddComponent<ItemInspector>();

        // Desactiva los controles del jugador.
        if (playerMovement != null)
        {
            playerMovement.controlsEnabled = false;
        }

        // Oculta el panel de inventario y muestra el de inspección.
        radialPanel.SetActive(false);
        inspectionCamera.SetActive(true);
        infoPanel.SetActive(false);
        isInspecting = true;
    }

    // Se llama cuando el jugador termina de inspeccionar.
    public void StopInspecting()
    {
        // Destruye la instancia del ítem de inspección.
        if (inspectedItemInstance != null)
        {
            Destroy(inspectedItemInstance);
            inspectedItemInstance = null;
        }

        // Reactiva los controles del jugador.
        if (playerMovement != null)
        {
            playerMovement.controlsEnabled = true;
        }

        // Oculta la cámara de inspección y muestra el inventario de nuevo.
        inspectionCamera.SetActive(false);
        radialPanel.SetActive(true);
        infoPanel.SetActive(true);
        isInspecting = false;
    }


    // Intenta agregar un ítem al inventario, comprobando si hay espacio.
    public bool TryAddItem(string itemName, string pageName = "General")
    {
        // Crea una nueva página si no existe.
        if (!pagedItems.ContainsKey(pageName))
        {
            pagedItems.Add(pageName, new List<string>());
            pageOrder.Add(pageName);
        }

        var pageList = pagedItems[pageName];

        // Comprueba si la página está llena y, si es así, muestra un mensaje.
        if (pageList.Count >= maxSlots)
        {
            ShowInventoryFullMessage(itemName, pageName);
            return false;
        }

        // Agrega el ítem y actualiza la UI.
        pageList.Add(itemName);
        RefreshInventoryUI();
        return true;
    }

    // Un método alternativo para agregar un ítem, sin valor de retorno booleano.
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

    // Muestra un mensaje en el panel de información cuando el inventario está lleno.
    void ShowInventoryFullMessage(string itemName, string pageName)
    {
        infoPanel.SetActive(true);
        itemNameText.text = "Inventario lleno";
        itemDescriptionText.text = $"No puedes guardar \"{itemName}\". La pagina \"{pageName}\" del inventario está al máximo ({maxSlots}).";
        Debug.Log($"Inventario lleno en página {pageName}. No se puede agregar: {itemName}");
    }

    // Elimina un ítem del inventario y lo suelta en el mundo del juego.
    public void RemoveItem(string itemName)
    {
        // Busca el ítem y lo elimina de la lista.
        if (pagedItems.ContainsKey(CurrentPage) && pagedItems[CurrentPage].Contains(itemName))
        {
            pagedItems[CurrentPage].Remove(itemName);
            RefreshInventoryUI();
            Debug.Log("Objeto eliminado: " + itemName);

            if (rubberHoseDeleteSound != null)
                audioSource.PlayOneShot(rubberHoseDeleteSound);

            // Crea una instancia del prefab físico del ítem y lo lanza.
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

    // Confirma la eliminación de un ítem y cierra el panel de confirmación.
    public void ConfirmDeletion()
    {
        if (!string.IsNullOrEmpty(pendingItemToRemove))
            RemoveItem(pendingItemToRemove);

        confirmDeletePanel.SetActive(false);
        pendingItemToRemove = null;
    }

    // Cancela la eliminación y cierra el panel de confirmación.
    public void CancelDeletion()
    {
        confirmDeletePanel.SetActive(false);
        pendingItemToRemove = null;
    }

    // Actualiza la interfaz de usuario del inventario, creando los slots y asignando los íconos.
    void RefreshInventoryUI()
    {
        // Destruye los slots existentes antes de recrearlos.
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        if (!pagedItems.ContainsKey(CurrentPage)) return;

        lastSelectedItemId = null;

        List<string> items = pagedItems[CurrentPage];
        if (items.Count == 0) return;

        // Actualiza el texto con el nombre de la página.
        if (pageNameText != null)
            pageNameText.text = CurrentPage;

        // Instancia los prefabs de los slots y los configura con los datos de los ítems.
        for (int i = 0; i < items.Count && i < slotPositions.Length; i++)
        {
            string itemName = items[i];
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.name = itemName;

            // Asigna el ícono del ítem.
            Image icon = slot.GetComponentInChildren<Image>();
            if (itemIcons.ContainsKey(itemName))
                icon.sprite = itemIcons[itemName];

            // Posiciona el slot en la UI.
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = slotPositions[i];

            int index = i;
            // Configura los eventos de clic para seleccionar un ítem.
            slot.GetComponent<Button>().onClick.AddListener(() => SelectItem(index));

            // Configura el evento de clic derecho para la eliminación.
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

    // Muestra la información de un ítem seleccionado en el panel de información.
    void SelectItem(int index)
    {
        if (!pagedItems.ContainsKey(CurrentPage)) return;

        List<string> items = pagedItems[CurrentPage];
        if (index < 0 || index >= items.Count) return;

        string selected = items[index];
        lastSelectedItemId = selected;
        infoPanel.SetActive(true);
        itemNameText.text = selected;

        if (itemDescriptions.ContainsKey(selected))
            itemDescriptionText.text = itemDescriptions[selected];
        else
            itemDescriptionText.text = "Sin descripción disponible.";
    }

    // Cambia a la siguiente página del inventario.
    public void NextPage()
    {
        if (pageOrder.Count == 0) return;
        currentPageIndex = (currentPageIndex + 1) % pageOrder.Count;
        RefreshInventoryUI();
    }

    // Cambia a la página anterior del inventario.
    public void PreviousPage()
    {
        if (pageOrder.Count == 0) return;
        currentPageIndex = (currentPageIndex - 1 + pageOrder.Count) % pageOrder.Count;
        RefreshInventoryUI();
    }

    // Comprueba si un ítem específico está en el inventario.
    public bool HasItem(string itemName)
    {
        if (!pagedItems.ContainsKey(CurrentPage))
            return false;

        foreach (var list in pagedItems.Values)
            if (list.Contains(itemName))
                return true;

        return false;
    }
}