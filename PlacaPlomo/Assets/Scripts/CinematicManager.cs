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

        // Cargar el mapa de cinem�ticas
        if (cinematicData != null)
        {
            foreach (var entry in cinematicData.Cinemetics)
            {
                // Mapeamos el ID de la misi�n (ej. M1) a su Intro Timeline.
                introMap[entry.MissionId] = entry.IntroTimeline;
            }
        }
    }

    private void Start()
    {
        // Esto asume que tu MissionManager tiene el startingStepId configurado a M1-01.
        // Iniciamos la primera cinem�tica del juego (ej. la de Misi�n 1).
        StartMissionIntro("M1-01");
    }

    /// <summary>
    /// Inicia la secuencia de introducci�n para la Misi�n asociada al stepId.
    /// </summary>
    /// <param name="nextStepId">El primer paso de la misi�n a iniciar (ej. "M2-01").</param>
    public void StartMissionIntro(string nextStepId)
    {
        // Obtener el ID de la Misi�n (asumimos que es M1, M2, etc.)
        string missionId = nextStepId.Substring(0, 2);

        if (introMap.TryGetValue(missionId, out PlayableAsset introTimeline) && introTimeline != null)
        {
            StartCoroutine(PlayCinematicRoutine(introTimeline, nextStepId));
        }
        else
        {
            Debug.LogWarning($"No se encontr� una cinem�tica de introducci�n para {missionId}. Iniciando la misi�n directamente.");
            // Si no hay cinem�tica, iniciamos la misi�n inmediatamente.
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
            // o asegurarte de que tus m�todos Disable/EnableControls sean PUBLICOS en PlayerMovement.
            playerMovement.enabled = false;
        }

        // 2. Reproducir la Timeline
        playableDirector.playableAsset = timeline;
        playableDirector.Play();

        // 3. Esperar a que la cinem�tica termine
        float duration = (float)timeline.duration;
        yield return new WaitForSeconds(duration);

        // 4. Iniciar la Misi�n
        MissionManager.I?.GoToStep(missionStepId);

        // 5. Re-habilitar Controles
        if (playerMovement != null)
        {
            playerMovement.enabled = true; // O playerMovement.EnableControls();
        }
    }
    // Puedes a�adir un m�todo para las cinem�ticas de Outro aqu� si es necesario.
}