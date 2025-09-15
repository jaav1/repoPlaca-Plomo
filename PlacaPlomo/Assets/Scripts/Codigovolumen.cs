using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodigoVolumen : MonoBehaviour
{
    public Slider Slider;
    public float SliderValue;
    public Image ImageMute;

    // Start is called before the first frame update
    void Start()
    {
        Slider.value = PlayerPrefs.GetFloat("VolumenAudio", 0.5f); //para que el valor del volumen se guarde
        AudioListener.volume = Slider.value;
        RevisarSiEstoyMute();
    }

    public void ChangeSlider(float valor)
    {
        SliderValue = valor;
        PlayerPrefs.SetFloat("VolumenAudio", SliderValue);
        AudioListener.volume = SliderValue;
        RevisarSiEstoyMute();
    }

    public void RevisarSiEstoyMute()
    {
        if (SliderValue == 0)
        {
            ImageMute.enabled = true;
        }
        else
        {
            ImageMute.enabled = false;
        }
    }
}