using System;
using System.Collections.Generic;
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

    #region Singleton simple (opcional)
    public static MissionManager I { get; private set; }
    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
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

            if (step.StepId == "M1-03")
            {
                Debug.Log($"[CSV Loader] Paso M1-03 leído. Triggers: {triggers.Count} | Targets: {targets.Count}");
                Debug.Log($"[CSV Loader] Contenido crudo de 'target_ids': '{Cell(row, "target_ids")}'");
            }

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
            // Si hay triggers sin target (o viceversa), lo reportamos:
            if (triggers.Count != targets.Count)
                Debug.LogWarning($"[{step.StepId}] Triggers({triggers.Count}) != Targets({targets.Count}). Revisa el CSV.");

            if (!string.IsNullOrEmpty(step.StepId))
                stepsById[step.StepId] = step;
        }

        Debug.Log($"MissionManager: cargadas {stepsById.Count} filas del CSV.");
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

        // Aplica delta de sospecha al ENTER del paso (si así lo quieres)
        if (Mathf.Abs(current.SuspicionDelta) > 0.0001f)
        {
            suspicion = Mathf.Max(0f, suspicion + current.SuspicionDelta);
            // TODO: actualizar UI de sospecha si existe
        }

        // Flags (solo las no condicionales con formato FLAG=1 / FLAG=0)
        ApplyFlags(current.AddsFlagsRaw, add: true);
        ApplyFlags(current.RemovesFlagsRaw, add: false);

        // HUD
        if (objectiveHUD) objectiveHUD.SetText(current.OnScreenText);

        Debug.Log($"[MISSION] {current.StepId} ? {current.StepTitle}");
    }

    private void ApplyFlags(string flagsRaw, bool add)
    {
        if (string.IsNullOrWhiteSpace(flagsRaw)) return;
        // Formato esperado: "FLAG=1; OTRAFLAG=0" (líneas con "if" se ignoran)
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

    // === API pública para que el juego reporte eventos ===
    
    public void ReportEvent(TriggerType type, string targetId)
    {
        if (current == null) return;

        // === LÍNEAS DE DEPURACIÓN ===
        Debug.Log($"[MissionManager] Recibido evento: Tipo '{type}', Objetivo '{targetId}'.");
        Debug.Log($"[MissionManager] Paso de misión actual: {current.StepId}. Requisitos: {current.Requirements.Count}");

        // Marca requerimiento cumplido en la lista
        bool anyMatched = false;
        foreach (var req in current.Requirements)
        {
            if (!req.Completed && req.Type == type && string.Equals(req.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
            {
                req.Completed = true;
                _satisfiedForThisStep.Add($"{type}:{targetId}");
                anyMatched = true;
                break; // Salimos del bucle una vez que encontramos una coincidencia
            }
        }

        if (!anyMatched)
        {
            Debug.LogWarning($"[MissionManager] Evento reportado no coincide con ningún requisito del paso actual.");
            return;
        }

        // AHORA verificamos si todos los requisitos han sido cumplidos
        // comparando el número de requisitos satisfechos con el total.
        if (_satisfiedForThisStep.Count == current.Requirements.Count)
        {
            // Si NO es paso de elección, avanzar directo por NextOnSuccess
            if (!current.IsChoiceStep)
            {
                AdvanceTo(current.NextOnSuccess);
            }
            else
            {
                // Paso con elección: espera SubmitChoice(..)
                if (objectiveHUD) objectiveHUD.SetText(current.OnScreenText + " (elige una opción)");
            }
        }
    }

    // Para pasos con Choice (ej. "ENTREGAR / GUARDAR")
    public void SubmitChoice(int optionIndex)
    {
        if (current == null || !current.IsChoiceStep) return;
        string next = optionIndex == 0 ? current.NextOnSuccess : current.NextOnAlt;
        AdvanceTo(next);
    }

    private void AdvanceTo(string nextId)
    {
        if (string.IsNullOrEmpty(nextId))
        {
            Debug.Log("Paso finalizado sin siguiente step. Fin de misión/capítulo.");
            if (objectiveHUD) objectiveHUD.SetText("Misión actual completa.");
            return;
        }
        GoToStep(nextId);
    }

    // Utilidad por si necesitas consultar flags/sospecha
    public bool HasFlag(string flag) => flags.Contains(flag);
    public float GetSuspicion() => suspicion;
}
