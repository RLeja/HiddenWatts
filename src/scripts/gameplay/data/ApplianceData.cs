using System.Collections.Generic;
using System.Linq;

namespace HiddenWatts.Scripts.Gameplay.Data;

public sealed class ApplianceData
{
    public string Id { get; init; } = "";

    public string Name { get; init; } = "";

    public string Room { get; init; } = "";

    public string NodePath { get; init; } = "";

    public string Observation { get; init; } = "";

    public string PreInspectionHint { get; init; } = "";

    public string Evidence { get; init; } = "";

    public bool MajorProblem { get; init; }

    public double DailySavingsKwh { get; init; }

    public UsageData Usage { get; init; } = new();

    public List<ActionData> Actions { get; init; } = new();

    public ActionData CorrectAction => Actions.Single(action => action.IsCorrect);
}
