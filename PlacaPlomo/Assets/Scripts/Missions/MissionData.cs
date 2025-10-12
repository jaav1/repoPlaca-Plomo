using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] si planeas usarlo en el Inspector o guardarlo
public class MissionData
{
    // El ID principal de la misi?n (ej. "M2")
    public string MissionId;

    // Opcional: El t?tulo completo (ej. "El Secreto de la Iglesia")
    public string MissionTitle;

    // Opcional: Lista de todos los pasos (esto se har?a en una implementaci?n m?s compleja)
    // public List<MissionStep> Steps;

    // CONSTRUCTOR SIMPLE:
    public MissionData(string id)
    {
        MissionId = id;
    }
}