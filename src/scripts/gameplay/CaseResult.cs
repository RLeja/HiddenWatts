namespace HiddenWatts.Scripts.Gameplay;

public sealed class CaseResult
{
	public int Correct { get; init; }

	public int Wrong { get; init; }

	public int InspectionsUsed { get; init; }

	public int Score { get; init; }

	public int MaximumScore { get; init; }

	public string Medal { get; init; }

	public int PreviousBest { get; set; }

	public bool IsNewBest { get; set; }

	public double MonthlyKwh { get; init; }

	public double MonthlyCost { get; init; }

	public double MonthlyEmissions { get; init; }
}
