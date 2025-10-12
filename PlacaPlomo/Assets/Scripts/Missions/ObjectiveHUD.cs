using UnityEngine;
using TMPro;

public class ObjectiveHUD : MonoBehaviour
{
    // Asegúrate de que este campo esté arrastrado y conectado en el Inspector.
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        // 1. Verificación en Awake: Si no se asignó en el Inspector, intenta encontrarlo.
        if (label == null)
        {
            // Busca en el GameObject local o en sus hijos.
            label = GetComponentInChildren<TextMeshProUGUI>();

            if (label == null)
            {
                Debug.LogError("ObjectiveHUD: ¡El componente TextMeshProUGUI ('label') no está asignado en el Inspector y no se pudo encontrar en los hijos del GameObject!");
            }
        }

        // 2. Limpia el texto al inicio, para que no muestre "New Text".
        if (label != null)
        {
            label.text = string.Empty;
        }
    }

    /// <summary>
    /// Establece el texto del objetivo en el HUD.
    /// </summary>
    /// <param name="text">El nuevo texto a mostrar.</param>
    public void SetText(string text)
    {
        if (label != null)
        {
            label.text = text;
            // Opcional: Esto te ayuda a confirmar que el método fue llamado correctamente.
            Debug.Log($"[ObjectiveHUD] Texto actualizado a: {text}");
        }
        else
        {
            // Si ves este error, la referencia 'label' es el problema.
            Debug.LogError($"ObjectiveHUD.SetText() fue llamado, pero 'label' es null. Texto no actualizado: {text}");
        }
    }
}