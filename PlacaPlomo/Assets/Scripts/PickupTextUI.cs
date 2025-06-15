using UnityEngine;
using TMPro;

public class PickupTextUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Start()
    {
        text.gameObject.SetActive(false);
    }

    public void ShowText(string message)
    {
        text.text = message;
        text.gameObject.SetActive(true);
    }

    public void HideText()
    {
        text.gameObject.SetActive(false);
    }
}
