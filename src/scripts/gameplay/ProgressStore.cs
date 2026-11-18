using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

namespace HiddenWatts.Scripts.Gameplay;

public static class ProgressStore
{
    private const string SavePath = "user://progress.json";
    private static Dictionary<string, Record> records;

    public sealed class Record
    {
        public int Score { get; set; }
        public string Medal { get; set; } = "Unranked";
        public int Completions { get; set; }
    }

    public static Record Get(string caseId)
    {
        Load();
        return records.GetValueOrDefault(caseId) ?? new Record();
    }

    public static int CompletedCaseCount(string levelId)
    {
        return ScenarioCatalog.Scenarios.Count(item => item.LevelId == levelId && Get(item.Id).Completions > 0);
    }

    public static bool IsLevelCompleted(string levelId)
    {
        return CompletedCaseCount(levelId) > 0;
    }

    public static string LevelLabel(HiddenWatts.Scripts.Gameplay.Data.LevelData level)
    {
        return level.Label + (IsLevelCompleted(level.Id) ? " / Completed" : "");
    }

    public static string CompletionSummary => $"Households completed: {ScenarioCatalog.Levels.Count(level => IsLevelCompleted(level.Id))}/{ScenarioCatalog.Levels.Count}";

    public static void Save(string caseId, CaseResult result, bool solved)
    {
        Load();
        Record previous = Get(caseId);
        result.PreviousBest = previous.Score;
        result.IsNewBest = solved && result.Score > previous.Score;

        if (!solved)
        {
            return;
        }
        records[caseId] = new Record
        {
            Score = Math.Max(previous.Score, result.Score),
            Medal = result.Score >= previous.Score ? result.Medal : previous.Medal,
            Completions = previous.Completions + 1
        };

        try
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);

            if (file == null)
            {
                GD.PushWarning("Could not save personal bests.");
                return;
            }
            file.StoreString(JsonSerializer.Serialize(records));
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Could not save personal bests: {exception.Message}");
        }
    }

    private static void Load()
    {
        if (records != null)
        {
            return;
        }
        records = new Dictionary<string, Record>();

        if (!FileAccess.FileExists(SavePath))
        {
            return;
        }

        try
        {
            records = JsonSerializer.Deserialize<Dictionary<string, Record>>(FileAccess.GetFileAsString(SavePath)) ?? records;
        }
        catch (JsonException)
        {
            GD.PushWarning("Saved records could not be read. Starting a fresh record book.");
        }
    }
}
