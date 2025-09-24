using UnityEngine;

public class ClueMissionBridge : MonoBehaviour
{
    public string targetId;
    public TriggerType triggerType = TriggerType.Interact;

    public void TriggerMissionEvent()
    {
        MissionManager.I?.ReportEvent(triggerType, targetId); 
    }
}
/*
using UnityEngine;

public class ClueMissionBridge : MonoBehaviour
{
    // Ya no necesitamos estas variables, ya que los valores correctos se pasan por parámetro
    // public string targetId;
    // public TriggerType triggerType = TriggerType.Interact;

    public void TriggerMissionEvent(TriggerType type, string id)
    {
        MissionManager.I?.ReportEvent(type, id);
    }
}
*/