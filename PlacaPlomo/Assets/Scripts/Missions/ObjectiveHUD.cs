using UnityEngine;
using TMPro;

public class ObjectiveHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    public void SetText(string text)
    {
        if (label) label.text = text;
    }
}
