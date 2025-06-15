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
    public float radius = 150f;

    [Header("Ventana de Información")]
    public GameObject infoPanel;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;

    [Header("Íconos por ítem")]
    public Sprite llaveSprite;
    public Sprite discoSprite;
    public Sprite carpetaSprite;

    [Header("Prefabs físicos para soltar")]
    public GameObject llavePrefab;
    public GameObject discoPrefab;
    public GameObject carpetaPrefab;

    [Header("Confirmación de eliminación")]
    public GameObject confirmDeletePanel;
    public TMP_Text confirmDeleteText;

    [Header("UI de página")]
    public TMP_Text pageNameText;

    public int maxSlots = 12;

    private Dictionary<string, List<string>> pagedItems = new Dictionary<string, List<string>>();
    private List<string> pageOrder = new List<string>();
    private int currentPageIndex = 0;
    private string pendingItemToRemove;

    private Dictionary<string, string> itemDescriptions = new Dictionary<string, string>()
    {
        { "Llave", "Una llave de bronce. Parece encajar en una cerradura antigua." },
        { "Disco Duro", "Contiene información importante. No lo pierdas." },
        { "Carpeta", "Al parecer hay mucha información importante aquí." }
    };

    private Dictionary<string, Sprite> itemIcons = new Dictionary<string, Sprite>();
    private Dictionary<string, GameObject> worldItemPrefabs = new Dictionary<string, GameObject>();

    string CurrentPage => pageOrder.Count > 0 ? pageOrder[currentPageIndex] : "";

    void Start()
    {
        itemIcons.Add("Llave", llaveSprite);
        itemIcons.Add("Disco Duro", discoSprite);
        itemIcons.Add("Carpeta", carpetaSprite);

        worldItemPrefabs.Add("Llave", llavePrefab);
        worldItemPrefabs.Add("Disco Duro", discoPrefab);
        worldItemPrefabs.Add("Carpeta", carpetaPrefab);

        confirmDeletePanel.SetActive(false);
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
            Debug.Log($"Página '{pageName}' llena. No se puede agregar: " + itemName);
            return;
        }

        pageList.Add(itemName);
        RefreshInventoryUI();
    }

    public void RemoveItem(string itemName)
    {
        if (pagedItems.ContainsKey(CurrentPage) && pagedItems[CurrentPage].Contains(itemName))
        {
            pagedItems[CurrentPage].Remove(itemName);
            RefreshInventoryUI();
            Debug.Log("Objeto eliminado: " + itemName);

            if (worldItemPrefabs.ContainsKey(itemName))
            {
                Vector3 dropPos = Camera.main.transform.position
                            + Camera.main.transform.forward * 6.0f;
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

        float angleStep = 360f / items.Count;
        float angle = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            string itemName = items[i];
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.name = itemName;

            Image icon = slot.GetComponentInChildren<Image>();
            if (itemIcons.ContainsKey(itemName))
                icon.sprite = itemIcons[itemName];

            RectTransform rt = slot.GetComponent<RectTransform>();
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            rt.anchoredPosition = new Vector2(x, y);

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

            angle += angleStep;
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
}