using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro; // Para HUD

public class MissionManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string resourcesPath = "Data/missions_step_list"; // sin .csv
    [SerializeField] private string startingStepId = "M1-01";

    [Header("Refs")]
    [SerializeField] private ObjectiveHUD objectiveHUD;

    // Estado
    private readonly Dictionary<string, MissionStep> stepsById = new();
    private MissionStep current;
    private readonly HashSet<string> flags = new();  // SIMPLE: guarda flags presentes (FLAG=1)
    private float suspicion = 0f;

    // Para pasos que requieren múltiples eventos
    private readonly HashSet<string> _satisfiedForThisStep = new();

    private Coroutine chaseTimerRoutine;

    private GameObject enemyObjRef;
    private Transform playerTransformRef;

    private Coroutine successCheckRoutine;

    private VehicleInteraction vehicleInteractionRef;

    #region Singleton simple (opcional)
    public static MissionManager I { get; private set; }
    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // *** INICIALIZACIÓN DE REFERENCIAS ***
        enemyObjRef = GameObject.Find("E_Perseguidor");

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransformRef = player.transform;
        else Debug.LogError("Awake(): ¡El objeto del Jugador no fue encontrado con la etiqueta 'Player'!");

        // Agrega la búsqueda del VehicleInteraction
        GameObject carObj = GameObject.Find("Car"); // <-- Usa el mismo que en PursuerNavMeshAI
        if (carObj != null)
        {
            vehicleInteractionRef = carObj.GetComponent<VehicleInteraction>();
        }
        if (vehicleInteractionRef == null) Debug.LogWarning("VehicleInteraction no encontrado. La misión de persecución puede fallar.");

        LoadCsv();
    }
    #endregion

    private void Start()
    {
        GoToStep(startingStepId);
    }

    private void LoadCsv()
    {
        TextAsset ta = Resources.Load<TextAsset>(resourcesPath);
        if (ta == null) { Debug.LogError("CSV no encontrado en Resources/" + resourcesPath); return; }

        var lines = ta.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        if (lines.Length <= 1) { Debug.LogError("CSV vacío."); return; }

        // Encabezados
        // Asumo que tienes clases como CsvUtility, MissionStep, MissionEventReq, TriggerType, MissionParsers.
        var headers = CsvUtility.ParseLine(lines[0]);
        var idx = new Dictionary<string, int>();
        for (int i = 0; i < headers.Count; i++) idx[headers[i]] = i;

        // Helper para leer celda
        string Cell(List<string> row, string col) => idx.ContainsKey(col) && idx[col] < row.Count ? row[idx[col]] : "";

        for (int l = 1; l < lines.Length; l++)
        {
            if (string.IsNullOrWhiteSpace(lines[l])) continue;
            var row = CsvUtility.ParseLine(lines[l]);

            var step = new MissionStep
            {
                Mission = Cell(row, "mission"),
                StepId = Cell(row, "step_id"),
                StepTitle = Cell(row, "step_title"),
                OnScreenText = Cell(row, "on_screen_text"),
                LocationZone = Cell(row, "location_zone"),
                ChoiceBranchRaw = Cell(row, "choice_branch"),
                NextOnSuccess = Cell(row, "next_step_on_success"),
                NextOnAlt = Cell(row, "next_step_on_alt"),
                AddsFlagsRaw = Cell(row, "adds_flags"),
                RemovesFlagsRaw = Cell(row, "removes_flags"),
                SuspicionDelta = MissionParsers.ParseSuspicion(Cell(row, "increments")),
                Rewards = Cell(row, "rewards"),
                FailureCondition = Cell(row, "failure_condition"),
                Comments = Cell(row, "comments")
            };

            // Triggers y Targets: pueden venir múltiples con "+" y ","
            var triggers = MissionParsers.ParseTriggersList(Cell(row, "trigger"));
            var targets = MissionParsers.ParseListByComma(Cell(row, "target_ids"));
            var requiredItems = MissionParsers.ParseListByComma(Cell(row, "required_items"));
            step.RequiredItems = requiredItems;

            int count = Mathf.Min(triggers.Count, targets.Count);
            for (int i = 0; i < count; i++)
            {
                step.Requirements.Add(new MissionEventReq
                {
                    Type = triggers[i],
                    TargetId = targets[i],
                    Completed = false
                });
            }

            if (triggers.Count != targets.Count)
                Debug.LogWarning($"[{step.StepId}] Triggers({triggers.Count}) != Targets({targets.Count}). Revisa el CSV.");

            if (!string.IsNullOrEmpty(step.StepId))
                stepsById[step.StepId] = step;
        }

        Debug.Log($"MissionManager: cargadas {stepsById.Count} filas del CSV.");
    }

    // Método para obtener el Transform que debe escapar
    public Transform GetChaseTarget()
    {
        // Si la referencia al jugador no está, la busca (como en GetPlayerTransform)
        if (playerTransformRef == null) GetPlayerTransform();

        // Si el jugador está DENTRO del coche, el target de escape es el coche.
        if (vehicleInteractionRef != null && vehicleInteractionRef.IsPlayerInside)
        {
            // Asumiendo que vehicleInteraction está en el mismo objeto que CarController,
            // y ese objeto es el coche.
            return vehicleInteractionRef.transform;
        }

        // Si no, el target de escape es el jugador.
        return playerTransformRef;
    }

    public void GoToStep(string stepId)
    {
        if (string.IsNullOrEmpty(stepId) || !stepsById.TryGetValue(stepId, out var s))
        {
            Debug.LogWarning("Step no encontrado: " + stepId);
            return;
        }
        current = s;
        _satisfiedForThisStep.Clear();
        foreach (var r in current.Requirements) r.Completed = false;

        // Aplica delta de sospecha al ENTER del paso
        if (Mathf.Abs(current.SuspicionDelta) > 0.0001f)
        {
            suspicion = Mathf.Max(0f, suspicion + current.SuspicionDelta);
            // TODO: actualizar UI de sospecha si existe
        }

        // Flags
        ApplyFlags(current.AddsFlagsRaw, add: true);
        ApplyFlags(current.RemovesFlagsRaw, add: false);

        // HUD
        if (objectiveHUD) objectiveHUD.SetText(current.OnScreenText);

        Debug.Log($"[MISSION] {current.StepId} ? {current.StepTitle}");

        // Lógica específica para el paso de persecución M1-08
        if (current.StepId == "M1-08")
        {
            GameObject startPoint = GameObject.Find("M1-08_START");
            if (startPoint != null)
            {
                // 1. Mover al jugador al punto de inicio (con distancia)
                Transform playerTransform = GetPlayerTransform();
                PlayerMovement playerMovement = playerTransform?.GetComponent<PlayerMovement>();

                if (playerMovement != null)
                {
                    // Usa una distancia de 30m para el inicio, o 15m si usas la variable del restart
                    float startDistance = 30f;
                    Vector3 destination = startPoint.transform.position + startPoint.transform.forward * startDistance;

                    playerMovement.TeleportTo(destination);

                    // **CORRECCIÓN CRUCIAL:** HABILITAR los controles del jugador al inicio/reinicio del paso.
                    playerMovement.EnableControls();
                }

                // 2. Mover al NPC al punto de inicio
                GameObject enemyObj = GameObject.Find("E_Perseguidor");
                if (enemyObj != null)
                {
                    Rigidbody enemyRB = enemyObj.GetComponent<Rigidbody>();
                    if (enemyRB != null)
                    {
                        enemyRB.position = startPoint.transform.position;
                        enemyRB.rotation = startPoint.transform.rotation;
                    }
                    else
                    {
                        enemyObj.transform.position = startPoint.transform.position;
                        enemyObj.transform.rotation = startPoint.transform.rotation;
                    }

                    // 3. Activar la IA del perseguidor
                    enemyObj.GetComponent<PursuerNavMeshAI>()?.StartChase();
                }

            }
            else
            {
                Debug.LogError("M1-08_START no encontrado. No se puede iniciar la persecución correctamente.");
                return;
            }

            // 4. Iniciar el temporizador para el fracaso por tiempo (30 segundos)
            if (chaseTimerRoutine != null) StopCoroutine(chaseTimerRoutine); // Asegura que no hay 2 corriendo
            chaseTimerRoutine = StartCoroutine(ChaseTimer(60f, current.StepId)); // <--- CORRECCIÓN
        }

        // Lógica específica para activar la pizarra
        if (current.StepId == "M1-09A")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            // Asegúrate de que tu ChalkboardSolver esté implementado como un Singleton 
            // o búscalo con GameObject.Find.
            ChalkboardSolver solver = FindFirstObjectByType<ChalkboardSolver>();
            if (solver != null)
            {
                solver.ShowChalkboard();
                // El solver se encargará de reportar el evento de éxito cuando se resuelva.
            }
            else
            {
                Debug.LogError("ChalkboardSolver no encontrado en la escena.");
            }
        }

        if (current.StepId == "M2-08A")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // ESTA LÍNEA ES LA CLAVE:
            ChalkboardSolver1 solver = FindFirstObjectByType<ChalkboardSolver1>();

            if (solver != null)
            {
                solver.ShowChalkboard();
            }
            else
            {
                Debug.LogError("ChalkboardSolver1 no encontrado en la escena. Asegúrate de que el objeto esté activo.");
            }
        }
    }

    private void ApplyFlags(string flagsRaw, bool add)
    {
        if (string.IsNullOrWhiteSpace(flagsRaw)) return;
        var parts = flagsRaw.Split(';');
        foreach (var p in parts)
        {
            var t = p.Trim();
            if (string.IsNullOrEmpty(t) || t.StartsWith("if", StringComparison.OrdinalIgnoreCase)) continue;

            var kv = t.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (kv.Length == 2)
            {
                var key = kv[0].Trim();
                var val = kv[1].Trim();
                if (add && val == "1") flags.Add(key);
                if (!add) flags.Remove(key);
            }
        }
    }

    private IEnumerator ContinuousSuccessCheck()
    {
        // Espera un pequeño momento para que la física se asiente
        yield return null;

        while (current != null && current.StepId == "M1-08") // Asegura que solo corre en M1-08
        {
            Debug.Log("... Chequeando distancia de escape (bucle activo) ...");

            // Llama a CheckSuccessCondition (que ahora SÍ imprime la distancia)
            if (CheckSuccessCondition(current, TriggerType.ReachZone))
            {
                TempMessageDisplay.Instance.ShowMessage("¡HAS ESCAPADO! Misión completada.");

                // Éxito confirmado.
                string nextStep = current.NextOnSuccess;

                // *** DEBUG CRÍTICO PARA CONFIRMAR EL DESTINO ***
                Debug.Log($"¡CORRUTINA CONFIRMA ÉXITO! Avanzando a: {nextStep}");

                // 1. Detener al perseguidor de nuevo, si fuera necesario (aunque ya está 'desactivado')
                enemyObjRef?.GetComponent<PursuerNavMeshAI>()?.StopChase();

                // 2. Avanzar el paso
                AdvanceTo(nextStep);

                break; // Sale del bucle
            }

            yield return new WaitForSeconds(0.2f);
        }

        // Muy importante: detiene la corrutina y limpia la referencia
        successCheckRoutine = null;
        Debug.Log("Corrutina de chequeo de éxito finalizada.");
    }

    // === API pública para que el juego reporte eventos ===
    public void ReportEvent(TriggerType type, string targetId)
    {
        if (current == null) return;


        bool anyMatched = false;
        foreach (var req in current.Requirements)
        {
            if (!req.Completed && req.Type == type && string.Equals(req.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
            {
                req.Completed = true;
                _satisfiedForThisStep.Add($"{type}:{targetId}");
                anyMatched = true;

                // *** LÓGICA ESPECÍFICA PARA M1-08 Y REACHZONE ***
                if (current.StepId == "M1-08" && req.Type == TriggerType.ReachZone)
                {
                    // 1. Cancelar Timer
                    if (chaseTimerRoutine != null)
                    {
                        StopCoroutine(chaseTimerRoutine);
                        chaseTimerRoutine = null;
                        Debug.Log("ChaseTimer cancelado: Jugador alcanzó la zona segura.");
                    }

                    // 2. Detener Perseguidor
                    enemyObjRef?.GetComponent<PursuerNavMeshAI>()?.StopChase();
                    Debug.Log("Perseguidor congelado: Chequeando distancia de escape.");

                    // 3. INICIAR CHEQUEO CONTINUO
                    if (successCheckRoutine == null)
                    {
                        successCheckRoutine = StartCoroutine(ContinuousSuccessCheck());
                        Debug.Log("Iniciada Corrutina de Chequeo de Éxito.");
                    }

                    break; // Salimos del foreach, ya encontramos el requisito
                }

                break; // Salimos del foreach, ya encontramos el requisito
            }
        }

        if (!anyMatched) return;

        // AHORA verificamos si todos los requisitos han sido cumplidos
        if (anyMatched && _satisfiedForThisStep.Count == current.Requirements.Count)
        {
            if (CheckSuccessCondition(current, type))
            {
                if (!current.IsChoiceStep)
                {
                    AdvanceTo(current.NextOnSuccess);
                }
                else
                {
                    if (objectiveHUD) objectiveHUD.SetText(current.OnScreenText + " (elige una opción)");
                }
            }
        }
    }

    // Para pasos con Choice (ej. "ENTREGAR / GUARDAR")
    public void SubmitChoice(string choiceBranch)
    {
        if (current == null || !current.IsChoiceStep) return;

        var branches = current.ChoiceBranchRaw.Split(new[] { '/', ';' }, StringSplitOptions.RemoveEmptyEntries);

        string nextStepId = current.NextOnAlt; // Por defecto va a la rama alternativa (el segundo paso)

        // Busca el texto de la elección enviada ("ENTREGAR" o "GUARDAR")
        for (int i = 0; i < branches.Length; i++)
        {
            if (string.Equals(branches[i].Trim(), choiceBranch, StringComparison.OrdinalIgnoreCase))
            {
                // Si la elección coincide con el primer elemento (índice 0)
                if (i == 0)
                {
                    nextStepId = current.NextOnSuccess;
                }
                // Cualquier otro índice usa NextOnAlt (asumiendo que solo hay dos opciones)
                else
                {
                    nextStepId = current.NextOnAlt;
                }
                break;
            }
        }

        // LÓGICA M2-02 (Acceso Trasero)
        if (current.StepId == "M2-02")
        {
            // Si el camino es NextOnSuccess (M2-03A)
            if (nextStepId == current.NextOnSuccess)
            {
                ApplyFlags("Acceso_Trasero=1", add: true);
                Debug.Log("[M2-02] Flag 'Acceso_Trasero' aplicado. La puerta trasera ahora está abierta.");
            }
            else // Si es el camino NextOnAlt (M2-03B)
            {
                // Opcional: Asegurarse de que el flag no esté puesto
                ApplyFlags("Acceso_Trasero=1", add: false);
            }
        }

        // LÓGICA M2-06 (Conflicto en la Iglesia)
        if (current.StepId == "M2-06")
        {
            // Aquí debes verificar la elección específica o el ID del paso de éxito (M2-07A)
            if (choiceBranch.Equals("Confrontar", StringComparison.OrdinalIgnoreCase))
            {
                ApplyFlags("ConflictoIglesia=1", add: true);
            }
        }

        Debug.Log($"[MissionManager] Decisión: {choiceBranch}. Avanzando a: {nextStepId}");
        AdvanceTo(nextStepId);
    }

    private void AdvanceTo(string nextId)
    {
        if (string.IsNullOrEmpty(nextId))
        {
            Debug.Log("Paso finalizado sin siguiente step. Fin de misión/capítulo.");
            if (objectiveHUD) objectiveHUD.SetText("Misión actual completa.");
            return;
        }
        // llamamos al CinematicManager para que reproduzca la introducción 
        // de la nueva misión y luego llame a GoToStep.

        if (CinematicManager.I != null)
        {
            CinematicManager.I.StartMissionIntro(nextId);
        }
        else
        {
            // Fallback: si el CinematicManager no está listo, vamos directo al paso.
            GoToStep(nextId);
        }
    }

    // Utilidad por si necesitas consultar flags/sospecha
    public bool HasFlag(string flag) => flags.Contains(flag);
    public float GetSuspicion() => suspicion;

    // === Helpers para Decisiones ===

    public bool IsCurrentStepAChoice()
    {
        return current != null && current.IsChoiceStep;
    }

    public string GetCurrentStepChoiceBranch()
    {
        return current?.ChoiceBranchRaw ?? "";
    }

    public bool CheckSuccessCondition(MissionStep currentStep, TriggerType lastTrigger)
    {
        // Solo chequea si todos los requisitos (incluyendo ReachZone) se cumplieron
        bool allRequirementsMet = _satisfiedForThisStep.Count == currentStep.Requirements.Count;

        // LÓGICA M1-08: SOBREESCRIBE LA CONDICIÓN DE ÉXITO ESTÁNDAR
        if (currentStep.StepId == "M1-08")
        {
            Transform escapeTarget = GetChaseTarget();

            if (enemyObjRef != null && escapeTarget != null)
            {
                float distanceToEnemy = Vector3.Distance(escapeTarget.position, enemyObjRef.transform.position);
                float escapeThreshold = 20f;

                // EL DEBUG MÁS IMPORTANTE
                Debug.Log($"[DISTANCIA REAL FINAL] Distancia actual: {distanceToEnemy:F2}m. Target: >{escapeThreshold}m. Target Name: {escapeTarget.name}");

                // LA ÚNICA CONDICIÓN DE ÉXITO DEBE SER LA DISTANCIA
                if (distanceToEnemy > escapeThreshold)
                {
                    // Si la distancia es OK, y ya se cumplió el requisito de la zona, avanzamos.
                    // Como esta corrutina SOLO empieza después de cumplir el ReachZone, 
                    // podemos estar seguros de que si llegamos aquí, SÍ está listo para avanzar.
                    Debug.Log("¡Escape EXITOSO! Distancia cumplida. Avanzando misión.");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // (Tu lógica de error de referencia que ya está funcionando)
                if (enemyObjRef == null) Debug.LogError("CheckSuccessCondition: E_Perseguidor es NULL. Fallo de referencia.");
                if (playerTransformRef == null) Debug.LogError("CheckSuccessCondition: Player es NULL. Fallo de referencia.");
                return false;
            }
        }

        // Para todos los demás pasos, simplemente devolvemos si todos los requisitos se cumplieron.
        return allRequirementsMet;
    }

    // Corrutina para manejar el tiempo límite de la persecución
    private IEnumerator ChaseTimer(float duration, string startStepID)
    {
        yield return new WaitForSeconds(duration);

        if (current != null && current.StepId == startStepID)
        {
            TempMessageDisplay.Instance.ShowMessage("TIEMPO AGOTADO. No pudiste escapar.");
            Debug.LogWarning($"El tiempo de persecución terminó ({duration}s). Reiniciando M1-08.");

            // No necesitas ReportFailure aquí, solo llamas al delay de reinicio.
            StartCoroutine(RestartAfterDelay(1.5f, current.StepId));
        }
    }

    public void ReportFailure(string failureReason)
    {
        // Solo si estamos en la misión 08 (o la misión activa)
        if (current != null && current.StepId == "M1-08")
        {
            // 1. Detener al NPC inmediatamente (usamos la referencia de clase)
            enemyObjRef?.GetComponent<PursuerNavMeshAI>()?.StopChase();

            // 2. DESHABILITAR EL MOVIMIENTO DEL JUGADOR al fallar
            // Usamos GetPlayerTransform() para asegurar que la referencia de CLASE esté fresca.
            Transform playerTransform = GetPlayerTransform();
            playerTransform?.GetComponent<PlayerMovement>()?.DisableControls();

            Debug.LogError($"Fracaso en M1-08: {failureReason}. Reiniciando...");

            // 3. Reiniciar el paso
            RestartFromStep(current.StepId);
        }
    }

    public Transform GetPlayerTransform()
    {
        // Si la referencia de clase es null, la buscamos y la guardamos.
        if (playerTransformRef == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransformRef = player.transform;
                return playerTransformRef;
            }

            Debug.LogError("Player object not found with tag 'Player'!");
            return null;
        }

        // Si ya existe, solo la devolvemos.
        return playerTransformRef;
    }

    // Método para reiniciar el estado de un paso
    public void RestartFromStep(string stepId)
    {
        // 1. Encontrar el punto de reinicio (Checkpoint)
        GameObject startPoint = GameObject.Find(stepId + "_START");
        if (startPoint == null)
        {
            Debug.LogError($"[REINICIO] Punto de inicio no encontrado para el paso: {stepId}_START. No se puede reiniciar.");
            return;
        }

        // 2. Detener y reubicar al perseguidor (usamos la referencia de clase)
        if (enemyObjRef != null)
        {
            PursuerNavMeshAI pursuer = enemyObjRef.GetComponent<PursuerNavMeshAI>();
            pursuer?.StopChase();

            // 3. Reubicar el NPC al punto de inicio (usamos la referencia de clase)
            // ... (Tu lógica de reubicación usando enemyObjRef)
            Rigidbody enemyRB = enemyObjRef.GetComponent<Rigidbody>();
            if (enemyRB != null)
            {
                // ... (Tu lógica de reubicación es correcta)
                enemyRB.transform.position = startPoint.transform.position;
                enemyRB.transform.rotation = startPoint.transform.rotation;
            }
            else
            {
                enemyObjRef.transform.position = startPoint.transform.position;
                enemyObjRef.transform.rotation = startPoint.transform.rotation;
            }
        }

        // 4. Mover al jugador al punto de inicio (usamos GetPlayerTransform() para la referencia fresca)
        Transform playerTransform = GetPlayerTransform();
        if (playerTransform != null)
        {
            // ... (Tu lógica de reubicación del jugador es correcta)
            Vector3 destination = startPoint.transform.position + startPoint.transform.forward * 30f;
            playerTransform.GetComponent<PlayerMovement>()?.TeleportTo(destination);
        }

        // 5. Iniciar la lógica del paso
        Debug.Log($"[REINICIO] Regresando al paso: {stepId}");
        // Esto llama a GoToStep, que re-habilita al Jugador y al NPC (llama a StartChase).
        GoToStep(stepId);
    }

    public void PlayerCaught()
    {
        if (current.StepId == "M1-08")
        {
            // Detener chequeo de éxito si existe
            if (successCheckRoutine != null)
            {
                StopCoroutine(successCheckRoutine);
                successCheckRoutine = null;
            }

            // 1. Mostrar mensaje de fallo
            TempMessageDisplay.Instance.ShowMessage("¡TE HAN ATRAPADO! Reiniciando...");

            // 2. Iniciar el retraso para el reinicio
            StartCoroutine(RestartAfterDelay(1.5f, current.StepId));
        }
    }

    // CORRUTINA: Para que el mensaje se muestre antes de reiniciar
    private IEnumerator RestartAfterDelay(float delay, string stepId)
    {
        // Deshabilitar el control del jugador para evitar movimiento durante el mensaje
        GetPlayerTransform()?.GetComponent<PlayerMovement>()?.DisableControls();

        yield return new WaitForSeconds(delay);

        // Reiniciar el paso después del mensaje
        RestartFromStep(stepId);
    }
}