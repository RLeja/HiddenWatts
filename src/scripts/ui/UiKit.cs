using System;
using Godot;

namespace HiddenWatts.Scripts.Ui;

public static class UiKit
{
    public static readonly Color Ink = new("342e25");

    public static readonly Color Muted = new("756d5d");

    public static readonly Color Accent = new("956623");

    private static FontFile uiFont;

    public static FontFile UiFont => uiFont ??= GD.Load<FontFile>("res://assets/fonts/AtkinsonHyperlegible/Regular.ttf");

    public static StyleBoxFlat Box(string color, int padding = 16)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(color),
            ContentMarginLeft = padding,
            ContentMarginRight = padding,
            ContentMarginTop = padding,
            ContentMarginBottom = padding
        };
    }

    public static Theme CreateTheme()
    {
        var theme = new Theme { DefaultFont = UiFont, DefaultFontSize = 14 };
        theme.SetFont("bold_font", "RichTextLabel", UiFont);
        theme.SetColor("font_color", "Label", Ink);
        theme.SetColor("default_color", "RichTextLabel", Ink);

        foreach (string type in new[] { "Button", "OptionButton" })
        {
            theme.SetFont("font", type, UiFont);
            theme.SetStylebox("normal", type, Box("e1d8c0", 10));
            theme.SetStylebox("hover", type, Box("d2c09a", 10));
            theme.SetStylebox("pressed", type, Box("c4ad7e", 10));
            theme.SetStylebox("disabled", type, Box("e7e1d3", 10));
            theme.SetStylebox("focus", type, new StyleBoxFlat
            {
                BgColor = Colors.Transparent,
                BorderColor = Accent,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2
            });
            theme.SetColor("font_color", type, Ink);
            theme.SetColor("font_hover_color", type, Ink);
            theme.SetColor("font_focus_color", type, Ink);
            theme.SetColor("font_pressed_color", type, Ink);
            theme.SetColor("font_hover_pressed_color", type, Ink);
            theme.SetColor("font_disabled_color", type, new Color("9b9588"));
        }
        theme.SetStylebox("panel", "PanelContainer", Box("f0e9d7"));

        foreach (string type in new[] { "TabContainer", "TabBar" })
        {
            theme.SetFont("font", type, UiFont);
            theme.SetStylebox("panel", type, Box("e7dfca", 8));
            theme.SetStylebox("tab_selected", type, Box("e7dfca", 10));
            theme.SetStylebox("tab_unselected", type, Box("d5ccb6", 10));
            theme.SetStylebox("tab_hovered", type, Box("ded2b8", 10));
            theme.SetColor("font_selected_color", type, Ink);
            theme.SetColor("font_unselected_color", type, Muted);
            theme.SetColor("font_hovered_color", type, Ink);
        }
        theme.SetConstant("side_margin", "TabContainer", 0);
        theme.SetConstant("separation", "VBoxContainer", 10);
        theme.SetConstant("separation", "HBoxContainer", 12);
        return theme;
    }

    public static T Fill<T>(T control) where T : Control
    {
        control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        return control;
    }

    public static Label Text(string text, int size = 14, bool wrap = true)
    {
        var label = new Label { Text = text, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontOverride("font", UiFont);
        label.AddThemeFontSizeOverride("font_size", size);

        if (wrap)
        {
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        }
        return label;
    }

    public static Button Button(string text, Action pressed)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 38) };
        button.Pressed += pressed;
        return button;
    }
}
