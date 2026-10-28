using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System;
using Godot;
using HiddenWatts.Scripts.Gameplay.Data;

namespace HiddenWatts.Scripts.Gameplay;

public static class ScenarioCatalog
{
    private static IReadOnlyList<ScenarioData> scenarios = Array.Empty<ScenarioData>();

    public static ScenarioData SelectedScenario { get; private set; }

    public static void Initialize(Godot.Json source)
    {
        if (scenarios.Count > 0)
        {
            return;
        }

        if (source == null)
        {
            source = GD.Load<Godot.Json>("res://data/scenarios.json");
        }

        if (source == null)
        {
            throw new InvalidDataException("The scenario data file could not be loaded.");
        }
        scenarios = ReadScenarios(Godot.FileAccess.GetFileAsString(source.ResourcePath));
    }

    public static IReadOnlyList<ScenarioData> ReadScenarios(string json)
    {
        var loadedScenarios = JsonSerializer.Deserialize<List<ScenarioData>>(json);

        if (loadedScenarios == null || loadedScenarios.Count == 0)
        {
            throw new InvalidDataException("The scenario file must contain at least one case.");
        }
        var ids = new HashSet<string>();

        foreach (ScenarioData scenario in loadedScenarios)
        {
            Require(scenario != null, "A case cannot be null.");
            Require(!string.IsNullOrWhiteSpace(scenario.Id) && ids.Add(scenario.Id), "Case IDs must be nonempty and unique.");
            ValidateScenario(scenario);
        }
        return loadedScenarios;
    }

    public static ScenarioData SelectRandomScenario()
    {
        if (scenarios.Count == 0)
        {
            throw new InvalidOperationException("Load the scenario catalog before selecting a case.");
        }
        List<ScenarioData> choices = scenarios.Where(scenario => scenario.Id != SelectedScenario?.Id).ToList();

        if (choices.Count == 0)
        {
            choices = scenarios.ToList();
        }
        SelectedScenario = choices[Random.Shared.Next(choices.Count)];
        return SelectedScenario;
    }

    public static ScenarioData GetSelectedScenario()
    {
        return SelectedScenario ?? SelectRandomScenario();
    }

    private static void ValidateScenario(ScenarioData scenario)
    {
        string context = $"Case '{scenario.Id}': ";
        Require(!string.IsNullOrWhiteSpace(scenario.Title), context + "Title is required.");
        Require(!string.IsNullOrWhiteSpace(scenario.Brief), context + "Brief is required.");
        Require(scenario.Number > 0, context + "Number must be positive.");
        Require(scenario.Appliances != null && scenario.Appliances.Count > 0, context + "At least one appliance is required.");
        Require(scenario.InspectionBudget > 0 && scenario.InspectionBudget <= scenario.Appliances.Count, context + "InspectionBudget is outside the appliance count.");
        Require(scenario.RequiredFindings > 0 && scenario.RequiredFindings <= scenario.InspectionBudget, context + "RequiredFindings must fit within the inspection budget.");
        Require(double.IsFinite(scenario.PricePerKwh) && scenario.PricePerKwh >= 0, context + "PricePerKwh must be nonnegative.");
        Require(double.IsFinite(scenario.EmissionsPerKwh) && scenario.EmissionsPerKwh >= 0, context + "EmissionsPerKwh must be nonnegative.");
        var applianceIds = new HashSet<string>();
        var paths = new HashSet<string>();

        foreach (ApplianceData appliance in scenario.Appliances)
        {
            Require(appliance != null, context + "An appliance cannot be null.");
            Require(!string.IsNullOrWhiteSpace(appliance.Id) && applianceIds.Add(appliance.Id), context + "Appliance IDs must be nonempty and unique.");
            Require(!string.IsNullOrWhiteSpace(appliance.NodePath) && paths.Add(appliance.NodePath), context + "Appliance node paths must be nonempty and unique.");
            ValidateAppliance(appliance, context + appliance.Id + ": ");
        }
        Require(scenario.RequiredFindings <= scenario.Appliances.Count(appliance => appliance.MajorProblem), context + "Not enough major problems to complete this case.");
    }

    private static void ValidateAppliance(ApplianceData appliance, string context)
    {
        Require(!string.IsNullOrWhiteSpace(appliance.Name), context + "Name is required.");
        Require(!string.IsNullOrWhiteSpace(appliance.Room), context + "Room is required.");
        Require(!string.IsNullOrWhiteSpace(appliance.Observation), context + "Observation is required.");
        Require(!string.IsNullOrWhiteSpace(appliance.PreInspectionHint), context + "PreInspectionHint is required.");
        Require(!string.IsNullOrWhiteSpace(appliance.Evidence), context + "Evidence is required.");
        Require(appliance.Actions != null && appliance.Actions.Count >= 2, context + "At least two actions are required.");
        Require(appliance.Actions.All(action => action != null && !string.IsNullOrWhiteSpace(action.Text)), context + "Actions need text.");
        Require(appliance.Actions.Count(action => action.IsCorrect) == 1, context + "Exactly one action must be correct.");
        Require(double.IsFinite(appliance.DailySavingsKwh) && appliance.DailySavingsKwh >= 0, context + "DailySavingsKwh must be nonnegative.");
        Require(appliance.MajorProblem == (appliance.DailySavingsKwh > 0), context + "Only major problems should have positive savings.");
        Require(appliance.Usage != null, context + "Usage readings are required.");
        Require(appliance.Usage.Watts > 0, context + "Watts must be positive.");
        Require(!string.IsNullOrWhiteSpace(appliance.Usage.OperatingTime), context + "OperatingTime is required.");
        Require(!string.IsNullOrWhiteSpace(appliance.Usage.RecentChange), context + "RecentChange is required.");
        Require(!string.IsNullOrWhiteSpace(appliance.Usage.Overnight), context + "Overnight is required.");
        Require(!string.IsNullOrWhiteSpace(appliance.Usage.Standby), context + "Standby is required.");
        Require(appliance.Usage.History != null && appliance.Usage.History.Length > 0, context + "History is required.");
        Require(appliance.Usage.History.All(entry => !string.IsNullOrWhiteSpace(entry)), context + "History cannot contain empty entries.");
        Require(appliance.Usage.DailyKwh != null && appliance.Usage.DailyKwh.Length == 7, context + "DailyKwh must contain seven daily readings.");
        Require(appliance.Usage.DailyKwh.All(value => double.IsFinite(value) && value >= 0), context + "DailyKwh readings must be finite and nonnegative.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}

