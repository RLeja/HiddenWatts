using Godot;

namespace HiddenWatts.Scripts.Levels;

public partial class HouseLevel : Node2D
{
	private Camera2D _overview;
	private Rect2 _bounds;

	public override void _Ready()
	{
		_overview = GetNode<Camera2D>("Camera2D");
		Rect2I cells = GetNode<TileMapLayer>("Tiles/Ground").GetUsedRect();
		cells = cells.Merge(GetNode<TileMapLayer>("Tiles/Walls").GetUsedRect());
		cells = cells.Merge(GetNode<TileMapLayer>("Tiles/OuterWallFrame").GetUsedRect());
		Vector2 tileSize = GetNode<TileMapLayer>("Tiles/Ground").TileSet.TileSize;
		_bounds = new Rect2((Vector2)cells.Position * tileSize, (Vector2)cells.Size * tileSize);
		GetViewport().SizeChanged += FitOverview;
		FitOverview();
		_overview.MakeCurrent();
	}

	private void FitOverview()
	{
		Vector2 available = GetViewportRect().Size;
		Vector2 framedSize = _bounds.Size + Vector2.One * 16.0f;
		if (available.X <= 0 || available.Y <= 0)
			return;
		float zoom = Mathf.Min(available.X / framedSize.X, available.Y / framedSize.Y);
		_overview.Position = _bounds.GetCenter();
		_overview.Zoom = Vector2.One * zoom;
	}

	public override void _ExitTree()
	{
		GetViewport().SizeChanged -= FitOverview;
	}
}
