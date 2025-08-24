using System;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    EnterZone, Dialogue, TakePhoto, Interact, Pickup,
    ShootTarget, ReachZone, Chase, SolveGraph, Stealth, MinorPuzzle, Unknown
}

[Serializable]
public class MissionEventReq
{
    public TriggerType Type;
    public string TargetId;
    public bool Completed;

    public override string ToString() => $"{Type}:{TargetId} [{(Completed ? "?" : " ")}]";
}

[Serializable]
public class MissionStep
{
    public string Mission;                 // "M1 - El Coche Fantasma"
    public string StepId;                  // "M1-01"
    public string StepTitle;               // "Localiza el coche..."
    public string OnScreenText;            // Texto para HUD
    public string LocationZone;            // Zona sugerida (opcional)
    public List<MissionEventReq> Requirements = new(); // Eventos requeridos
    public List<string> RequiredItems = new();         // Para validaciones extra si quieres

    public string ChoiceBranchRaw;         // "A / B" (si aplica)
    public string NextOnSuccess;           // StepId siguiente (rama A / default)
    public string NextOnAlt;               // StepId alternativa (rama B)

    public string AddsFlagsRaw;            // "FLAG=1; FLAG2=0" (comentarios “if …” ignorados)
    public string RemovesFlagsRaw;
    public float SuspicionDelta;           // Del campo "increments" (ej. +0.2)
    public string Rewards;
    public string FailureCondition;
    public string Comments;

    public bool IsChoiceStep => !string.IsNullOrWhiteSpace(ChoiceBranchRaw);

    public override string ToString() => $"{StepId} - {StepTitle}";
}

public static class MissionParsers
{
    public static TriggerType ParseTrigger(string raw)
    {
        raw = raw.Trim().ToLower();
        return raw switch
        {
            "enterzone" => TriggerType.EnterZone,
            "dialogue" => TriggerType.Dialogue,
            "takephoto" => TriggerType.TakePhoto,
            "interact" => TriggerType.Interact,
            "pickup" => TriggerType.Pickup,
            "shoottarget" => TriggerType.ShootTarget,
            "reachzone" => TriggerType.ReachZone,
            "chase" => TriggerType.Chase,
            "solvegraph" => TriggerType.SolveGraph,
            "stealth" => TriggerType.Stealth,
            "minorpuzzle" => TriggerType.MinorPuzzle,
            _ => TriggerType.Unknown
        };
    }

    public static List<TriggerType> ParseTriggersList(string triggerCell)
    {
        // Soporta "Interact + Pickup + TakePhoto"
        var list = new List<TriggerType>();
        foreach (var t in triggerCell.Split('+'))
        {
            var tt = ParseTrigger(t);
            if (tt != TriggerType.Unknown) list.Add(tt);
        }
        return list;
    }

    public static List<string> ParseListByComma(string cell)
    {
        var list = new List<string>();
        foreach (var s in cell.Split(','))
        {
            var v = s.Trim();
            if (!string.IsNullOrEmpty(v)) list.Add(v);
        }
        return list;
    }

    public static float ParseSuspicion(string cell)
    {
        // Ej: "Sospecha:+0.2" -> 0.2
        if (string.IsNullOrWhiteSpace(cell)) return 0f;
        int idx = cell.IndexOf(':');
        if (idx >= 0 && float.TryParse(cell[(idx + 1)..].Replace('+', ' ').Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float val))
            return val;
        return 0f;
    }
}
