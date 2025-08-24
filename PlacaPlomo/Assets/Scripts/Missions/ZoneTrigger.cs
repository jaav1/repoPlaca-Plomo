using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    public string zoneId = "Z_Auto";             // Debe coincidir con target_ids del CSV
    public TriggerType triggerType = TriggerType.EnterZone;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        MissionManager.I?.ReportEvent(triggerType, zoneId);
    }
}
