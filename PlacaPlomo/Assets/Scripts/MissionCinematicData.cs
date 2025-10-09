using UnityEngine;
using UnityEngine.Playables; // Necesario para PlayableAsset y PlayableDirector

[CreateAssetMenu(fileName = "MissionCinematicData", menuName = "Mission/Cinematic Map", order = 1)]
public class MissionCinematicData : ScriptableObject
{
    [System.Serializable]
    public class CinematicEntry
    {
        public string MissionId;        // Ej: "M1" o "M2"
        public PlayableAsset IntroTimeline; // La cinemática de inicio de esta misión
        public PlayableAsset OutroTimeline; // (Opcional) La cinemática de fin de esta misión
    }

    public CinematicEntry[] Cinemetics;
}