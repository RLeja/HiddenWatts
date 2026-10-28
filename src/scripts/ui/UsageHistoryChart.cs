using System.Linq;
using Godot;

namespace HiddenWatts.Scripts.Ui;

public partial class UsageHistoryChart : Control
{
    public double[] Values { get; set; } = System.Array.Empty<double>();

    public override void _Draw()
    {
        if (Values.Length == 0)
        {
            return;
        }

        var font = UiKit.UiFont;
        float maximum = (float)Values.Max();
        float width = Size.X / Values.Length;
        float height = Size.Y - 32;
        DrawString(font, new Vector2(0, 12), $"Daily electricity use · peak {maximum:0.###} kWh", fontSize: 11, modulate: UiKit.Muted);

        for (int i = 0; i < Values.Length; i++)
        {
            float bar = maximum <= 0 ? 0 : (float)Values[i] / maximum * (height - 22);
            DrawRect(new Rect2(i * width + 3, height - bar, width - 10, bar), new Color(i == Values.Length - 1 ? "a27b3c" : "bdb39a"));
            DrawString(font, new Vector2(i * width + 3, Size.Y - 6), i == Values.Length - 1 ? "Today" : $"−{Values.Length - i - 1}d", fontSize: 10, modulate: UiKit.Muted);
        }
    }
}
