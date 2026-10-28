using Godot;

namespace HiddenWatts.Scripts.Core;

public partial class Globals : Node
{
	public static Globals Instance { get; private set; }
	[ExportCategory("GamePlay")]
	[Export]
	public int GridSize { get; set; } = 16;

	public override void _Ready()
	{
		Instance = this;
		Logger.Info("Loading globals..");
	}
}
