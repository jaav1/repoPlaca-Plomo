using UnityEngine;

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public Dialogue nextDialogue;
    public float confianzaDelta;
    public float sospechaDelta;
}
