using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// Este script gestiona un sistema de inventario radial con m�ltiples p�ginas.
// Maneja la adici�n, eliminaci�n y visualizaci�n de �tems, as� como la interacci�n con la interfaz de usuario.
public class RadialInventoryManager : MonoBehaviour
{
    [Header("Inspecci�n de �tems")]
    public GameObject inspectionCamera; // La c�mara que mostrar� el objeto.
    public Transform inspectionPoint; // El punto en el mundo donde aparecer� el �tem.

    [Header("Referencias de UI y Player")]
    public Button inspectButton; // Referencia al nuevo bot�n de inspecci�n.
    public PlayerMovement playerMovement; // Referencia al script PlayerMovement.
    public RadialInventoryManager radialInventoryManager; // Referencia a este script.

    // Variables internas para la l�gica de inspecci�n
    private GameObject inspectedItemInstance; // Almacenar� la instancia del �tem que estamos inspeccionando.
    private bool isInspecting = false;

    private string lastSelectedItemId; // Almacena el nombre del �ltimo �tem seleccionado.

    // Variables de la interfaz de usuario que se asignan en el editor de Unity.
    [Header("UI B�sica")]
    public GameObject radialPanel; // Panel principal del inventario radial.
    public GameObject slotPrefab; // Prefab para crear los espacios de inventario.
    public Transform slotContainer; // Contenedor que albergar� los slots en la UI.

    // Variables para mostrar detalles de los �tems seleccionados.
    [Header("Ventana de Informaci�n")]
    public GameObject infoPanel; // Panel de informaci�n que muestra el nombre y la descripci�n del �tem.
    public TMP_Text itemNameText; // Texto para el nombre del �tem.
    public TMP_Text itemDescriptionText; // Texto para la descripci�n del �tem.

    // Sprites y prefabs de los �tems.
    [Header("�conos por �tem")]
    public Sprite llaveSprite; // Icono para el �tem "Llave".
    public Sprite carkeySprite; // Icono para el �tem "Llave de coche".
    public Sprite carpetaSprite; // Icono para el �tem "Carpeta".

    // Prefabs de los �tems que se pueden soltar en el mundo del juego.
    [Header("Prefabs f�sicos para soltar")]
    public GameObject llavePrefab; // Prefab f�sico de la llave.
    public GameObject carkeyPrefab; // Prefab f�sico de la llave de coche.
    public GameObject carpetaPrefab; // Prefab f�sico de la carpeta.

    // Variables para el panel de confirmaci�n de eliminaci�n.
    [Header("Confirmaci�n de eliminaci�n")]
    public GameObject confirmDeletePanel; // Panel que pregunta al jugador si desea eliminar un �tem.
    public TMP_Text confirmDeleteText; // Texto de confirmaci�n.

    // Texto que muestra el nombre de la p�gina actual del inventario.
    [Header("UI de p�gina")]
    public TMP_Text pageNameText;

    [Header("Inventario")]
    public int maxSlots = 7; // El n�mero m�ximo de slots por p�gina.

    // Sonido que se reproduce al eliminar un �tem.
    [Header("Sonido de eliminaci�n")]
    public AudioClip rubberHoseDeleteSound;
    private AudioSource audioSource;

    // --- Variables internas para la l�gica del inventario ---

    // Almacena los �tems por p�gina usando un diccionario.
    private Dictionary<string, List<string>> pagedItems = new Dictionary<string, List<string>>();
    // Mantiene el orden de las p�ginas del inventario.
    private List<string> pageOrder = new List<string>();
    // �ndice de la p�gina actual.
    private int currentPageIndex = 0;
    // Almacena el �tem que se va a eliminar hasta que se confirme.
    private string pendingItemToRemove;

    // Diccionarios para almacenar descripciones, �conos y prefabs de los �tems.
    private Dictionary<string, string> itemDescriptions = new Dictionary<string, string>()
    {
        { "Llave", "Una llave de bronce. Parece encajar en una cerradura antigua." },
        { "Llave de coche", "Sin esta llave no podras conducir." },
        { "Carpeta", "Al parecer hay mucha informaci�n importante aqu�." }
    };
    private Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> worldItemPrefabs = new Dictionary<string, GameObject>();

    // Propiedad que devuelve el nombre de la p�gina actual.
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

    // --- M�todos de ciclo de vida ---

    // Se llama al inicio para inicializar diccionarios y componentes de audio.
    void Start()
    {
        // Llena los diccionarios de �tems con los sprites y prefabs correspondientes.
        itemIcons.Add("Llave", llaveSprite);
        itemIcons.Add("Llave de coche", carkeySprite);
        itemIcons.Add("Carpeta", carpetaSprite);

        worldItemPrefabs.Add("Llave", llavePrefab);
        worldItemPrefabs.Add("Llave de coche", carkeyPrefab);
        worldItemPrefabs.Add("Carpeta", carpetaPrefab);

        // Agrega un componente AudioSource al objeto del juego.
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Desactiva el panel de confirmaci�n de eliminaci�n al inicio.
        confirmDeletePanel.SetActive(false);

        // Agrega el listener para el nuevo bot�n.
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
            // Muestra y desbloquea el cursor para la interacci�n con la UI.
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

        // Maneja la salida del modo de inspecci�n con la tecla Esc.
        if (isInspecting && Input.GetKeyDown(KeyCode.Tab))
        {
            StopInspecting();
        }

        // Permite cambiar de p�gina si el panel radial est� activo.
        if (radialPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E)) NextPage();
            if (Input.GetKeyDown(KeyCode.Q)) PreviousPage();
        }
    }

    // --- M�todos de gesti�n del inventario ---

    // Se llama cuando el jugador hace clic en el bot�n "Inspeccionar".
    public void StartInspecting()
    {
        // Asegurarse de que hay un �tem seleccionado para inspeccionar.
        if (string.IsNullOrEmpty(lastSelectedItemId))
        {
            Debug.LogWarning("No se puede inspeccionar. Ning�n �tem seleccionado.");
            return;
        }

        // Asegurarse de que el prefab exista.
        if (!worldItemPrefabs.ContainsKey(lastSelectedItemId))
        {
            Debug.LogWarning("El prefab f�sico para el �tem '" + lastSelectedItemId + "' no se encontr�.");
            return;
        }

        // Instancia el �tem y le a�ade el script de rotaci�n.
        inspectedItemInstance = Instantiate(worldItemPrefabs[lastSelectedItemId], inspectionPoint.position, Quaternion.identity);
        inspectedItemInstance.AddComponent<ItemInspector>();

        // Desactiva los controles del jugador.
        if (playerMovement != null)
        {
            playerMovement.controlsEnabled = false;
        }

        // Oculta el panel de inventario y muestra el de inspecci�n.
        radialPanel.SetActive(false);
        inspectionCamera.SetActive(true);
        infoPanel.SetActive(false);
        isInspecting = true;
    }

    // Se llama cuando el jugador termina de inspeccionar.
    public void StopInspecting()
    {
        // Destruye la instancia del �tem de inspecci�n.
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

        // Oculta la c�mara de inspecci�n y muestra el inventario de nuevo.
        inspectionCamera.SetActive(false);
        radialPanel.SetActive(true);
        infoPanel.SetActive(true);
        isInspecting = false;
    }


    // Intenta agregar un �tem al inventario, comprobando si hay espacio.
    public bool TryAddItem(string itemName, string pageName = "General")
    {
        // Crea una nueva p�gina si no existe.
        if (!pagedItems.ContainsKey(pageName))
        {
            pagedItems.Add(pageName, new List<string>());
            pageOrder.Add(pageName);
        }

        var pageList = pagedItems[pageName];

        // Comprueba si la p�gina est� llena y, si es as�, muestra un mensaje.
        if (pageList.Count >= maxSlots)
        {
            ShowInventoryFullMessage(itemName, pageName);
            return false;
        }

        // Agrega el �tem y actualiza la UI.
        pageList.Add(itemName);
        RefreshInventoryUI();
        return true;
    }

    // Un m�todo alternativo para agregar un �tem, sin valor de retorno booleano.
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

    // Muestra un mensaje en el panel de informaci�n cuando el inventario est� lleno.
    void ShowInventoryFullMessage(string itemName, string pageName)
    {
        infoPanel.SetActive(true);
        itemNameText.text = "Inventario lleno";
        itemDescriptionText.text = $"No puedes guardar \"{itemName}\". La pagina \"{pageName}\" del inventario est� al m�ximo ({maxSlots}).";
        Debug.Log($"Inventario lleno en p�gina {pageName}. No se puede agregar: {itemName}");
    }

    // Elimina un �tem del inventario y lo suelta en el mundo del juego.
    public void RemoveItem(string itemName)
    {
        // Busca el �tem y lo elimina de la lista.
        if (pagedItems.ContainsKey(CurrentPage) && pagedItems[CurrentPage].Contains(itemName))
        {
            pagedItems[CurrentPage].Remove(itemName);
            RefreshInventoryUI();
            Debug.Log("Objeto eliminado: " + itemName);

            if (rubberHoseDeleteSound != null)
                audioSource.PlayOneShot(rubberHoseDeleteSound);

            // Crea una instancia del prefab f�sico del �tem y lo lanza.
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

    // Confirma la eliminaci�n de un �tem y cierra el panel de confirmaci�n.
    public void ConfirmDeletion()
    {
        if (!string.IsNullOrEmpty(pendingItemToRemove))
            RemoveItem(pendingItemToRemove);

        confirmDeletePanel.SetActive(false);
        pendingItemToRemove = null;
    }

    // Cancela la eliminaci�n y cierra el panel de confirmaci�n.
    public void CancelDeletion()
    {
        confirmDeletePanel.SetActive(false);
        pendingItemToRemove = null;
    }

    // Actualiza la interfaz de usuario del inventario, creando los slots y asignando los �conos.
    void RefreshInventoryUI()
    {
        // Destruye los slots existentes antes de recrearlos.
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        if (!pagedItems.ContainsKey(CurrentPage)) return;

        lastSelectedItemId = null;

        List<string> items = pagedItems[CurrentPage];
        if (items.Count == 0) return;

        // Actualiza el texto con el nombre de la p�gina.
        if (pageNameText != null)
            pageNameText.text = CurrentPage;

        // Instancia los prefabs de los slots y los configura con los datos de los �tems.
        for (int i = 0; i < items.Count && i < slotPositions.Length; i++)
        {
            string itemName = items[i];
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.name = itemName;

            // Asigna el �cono del �tem.
            Image icon = slot.GetComponentInChildren<Image>();
            if (itemIcons.ContainsKey(itemName))
                icon.sprite = itemIcons[itemName];

            // Posiciona el slot en la UI.
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchoredPosition = slotPositions[i];

            int index = i;
            // Configura los eventos de clic para seleccionar un �tem.
            slot.GetComponent<Button>().onClick.AddListener(() => SelectItem(index));

            // Configura el evento de clic derecho para la eliminaci�n.
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
                    confirmDeleteText.text = $"�Deseas eliminar \"{itemName}\"?";
                    confirmDeletePanel.SetActive(true);
                }
            });
            trigger.triggers.Add(rightClick);
        }
    }

    // Muestra la informaci�n de un �tem seleccionado en el panel de informaci�n.
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
            itemDescriptionText.text = "Sin descripci�n disponible.";
    }

    // Cambia a la siguiente p�gina del inventario.
    public void NextPage()
    {
        if (pageOrder.Count == 0) return;
        currentPageIndex = (currentPageIndex + 1) % pageOrder.Count;
        RefreshInventoryUI();
    }

    // Cambia a la p�gina anterior del inventario.
    public void PreviousPage()
    {
        if (pageOrder.Count == 0) return;
        currentPageIndex = (currentPageIndex - 1 + pageOrder.Count) % pageOrder.Count;
        RefreshInventoryUI();
    }

    // Comprueba si un �tem espec�fico est� en el inventario.
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