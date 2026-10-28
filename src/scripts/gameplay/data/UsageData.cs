namespace HiddenWatts.Scripts.Gameplay.Data;

public sealed class UsageData
{
    public int Watts { get; init; }

    public string OperatingTime { get; init; } = "";

    public string RecentChange { get; init; } = "";

    public string Overnight { get; init; } = "";

    public string Standby { get; init; } = "";

    public string[] History { get; init; } = System.Array.Empty<string>();

    public double[] DailyKwh { get; init; } = System.Array.Empty<double>();
}
