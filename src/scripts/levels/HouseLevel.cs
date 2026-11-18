using Godot;
using System.Linq;

namespace HiddenWatts.Scripts.Levels;

public partial class HouseLevel : Node2D
{
	private Camera2D overview;

	public Rect2 Bounds { get; private set; }

	public Vector2 SpawnPosition => GetNodeOrNull<Marker2D>("PlayerSpawn")?.Position ?? new Vector2(336, 408);

	public override void _Ready()
	{
		overview = GetNode<Camera2D>("Camera2D");
		AlignFurnitureSorting();
		var frame = GetNode<TileMapLayer>("Tiles/OuterWallFrame");
		Rect2I cells = frame.GetUsedRect().Merge(GetNode<TileMapLayer>("Tiles/Walls").GetUsedRect());
		Vector2 tileSize = frame.TileSet.TileSize;
		Bounds = new Rect2((Vector2)cells.Position * tileSize, (Vector2)cells.Size * tileSize);
		GetViewport().SizeChanged += FitOverview;
		FitOverview();
		overview.MakeCurrent();
	}

	private void AlignFurnitureSorting()
	{
		foreach (Node2D room in GetChildren().OfType<Node2D>())
		{
			if (room.GetChildren().OfType<Sprite2D>().Any())
			{
				room.YSortEnabled = true;
			}
		}

		foreach (Sprite2D furniture in FindChildren("*", "Sprite2D", true, false).OfType<Sprite2D>())
		{
			if (!furniture.GetChildren().OfType<StaticBody2D>().Any())
			{
				continue;
			}
			Rect2 bounds = furniture.GetRect();
			Vector2 shift = new Vector2(bounds.GetCenter().X, bounds.End.Y);
			Vector2 target = furniture.ToGlobal(shift);
			furniture.Offset -= shift;

			foreach (Node2D child in furniture.GetChildren().OfType<Node2D>())
			{
				child.Position -= shift;
			}
			furniture.GlobalPosition = target;
		}
	}

	private void FitOverview()
	{
		Vector2 available = GetViewportRect().Size;
		Vector2 framedSize = Bounds.Size + Vector2.One * 32;

		if (available.X <= 0 || available.Y <= 0)
		{
			return;
		}
		overview.Position = Bounds.GetCenter();
		overview.Zoom = Vector2.One * Mathf.Min(available.X / framedSize.X, available.Y / framedSize.Y);
	}

	public override void _ExitTree()
	{
		GetViewport().SizeChanged -= FitOverview;
	}
}
