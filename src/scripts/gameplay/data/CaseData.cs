using System.Collections.Generic;

namespace HiddenWatts.Scripts.Gameplay.Data;

public sealed class CaseData
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Summary { get; init; } = "";
    public string Brief { get; init; } = "";
    public Dictionary<string, string> Situations { get; init; } = new();
}
