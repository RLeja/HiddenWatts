using System.Collections.Generic;
using System.Linq;
using System;
using HiddenWatts.Scripts.Gameplay.Data;

namespace HiddenWatts.Scripts.Gameplay;

public sealed class HouseholdCase
{
	public const int PointsPerProblem = 100;

	public const int UnusedInspectionBonus = 10;

	public const int WrongActionPenalty = 50;

	public const int UnhelpfulInspectionPenalty = 10;

	private readonly Dictionary<string, ApplianceData> appliancesById;
	private readonly HashSet<string> inspected = new();
	private readonly HashSet<string> resolved = new();
	private readonly HashSet<string> cleared = new();
	private readonly HashSet<(string ApplianceId, int ActionIndex)> attempts = new();
	private CaseResult result;

	public ScenarioData Scenario { get; }

	public IReadOnlyList<ApplianceData> Appliances => Scenario.Appliances;

	public IReadOnlySet<string> Inspected => inspected;

	public IReadOnlySet<string> Resolved => resolved;

	public IReadOnlySet<string> Cleared => cleared;

	public int WrongActions { get; private set; }

	public int InspectionsRemaining => Scenario.InspectionBudget - inspected.Count;

	public bool Finished => result != null;

	public bool Solved => resolved.Count >= Scenario.RequiredFindings;

	public HouseholdCase(ScenarioData scenario)
	{
		Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
		appliancesById = scenario.Appliances.ToDictionary(appliance => appliance.Id);
	}

	public bool Inspect(string applianceId)
	{
		if (Finished || !appliancesById.ContainsKey(applianceId))
		{
			return false;
		}

		if (inspected.Contains(applianceId))
		{
			return true;
		}

		if (InspectionsRemaining == 0)
		{
			return false;
		}

		inspected.Add(applianceId);
		return true;
	}

	public bool WasActionTried(string applianceId, int actionIndex)
	{
		return attempts.Contains((applianceId, actionIndex));
	}

	public ActionOutcome ApplyAction(string applianceId, int actionIndex)
	{
		if (!CanApplyAction(applianceId, actionIndex))
		{
			return new ActionOutcome(false, false, "This action is not available.");
		}

		ApplianceData appliance = appliancesById[applianceId];
		ActionData action = appliance.Actions[actionIndex];
		attempts.Add((applianceId, actionIndex));

		if (!action.IsCorrect)
		{
			WrongActions++;
			return new ActionOutcome(true, false, $"That action does not address what the readings show. Incorrect action: -{WrongActionPenalty} points. Review the history before trying another fix.");
		}

		if (!appliance.MajorProblem)
		{
			cleared.Add(applianceId);
			return new ActionOutcome(true, true, "Checked. This routine is reasonable and does not explain the bill increase. Look for devices with a larger change in use.");
		}

		resolved.Add(applianceId);
		string feedback = $"Problem resolved. Estimated avoidable use reduced by {appliance.DailySavingsKwh:0.00} kWh/day. {resolved.Count}/{Scenario.RequiredFindings} major problems found.";
		return new ActionOutcome(true, true, feedback);
	}

	private bool CanApplyAction(string applianceId, int actionIndex)
	{
		if (Finished || !appliancesById.TryGetValue(applianceId, out ApplianceData appliance))
		{
			return false;
		}

		if (!inspected.Contains(applianceId) || resolved.Contains(applianceId) || cleared.Contains(applianceId))
		{
			return false;
		}

		return actionIndex >= 0 && actionIndex < appliance.Actions.Count && !WasActionTried(applianceId, actionIndex);
	}

	public CaseResult Submit()
	{
		if (Finished)
		{
			return result;
		}

		int unhelpfulInspections = Appliances.Count(appliance => inspected.Contains(appliance.Id) && !appliance.MajorProblem);
		int inspectionBonus = resolved.Count > 0 ? InspectionsRemaining * UnusedInspectionBonus : 0;
		int score = resolved.Count * PointsPerProblem + inspectionBonus
			- WrongActions * WrongActionPenalty - unhelpfulInspections * UnhelpfulInspectionPenalty;
		double dailySavings = Appliances.Where(appliance => resolved.Contains(appliance.Id)).Sum(appliance => appliance.DailySavingsKwh);
		double monthlySavings = dailySavings * 30;
		result = new CaseResult
		{
			Correct = resolved.Count,
			Wrong = WrongActions,
			InspectionsUsed = inspected.Count,
			Score = Math.Max(0, score),
			MonthlyKwh = monthlySavings,
			MonthlyCost = monthlySavings * Scenario.PricePerKwh,
			MonthlyEmissions = monthlySavings * Scenario.EmissionsPerKwh
		};
		return result;
	}
}
