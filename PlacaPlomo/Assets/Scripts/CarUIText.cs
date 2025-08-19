using UnityEngine;
using TMPro;
using System.Collections;

public class CarUIText : MonoBehaviour
{
    public TMP_Text messageText;

    public void ShowText(string message, float duration = 5f)
    {
        StopAllCoroutines();
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        StartCoroutine(HideAfterTime(duration));
    }

    IEnumerator HideAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        messageText.gameObject.SetActive(false);
    }
}
