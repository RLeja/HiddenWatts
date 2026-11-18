using System.Collections.Generic;

namespace HiddenWatts.Scripts.Gameplay.Data;

public sealed class SituationData
{
    public string Id { get; init; } = "";
    public string Observation { get; init; } = "";
    public string PreInspectionHint { get; init; } = "";
    public string Evidence { get; init; } = "";
    public bool MajorProblem { get; init; }
    public double DailySavingsKwh { get; init; }
    public UsageData Usage { get; init; } = new();
    public List<ActionData> Actions { get; init; } = new();
}
