using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;

// Este script centraliza toda la lógica de la cámara fotográfica.
public class PhotoManager : MonoBehaviour
{
    // =================================================================================
    // REFERENCIAS PÚBLICAS Y ENCABEZADOS
    // =================================================================================

    [Header("Configuración de Cámaras")]
    public Camera mainCamera; // Asigna tu cámara principal aquí
    public Camera photoCamera; // Asigna la cámara que creaste para fotos aquí
    public MonoBehaviour playerLookScript; // Asigna el script de control de la vista del jugador

    [Header("UI y Sonido")]
    public GameObject instructionTextObject; // Asigna el objeto con el texto de "Presiona SPACE..."
    public GameObject photoPreviewPanel; // Asigna el panel de previsualización
    public Image photoPreviewImage; // Asigna la imagen de previsualización
    public AudioClip cameraShutterSound;
    public AudioSource audioSource;
    public GameObject hudPanel; // Asigna el panel de HUD principal (lo encuentra por tag)

    [Header("Sistemas Externos")]
    public RadialInventoryManager inventoryManager;

    // =================================================================================
    // VARIABLES INTERNAS
    // =================================================================================

    private bool isCameraActive = false;
    private string photosPath;

    // =================================================================================
    // MÉTODOS DE CICLO DE VIDA DE UNITY
    // =================================================================================

    private void Awake()
    {
        photosPath = Path.Combine(Application.persistentDataPath, "Photos");
        if (!Directory.Exists(photosPath))
        {
            Directory.CreateDirectory(photosPath);
        }
    }

    // Este es el método que activará la vista de la cámara
    public void StartCameraView()
    {
        // Activa la cámara para fotos y desactiva la principal
        if (photoCamera != null) photoCamera.gameObject.SetActive(true);
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);

        // Bloquea el control de la vista del jugador
        if (playerLookScript != null) playerLookScript.enabled = false;

        // Muestra el texto de instrucciones
        if (instructionTextObject != null) instructionTextObject.SetActive(true);

        // Oculta el HUD principal del juego
        if (hudPanel != null) hudPanel.SetActive(false);

        isCameraActive = true;

        // Congela el cursor y lo hace invisible
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Modo de cámara activado. Presiona SPACE para tomar foto.");
    }

    // Método para salir del modo de cámara
    public void ExitCameraView()
    {
        // Desactiva la cámara de fotos y activa la principal
        if (photoCamera != null) photoCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        // Devuelve el control de la vista al jugador
        if (playerLookScript != null) playerLookScript.enabled = true;

        // Oculta el texto de instrucciones
        if (instructionTextObject != null) instructionTextObject.SetActive(false);

        // Muestra el HUD principal
        if (hudPanel != null) hudPanel.SetActive(true);

        isCameraActive = false;

        Debug.Log("Saliendo del modo de cámara.");
    }

    private void Update()
    {
        // Si el modo de cámara está activo
        if (isCameraActive)
        {
            // Salir de la cámara al presionar TAB
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ExitCameraView();
            }
            // Tomar foto al presionar SPACE
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(TakeScreenshotAndPreview());
            }

            // NUEVO CÓDIGO PARA LA ROTACIÓN DE LA CÁMARA
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Aplica la rotación a la cámara de fotos
            if (photoCamera != null)
            {
                Vector3 newRotation = photoCamera.transform.localEulerAngles;
                newRotation.y += mouseX;
                newRotation.x -= mouseY; // Restamos para invertir la rotación vertical
                photoCamera.transform.localEulerAngles = newRotation;
            }
        }
    }

    // Corrutina para tomar la captura de pantalla y mostrar la previsualización
    private IEnumerator TakeScreenshotAndPreview()
    {
        // Oculta el texto de instrucciones
        if (instructionTextObject != null) instructionTextObject.SetActive(false);

        // Reproduce el sonido si existe
        if (cameraShutterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cameraShutterSound);
        }

        // --- NUEVA LÓGICA DE DETECCIÓN DE OBJETIVOS ---
        string detectedTargetId = "";
        RaycastHit hit;
        Ray ray = photoCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out hit, 100f)) // El "100f" es la distancia máxima del rayo
        {
            // Comprueba si el objeto impactado tiene el componente MissionTarget
            MissionTarget missionTarget = hit.transform.GetComponent<MissionTarget>();
            if (missionTarget != null)
            {
                detectedTargetId = missionTarget.targetId;
                // Reporta el evento de foto al gestor de misiones
                MissionManager.I?.ReportEvent(TriggerType.TakePhoto, detectedTargetId);
                Debug.Log("Foto tomada de objetivo: " + detectedTargetId);
            }
        }
        else
        {
            Debug.Log("Foto tomada, pero ningún objetivo de misión fue detectado.");
        }

        // --- FIN DE LA NUEVA LÓGICA ---

        // Espera un frame para que la UI se oculte antes de tomar la captura
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        Sprite photoSprite = Sprite.Create(screenshot, new Rect(0, 0, screenshot.width, screenshot.height), new Vector2(0.5f, 0.5f));

        // El resto de tu código para mostrar la previsualización es igual
        if (photoPreviewPanel != null && photoPreviewImage != null)
        {
            photoPreviewImage.sprite = photoSprite;
            photoPreviewPanel.SetActive(true);
        }

        // Desactiva el modo de cámara
        isCameraActive = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Método para guardar la foto
    public void SavePhoto()
    {
        // Guarda la foto en el disco
        Sprite photo = photoPreviewImage.sprite;
        if (photo == null || inventoryManager == null) return;
        string photoFileName = SavePhotoToDisk(photo);

        // Añade la foto al inventario
        inventoryManager.AddItem(photoFileName, "Fotos");
        inventoryManager.AddItemIcon(photoFileName, photo);
        inventoryManager.AddItemDescription(photoFileName, "Una foto capturada. Puedes verla con el botón 'Ver'.");

        // Regresa al estado de juego normal
        ExitCameraView();
        if (photoPreviewPanel != null) photoPreviewPanel.SetActive(false);
    }

    // Método para descartar la foto
    public void DiscardPhoto()
    {
        // Limpia el sprite de la foto
        if (photoPreviewImage != null) photoPreviewImage.sprite = null;

        // Regresa al estado de juego normal
        ExitCameraView();
        if (photoPreviewPanel != null) photoPreviewPanel.SetActive(false);
    }

    // =================================================================================
    // MÉTODOS DE MANEJO DE ARCHIVOS
    // =================================================================================

    public string SavePhotoToDisk(Sprite photoSprite)
    {
        Texture2D texture = photoSprite.texture;
        byte[] bytes = texture.EncodeToPNG();
        string photoName = "Foto_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        string filePath = Path.Combine(photosPath, photoName);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("Foto guardada en: " + filePath);
        return photoName;
    }

    public List<Sprite> LoadAllPhotos()
    {
        List<Sprite> loadedPhotos = new List<Sprite>();
        string[] files = Directory.GetFiles(photosPath, "*.png");
        foreach (string file in files)
        {
            if (File.Exists(file))
            {
                byte[] bytes = File.ReadAllBytes(file);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                loadedPhotos.Add(sprite);
            }
        }
        return loadedPhotos;
    }

    public void LoadPhotosToInventory()
    {
        string[] files = Directory.GetFiles(photosPath, "*.png");
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            Sprite loadedPhoto = LoadPhoto(fileName);
            if (loadedPhoto != null && !inventoryManager.HasItem(fileName))
            {
                inventoryManager.AddItem(fileName, "Fotos");
                inventoryManager.AddItemIcon(fileName, loadedPhoto);
                inventoryManager.AddItemDescription(fileName, "Una foto capturada.");
            }
        }
    }

    public Sprite LoadPhoto(string fileName)
    {
        string filePath = Path.Combine(photosPath, fileName);
        if (File.Exists(filePath))
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        Debug.LogWarning("Archivo de foto no encontrado: " + filePath);
        return null;
    }

    public void DeletePhoto(string photoName)
    {
        string filePath = Path.Combine(photosPath, photoName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Foto eliminada: " + filePath);
        }
        else
        {
            Debug.LogWarning("El archivo no se encontró: " + filePath);
        }
    }
}