using UnityEngine;

public class EmotionalStateManager : MonoBehaviour
{
    public static EmotionalStateManager Instance;

    public int confianzaMendoza = 0;
    public int rayEmpatia = 0;
    public int rayFrialdad = 0;
    public float tension = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void AjustarEstado(string tipo, int valor)
    {
        switch (tipo)
        {
            case "Empatia": rayEmpatia += valor; break;
            case "Frialdad": rayFrialdad += valor; break;
            case "Tension": tension = Mathf.Clamp(tension + valor, 0, 100); break;
            case "ConfianzaMendoza": confianzaMendoza += valor; break;
        }
    }
}
