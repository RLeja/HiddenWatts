using System.Collections.Generic;

namespace HiddenWatts.Scripts.Gameplay.Data;

public sealed class ElectronicData
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Room { get; init; } = "";
    public string NodePath { get; init; } = "";
    public List<SituationData> Situations { get; init; } = new();
}
