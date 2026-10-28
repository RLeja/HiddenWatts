namespace HiddenWatts.Scripts.Gameplay;

public sealed class CaseResult
{
    public int Correct { get; init; }

    public int Wrong { get; init; }

    public int InspectionsUsed { get; init; }

    public int Score { get; init; }

    public double MonthlyKwh { get; init; }

    public double MonthlyCost { get; init; }

    public double MonthlyEmissions { get; init; }
}
