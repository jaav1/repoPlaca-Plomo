using UnityEngine;
using TMPro;
using System.Collections;

public class TempMessageDisplay : MonoBehaviour
{
    // === Asignaciones en el Inspector ===
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject messagePanel;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float displayDuration = 2.5f;

    // === Singleton (para fácil acceso desde MissionManager) ===
    public static TempMessageDisplay Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Empezamos con el panel oculto
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    private Coroutine activeMessageCoroutine;

    /// <summary>
    /// Muestra un mensaje temporal en la pantalla.
    /// </summary>
    public void ShowMessage(string message)
    {
        if (messageText == null || messagePanel == null) return;

        // Detenemos cualquier corrutina de mensaje anterior
        if (activeMessageCoroutine != null)
        {
            StopCoroutine(activeMessageCoroutine);
        }

        messageText.text = message;
        activeMessageCoroutine = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        // 1. Mostrar el panel
        messagePanel.SetActive(true);

        // 2. Opcional: Fade-in del texto/panel (ej. cambiando el alpha)
        // Ya que MessagePanel tiene el fondo transparente, solo animamos el texto.

        Color startColor = messageText.color;
        startColor.a = 0f;
        messageText.color = startColor;

        // Animación de aparición (Fade-in)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, alpha);
            yield return null;
        }
        messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1f);

        // 3. Esperar el tiempo de visualización
        yield return new WaitForSeconds(displayDuration);

        // 4. Animación de desaparición (Fade-out)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, alpha);
            yield return null;
        }

        // 5. Ocultar el panel y limpiar
        messagePanel.SetActive(false);
        activeMessageCoroutine = null;
    }
}