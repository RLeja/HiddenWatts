using System.Collections.Generic;

namespace HiddenWatts.Scripts.Gameplay.Data;

public sealed class LevelData
{
    public string Id { get; init; } = "";
    public int Number { get; init; }
    public string Name { get; init; } = "";
    public string Difficulty { get; init; } = "";
    public string Scene { get; init; } = "";
    public int InspectionBudget { get; init; }
    public int RequiredFindings { get; init; }
    public double ScoreMultiplier { get; init; } = 1;
    public double PricePerKwh { get; init; } = 0.25;
    public double EmissionsPerKwh { get; init; } = 0.20;
    public List<ElectronicData> Electronics { get; init; } = new();
    public List<CaseData> Cases { get; init; } = new();

    public string Label => $"{Number}. {Name} / {Difficulty}";
}
