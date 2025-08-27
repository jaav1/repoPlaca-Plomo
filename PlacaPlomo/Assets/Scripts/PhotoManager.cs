using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PhotoManager : MonoBehaviour
{
    // Almacena la ruta donde se guardarán las fotos.
    private string photosPath;

    private void Awake()
    {
        photosPath = Path.Combine(Application.persistentDataPath, "Photos");

        // Crea la carpeta si no existe.
        if (!Directory.Exists(photosPath))
        {
            Directory.CreateDirectory(photosPath);
        }
    }

    // Guarda un Sprite como archivo .png y devuelve el nombre del archivo.
    public string SavePhoto(Sprite photoSprite)
    {
        // Convierte el Sprite a una Texture2D.
        Texture2D texture = photoSprite.texture;
        byte[] bytes = texture.EncodeToPNG();

        // Genera un nombre de archivo único con un timestamp.
        string photoName = "Foto_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        string filePath = Path.Combine(photosPath, photoName);

        // Guarda el archivo.
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("Foto guardada en: " + filePath);

        return photoName;
    }

    // Carga todas las fotos guardadas y las devuelve como una lista de Sprites.
    public List<Sprite> LoadAllPhotos()
    {
        List<Sprite> loadedPhotos = new List<Sprite>();

        // Obtiene todos los archivos .png de la carpeta de fotos.
        string[] files = Directory.GetFiles(photosPath, "*.png");

        foreach (string file in files)
        {
            // Carga los bytes del archivo y los convierte en una textura.
            byte[] bytes = File.ReadAllBytes(file);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);

            // Crea un Sprite a partir de la textura y lo añade a la lista.
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            loadedPhotos.Add(sprite);
        }

        return loadedPhotos;
    }
}