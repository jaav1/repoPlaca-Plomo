using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    [Tooltip("ID del NPC o Target ID del CSV para notificar a la misin (ej: NPC_DonTomas)")]
    public string targetMissionID;

    public string speakerName;

    [TextArea(3, 10)]
    public string dialogueText;

    public DialogueOption[] options;
}
