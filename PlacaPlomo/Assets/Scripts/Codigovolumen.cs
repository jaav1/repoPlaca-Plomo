using UnityEngine;
using UnityEngine.UI;

public class CodigoVolumen : MonoBehaviour
{
    public Slider Slider;
    public Image ImageMute;

    private float sliderValue;

    void Start()
    {
        // Recupera el volumen guardado o usa 0.5 como valor por defecto
        sliderValue = PlayerPrefs.GetFloat("VolumenAudio", 0.5f);
        Slider.value = sliderValue;

        // Aplica el volumen sin distorsión
        AudioListener.volume = Mathf.Clamp(sliderValue, 0f, 1f);

        RevisarSiEstoyMute();
    }

    public void ChangeSlider(float valor)
    {
        // Asegura que el valor esté entre 0 y 1
        sliderValue = Mathf.Clamp(valor, 0f, 1f);
        PlayerPrefs.SetFloat("VolumenAudio", sliderValue);

        // Aplica el volumen
        AudioListener.volume = sliderValue;

        RevisarSiEstoyMute();
    }

    private void RevisarSiEstoyMute()
    {
        // Activa la imagen de mute si el volumen es exactamente 0
        ImageMute.enabled = sliderValue == 0f;
    }
}