using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    public string speakerName;

    [TextArea(3, 10)]
    public string dialogueText;

    public DialogueOption[] options;
}
