using System.Collections.Generic;

namespace HiddenWatts.Scripts.Gameplay.Data;

public sealed class CampaignData
{
    public List<LevelData> Levels { get; init; } = new();
}
