using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] si planeas usarlo en el Inspector o guardarlo
public class MissionData
{
    // El ID principal de la misión (ej. "M2")
    public string MissionId;

    // Opcional: El título completo (ej. "El Secreto de la Iglesia")
    public string MissionTitle;

    // Opcional: Lista de todos los pasos (esto se haría en una implementación más compleja)
    // public List<MissionStep> Steps;

    // CONSTRUCTOR SIMPLE:
    public MissionData(string id)
    {
        MissionId = id;
    }
}