using Godot;

namespace HiddenWatts.Scripts.Gameplay;

public partial class ApplianceIndicator : Node2D
{
	public Vector2 DeviceSize { get; set; }

	public Vector2 DeviceOrigin { get; set; }

	public bool Nearby { get; set; }

	public bool Resolved { get; set; }

	public bool Checked { get; set; }

	private float elapsedTime;

	public override void _Process(double delta)
	{
		elapsedTime += (float)delta;
		QueueRedraw();
	}

	private Color GetIndicatorColor()
	{
		if (Resolved)
		{
			return new Color("a4cc87");
		}

		return new Color(Checked ? "b4bdb1" : "ffe29b");
	}

	public override void _Draw()
	{
		var rect = new Rect2(DeviceOrigin, DeviceSize);
		Color color = GetIndicatorColor();
		float pulse = 0.72f + 0.18f * Mathf.Sin(elapsedTime * 2.4f);

		if (!Resolved && !Checked)
		{
			for (int i = 7; i >= 2; i--)
			{
				DrawRect(rect.Grow(i), new Color(color, (Nearby ? 0.035f : 0.018f) * pulse), false, 2);
			}
		}

		float opacity = Resolved || Checked ? 0.25f : 0.45f * pulse;

		if (Nearby)
		{
			opacity = 0.9f;
		}

		DrawRect(rect.Grow(1), new Color(color, opacity), false, 0.7f);

		if (Resolved)
		{
			Vector2 top = DeviceOrigin + new Vector2(DeviceSize.X - 2, -4);
			DrawLine(top + new Vector2(-5, 0), top + new Vector2(-2, 3), color, 1.5f);
			DrawLine(top + new Vector2(-2, 3), top + new Vector2(3, -3), color, 1.5f);
		}
	}
}
