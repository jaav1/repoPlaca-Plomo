using UnityEngine;

public class ClueMissionBridge : MonoBehaviour
{
    public string targetId;

    public void TriggerMissionEvent()
    {
        Debug.Log($"[ClueMissionBridge] Enviando evento de misión: TriggerType.Interact, {targetId}");
        MissionManager.I?.ReportEvent(TriggerType.Interact, targetId);
    }
}