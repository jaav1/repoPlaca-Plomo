using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RadialInventoryManager : MonoBehaviour
{
    [Header("UI Básica")]
    public GameObject radialPanel;
    public GameObject slotPrefab;
    public Transform slotContainer;

    [Header("Ventana de Información")]
    public GameObject infoPanel;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;

    [Header("Íconos por ítem")]
    public Sprite llaveSprite;
    public Sprite carkeySprite;
    public Sprite carpetaSprite;

    [Header("Prefabs físicos para soltar")]
    public GameObject llavePrefab;
    public GameObject carkeyPrefab;
    public GameObject carpetaPrefab;

    [Header("Confirmación de eliminación")]
    public GameObject confirmDeletePanel;
    public TMP_Text confirmDeleteText;

    [Header("UI de página")]
    public TMP_Text pageNameText;

    [Header("Inventario")]
    public int maxSlots = 7; 

    [Header("Sonido de eliminación")]
    public AudioClip rubberHoseDeleteSound;
    private AudioSource audioSource;

    private Dictionary<string, List<string>> pagedItems = new Dictionary<string, List<string>>();
    private List<string> pageOrder = new List<string>();
    private int currentPageIndex = 0;
    private string pendingItemToRemove;

    private Dictionary<string, string> itemDescriptions = new Dictionary<string, string>()
    {
        { "Llave", "Una llave de bronce. Parece encajar en una cerradura antigua." },
        { "Llave de coche", "Sin esta llave no podras conducir." },
        { "Carpeta", "Al parecer hay mucha información importante aquí." }
    };

    private Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> worldItemPrefabs = new Dictionary<string, GameObject>();

    string CurrentPage => pageOrder.Count > 0 ? pageOrder[currentPageIndex] : "";

    // Posiciones fijas para los 7 slots (ajusta según tu fondo)
    private Vector2[] slotPositions = new Vector2[]
    {
        new Vector2(-50, 91),  // Slot 1
        new Vector2(50, 91),     // Slot 2
        new Vector2(105, 18),   // Slot 3
        new Vector2(99, -60),  // Slot 4
        new Vector2(0, -100),     // Slot 5
        new Vector2(-99, -60),   // Slot 6
        new Vector2(-105, 18)     // Slot 7
    };

    void Start()
    {
        itemIcons.Add("Llave", llaveSprite);
        itemIcons.Add("Llave de coche", carkeySprite);
        itemIcons.Add("Carpeta", carpetaSprite);

        worldItemPrefabs.Add("Llave", llavePrefab);
        worldItemPrefabs.Add("Llave de coche", carkeyPrefab);
        worldItemPrefabs.Add("Carpeta", carpetaPrefab);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        confirmDeletePanel.SetActive(false);
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
            return false; // Inventario lleno, no agregar
        }

        pageList.Add(itemName);
        RefreshInventoryUI();
        return true; // Agregado con éxito
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            radialPanel.SetActive(true);
            infoPanel.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            RefreshInventoryUI();
        }

        if (Input.GetKeyUp(KeyCode.Tab))
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
        infoPanel.SetActive(true);
        itemNameText.text = selected;

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
        if (!pagedItems.ContainsKey(CurrentPage))
            return false;

        foreach (var list in pagedItems.Values)
            if (list.Contains(itemName))
                return true;

        return false;
    }
}
