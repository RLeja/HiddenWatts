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
    private static readonly Dictionary<string, string> lastRandomCases = new();

    public static IReadOnlyList<ScenarioData> Scenarios => scenarios;

    public static IReadOnlyList<LevelData> Levels { get; private set; } = Array.Empty<LevelData>();

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
        using JsonDocument document = JsonDocument.Parse(json);
        List<ScenarioData> loadedScenarios;

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            loadedScenarios = JsonSerializer.Deserialize<List<ScenarioData>>(json);
        }
        else
        {
            CampaignData campaign = JsonSerializer.Deserialize<CampaignData>(json)
                ?? throw new InvalidDataException("Campaign data is missing.");
            Require(campaign.Levels.Count > 0, "At least one level is required.");
            Require(campaign.Levels.Select(level => level.Id).Distinct().Count() == campaign.Levels.Count, "Level IDs must be unique.");
            Levels = campaign.Levels.OrderBy(level => level.Number).ToArray();
            loadedScenarios = new List<ScenarioData>();

            foreach (LevelData level in Levels)
            {
                Require(!string.IsNullOrWhiteSpace(level.Id) && level.Number > 0, "Levels need an ID and positive number.");
                Require(double.IsFinite(level.ScoreMultiplier) && level.ScoreMultiplier > 0, "Score multipliers must be positive.");
                Require(level.Cases.Count > 0, $"{level.Id} needs cases.");
                Require(level.Electronics.Select(device => device.Id).Distinct().Count() == level.Electronics.Count, $"Duplicate electronic in {level.Id}.");

                foreach (ElectronicData device in level.Electronics)
                {
                    Require(device.Situations.Count > 0 && device.Situations.Any(state => state.Id == "Normal"), $"{device.Id} needs a Normal situation.");
                    Require(device.Situations.Select(state => state.Id).Distinct().Count() == device.Situations.Count, $"Duplicate situation for {device.Id}.");

                    foreach (SituationData state in device.Situations)
                    {
                        ValidateAppliance(Resolve(device, state), $"{level.Id}/{device.Id}/{state.Id}: ");
                    }
                }

                for (int index = 0; index < level.Cases.Count; index++)
                {
                    CaseData item = level.Cases[index];
                    Require(item.Situations.Keys.All(id => level.Electronics.Any(device => device.Id == id)), $"{item.Id} references an unknown electronic.");
                    var appliances = new List<ApplianceData>();

                    foreach (ElectronicData device in level.Electronics)
                    {
                        string stateId = item.Situations.GetValueOrDefault(device.Id, "Normal");
                        SituationData state = device.Situations.FirstOrDefault(value => value.Id == stateId)
                            ?? throw new InvalidDataException($"{item.Id}: unknown situation {device.Id}/{stateId}.");
                        appliances.Add(Resolve(device, state));
                    }
                    loadedScenarios.Add(new ScenarioData
                    {
                        Id = item.Id,
                        Number = index + 1,
                        Title = item.Title,
                        LevelId = level.Id,
                        LevelNumber = level.Number,
                        LevelName = level.Name,
                        LevelScene = level.Scene,
                        Difficulty = level.Difficulty,
                        ScoreMultiplier = level.ScoreMultiplier,
                        Summary = item.Summary,
                        Brief = item.Brief,
                        InspectionBudget = level.InspectionBudget,
                        RequiredFindings = level.RequiredFindings,
                        PricePerKwh = level.PricePerKwh,
                        EmissionsPerKwh = level.EmissionsPerKwh,
                        Appliances = appliances
                    });
                }
            }
        }

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

    private static ApplianceData Resolve(ElectronicData device, SituationData state)
    {
        return new ApplianceData
        {
            Id = device.Id,
            Name = device.Name,
            Room = device.Room,
            NodePath = device.NodePath,
            Observation = state.Observation,
            PreInspectionHint = state.PreInspectionHint,
            Evidence = state.Evidence,
            MajorProblem = state.MajorProblem,
            DailySavingsKwh = state.DailySavingsKwh,
            Usage = state.Usage,
            Actions = state.Actions
        };
    }

    public static ScenarioData SelectRandomScenario(string levelId = null)
    {
        levelId ??= SelectedScenario?.LevelId ?? scenarios.First().LevelId;
        string previousId = lastRandomCases.GetValueOrDefault(levelId);
        List<ScenarioData> choices = scenarios.Where(scenario => scenario.LevelId == levelId && scenario.Id != previousId && scenario.Id != SelectedScenario?.Id).ToList();

        if (choices.Count == 0)
        {
            choices = scenarios.Where(scenario => scenario.LevelId == levelId).ToList();
        }
        SelectedScenario = choices[Random.Shared.Next(choices.Count)];
        lastRandomCases[levelId] = SelectedScenario.Id;
        return SelectedScenario;
    }

    public static bool HasNextLevel => scenarios.Any(item => item.LevelNumber > GetSelectedScenario().LevelNumber);

    public static ScenarioData SelectNextLevel()
    {
        int next = GetSelectedScenario().LevelNumber + 1;
        return SelectRandomScenario(scenarios.First(item => item.LevelNumber == next).LevelId);
    }

    public static ScenarioData SelectScenario(string id)
    {
        SelectedScenario = scenarios.FirstOrDefault(scenario => scenario.Id == id)
            ?? throw new ArgumentException($"Unknown case: {id}", nameof(id));
        return SelectedScenario;
    }

    public static ScenarioData GetSelectedScenario()
    {
        return SelectedScenario ?? SelectScenario(scenarios.First().Id);
    }

    private static void ValidateScenario(ScenarioData scenario)
    {
        string context = $"Case '{scenario.Id}': ";
        Require(!string.IsNullOrWhiteSpace(scenario.Title), context + "Title is required.");
        Require(!string.IsNullOrWhiteSpace(scenario.LevelName), context + "LevelName is required.");
        Require(!string.IsNullOrWhiteSpace(scenario.LevelScene) && scenario.LevelScene.StartsWith("res://scenes/levels/") && scenario.LevelScene.EndsWith(".tscn"), context + "LevelScene must reference a household scene.");
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

