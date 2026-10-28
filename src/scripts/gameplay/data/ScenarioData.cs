using System.Collections.Generic;

namespace HiddenWatts.Scripts.Gameplay.Data;

public sealed class ScenarioData
{
    public string Id { get; init; } = "";

    public int Number { get; init; }

    public string Title { get; init; } = "";

    public string Summary { get; init; } = "";

    public string Brief { get; init; } = "";

    public int InspectionBudget { get; init; }

    public int RequiredFindings { get; init; }

    public double PricePerKwh { get; init; }

    public double EmissionsPerKwh { get; init; }

    public List<ApplianceData> Appliances { get; init; } = new();

    public string CaseLabel => $"Case {Number:00} / {Title}";
}
