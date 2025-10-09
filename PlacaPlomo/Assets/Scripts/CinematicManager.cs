using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;

public class CinematicManager : MonoBehaviour
{
    public static CinematicManager I { get; private set; }

    // Asignaciones en el Inspector
    [SerializeField] private PlayableDirector playableDirector; // El componente Timeline
    [SerializeField] private MissionCinematicData cinematicData;

    private Dictionary<string, PlayableAsset> introMap = new();

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        // Cargar el mapa de cinemáticas
        if (cinematicData != null)
        {
            foreach (var entry in cinematicData.Cinemetics)
            {
                // Mapeamos el ID de la misión (ej. M1) a su Intro Timeline.
                introMap[entry.MissionId] = entry.IntroTimeline;
            }
        }
    }

    private void Start()
    {
        // Esto asume que tu MissionManager tiene el startingStepId configurado a M1-01.
        // Iniciamos la primera cinemática del juego (ej. la de Misión 1).
        StartMissionIntro("M1-01");
    }

    /// <summary>
    /// Inicia la secuencia de introducción para la Misión asociada al stepId.
    /// </summary>
    /// <param name="nextStepId">El primer paso de la misión a iniciar (ej. "M2-01").</param>
    public void StartMissionIntro(string nextStepId)
    {
        // Obtener el ID de la Misión (asumimos que es M1, M2, etc.)
        string missionId = nextStepId.Substring(0, 2);

        if (introMap.TryGetValue(missionId, out PlayableAsset introTimeline) && introTimeline != null)
        {
            StartCoroutine(PlayCinematicRoutine(introTimeline, nextStepId));
        }
        else
        {
            Debug.LogWarning($"No se encontró una cinemática de introducción para {missionId}. Iniciando la misión directamente.");
            // Si no hay cinemática, iniciamos la misión inmediatamente.
            MissionManager.I?.GoToStep(nextStepId);
        }
    }

    private IEnumerator PlayCinematicRoutine(PlayableAsset timeline, string missionStepId)
    {
        // Obtener el componente PlayerMovement una vez
        PlayerMovement playerMovement = MissionManager.I?.GetPlayerTransform()?.GetComponent<PlayerMovement>();

        // 1. Deshabilitar Controles del Jugador
        if (playerMovement != null)
        {
            // En lugar de llamar a DisableControls(), puedes deshabilitar el script completo
            // o asegurarte de que tus métodos Disable/EnableControls sean PUBLICOS en PlayerMovement.
            playerMovement.enabled = false;
        }

        // 2. Reproducir la Timeline
        playableDirector.playableAsset = timeline;
        playableDirector.Play();

        // 3. Esperar a que la cinemática termine
        float duration = (float)timeline.duration;
        yield return new WaitForSeconds(duration);

        // 4. Iniciar la Misión
        MissionManager.I?.GoToStep(missionStepId);

        // 5. Re-habilitar Controles
        if (playerMovement != null)
        {
            playerMovement.enabled = true; // O playerMovement.EnableControls();
        }
    }
    // Puedes añadir un método para las cinemáticas de Outro aquí si es necesario.
}