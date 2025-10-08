using UnityEngine;
using UnityEngine.Playables; // Necesario para PlayableAsset y PlayableDirector

[CreateAssetMenu(fileName = "MissionCinematicData", menuName = "Mission/Cinematic Map", order = 1)]
public class MissionCinematicData : ScriptableObject
{
    [System.Serializable]
    public class CinematicEntry
    {
        public string MissionId;        // Ej: "M1" o "M2"
        public PlayableAsset IntroTimeline; // La cinem�tica de inicio de esta misi�n
        public PlayableAsset OutroTimeline; // (Opcional) La cinem�tica de fin de esta misi�n
    }

    public CinematicEntry[] Cinemetics;
}