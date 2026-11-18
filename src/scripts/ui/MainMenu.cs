using Godot;
using System.Linq;
using HiddenWatts.Scripts.Gameplay.Data;
using HiddenWatts.Scripts.Gameplay;

namespace HiddenWatts.Scripts.Ui;

public partial class MainMenu : Control
{
	private Control caseFilePage;
	private Button startButton;
	private ScenarioData scenario;
	private Label caseLabel;
	private Label summaryLabel;
	private Label captionText;
	private SubViewportContainer housePreview;
	private OptionButton caseSelector;
	private Label recordLabel;
	private bool randomSituation = true;

	[Export]
	public Godot.Json ScenarioFile { get; set; }

	public override void _Ready()
	{
		ScenarioCatalog.Initialize(ScenarioFile);
		scenario = ScenarioCatalog.GetSelectedScenario();
		Theme = UiKit.CreateTheme();
		AddChild(UiKit.Fill(new ColorRect { Color = new Color("252d28"), MouseFilter = MouseFilterEnum.Ignore }));
		BuildHousePreview();
		BuildPaper();
		BuildMenu();
		BuildCaption();
		startButton.GrabFocus();
	}

	private void BuildPaper()
	{
		var paper = new ColorRect { Color = new Color("eee5ce"), MouseFilter = MouseFilterEnum.Ignore };
		AddChild(paper);
		paper.AnchorRight = 0.365f;
		paper.AnchorBottom = 1;
		var edge = new ColorRect { Color = new Color("aa8e58"), MouseFilter = MouseFilterEnum.Ignore };
		AddChild(edge);
		edge.AnchorLeft = 0.365f;
		edge.AnchorRight = 0.365f;
		edge.AnchorBottom = 1;
		edge.OffsetRight = 3;
	}

	private void BuildMenu()
	{
		var margin = new MarginContainer();
		AddChild(margin);
		margin.AnchorRight = 0.365f;
		margin.AnchorBottom = 1;
		margin.OffsetLeft = 36;
		margin.OffsetRight = -34;
		margin.OffsetTop = 34;
		margin.OffsetBottom = -28;
		var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
		margin.AddChild(scroll);
		var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
		scroll.AddChild(column);
		column.AddThemeConstantOverride("separation", 5);
		var stamp = UiKit.Text("ENERGY INVESTIGATIONS", 12);
		stamp.AddThemeFontOverride("font", UiKit.UiFont);
		stamp.Modulate = UiKit.Muted;
		column.AddChild(stamp);
		var title = UiKit.Text("Hidden\nWatts", 56, false);
		title.AddThemeFontOverride("font", UiKit.UiFont);
		title.AddThemeConstantOverride("line_spacing", -8);
		column.AddChild(title);
		column.AddChild(new HSeparator());
		caseLabel = UiKit.Text(scenario.CaseLabel.ToUpperInvariant(), 13);
		column.AddChild(caseLabel);
		summaryLabel = UiKit.Text(scenario.Summary, 17);
		column.AddChild(summaryLabel);
		AddCaseSelector(column);
		column.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0, 4) });
		startButton = UiKit.Button("Begin investigation", BeginInvestigation);
		startButton.Name = "StartCase";
		startButton.Alignment = HorizontalAlignment.Left;
		startButton.AddThemeStyleboxOverride("normal", UiKit.Box("514a35", 14));
		startButton.AddThemeStyleboxOverride("hover", UiKit.Box("6c6040", 14));
		startButton.AddThemeStyleboxOverride("pressed", UiKit.Box("3c382a", 14));

		foreach (string state in new[] { "font_color", "font_hover_color", "font_focus_color", "font_pressed_color" })
		{
			startButton.AddThemeColorOverride(state, new Color("fff5dc"));
		}
		column.AddChild(startButton);
		var brief = UiKit.Button("Read the case file", ShowCaseFile);
		brief.Name = "CaseFile";
		brief.Alignment = HorizontalAlignment.Left;
		column.AddChild(brief);
		var quit = UiKit.Button("Quit", () => GetTree().Quit());
		quit.Alignment = HorizontalAlignment.Left;
		column.AddChild(quit);
		var controls = UiKit.Text("WASD / arrows to move\nE to examine a device", 11);
		controls.Modulate = UiKit.Muted;
		column.AddChild(controls);
	}

	private void AddCaseSelector(VBoxContainer column)
	{
		var levels = new OptionButton { Name = "LevelSelector", CustomMinimumSize = new Vector2(0, 38) };

		for (int index = 0; index < ScenarioCatalog.Levels.Count; index++)
		{
			LevelData level = ScenarioCatalog.Levels[index];
			levels.AddItem(ProgressStore.LevelLabel(level));

			if (level.Id == scenario.LevelId)
			{
				levels.Select(index);
			}
		}
		levels.ItemSelected += index =>
		{
			scenario = ScenarioCatalog.SelectScenario(ScenarioCatalog.Scenarios.First(item => item.LevelId == ScenarioCatalog.Levels[(int)index].Id).Id);
			randomSituation = true;
			PopulateCases();
			RefreshSelection();
		};
		column.AddChild(levels);
		caseSelector = new OptionButton { Name = "CaseSelector", CustomMinimumSize = new Vector2(0, 38) };
		PopulateCases();
		caseSelector.ItemSelected += index =>
		{
			ScenarioData[] cases = ScenarioCatalog.Scenarios.Where(item => item.LevelId == scenario.LevelId).ToArray();
			randomSituation = index == 0;

			if (!randomSituation)
			{
				scenario = ScenarioCatalog.SelectScenario(cases[(int)index - 1].Id);
			}
			RefreshSelection();
		};
		column.AddChild(caseSelector);
		recordLabel = UiKit.Text(RecordText(), 12);
		column.AddChild(recordLabel);
		RefreshSelectionText();
	}

	private void PopulateCases()
	{
		caseSelector.Clear();
		caseSelector.AddItem("Random situation");
		caseSelector.Select(0);
		ScenarioData[] cases = ScenarioCatalog.Scenarios.Where(item => item.LevelId == scenario.LevelId).ToArray();

		for (int index = 0; index < cases.Length; index++)
		{
			caseSelector.AddItem($"{cases[index].Number:00} / {cases[index].Title}");

			if (!randomSituation && cases[index].Id == scenario.Id)
			{
				caseSelector.Select(index + 1);
			}
		}
	}

	private string RecordText()
	{
		ProgressStore.Record record = ProgressStore.Get(scenario.Id);
		int caseCount = ScenarioCatalog.Scenarios.Count(item => item.LevelId == scenario.LevelId);
		string completion = ProgressStore.IsLevelCompleted(scenario.LevelId) ? "Household completed" : "Household not completed";
		string best = randomSituation ? $"{completion} / {ProgressStore.CompletedCaseCount(scenario.LevelId)}/{caseCount} cases solved" : $"Best: {record.Score} / {record.Medal}";
		return $"{scenario.RequiredFindings} problems / {scenario.InspectionBudget} inspections / x{scenario.ScoreMultiplier:0.##} score\n{best}\n{ProgressStore.CompletionSummary}";
	}

	private void RefreshSelection()
	{
		RefreshSelectionText();
		captionText.Text = scenario.LevelName.ToUpperInvariant();
		RemoveChild(housePreview);
		housePreview.QueueFree();
		BuildHousePreview();
		MoveChild(housePreview, 1);
	}

	private void RefreshSelectionText()
	{
		caseLabel.Text = randomSituation ? "RANDOM SITUATION" : scenario.CaseLabel.ToUpperInvariant();
		summaryLabel.Text = randomSituation ? "A household case will be assigned when you enter. Read the brief, then investigate." : scenario.Summary;
		recordLabel.Text = RecordText();
	}

	private void BeginInvestigation()
	{
		if (randomSituation)
		{
			scenario = ScenarioCatalog.SelectRandomScenario(scenario.LevelId);
		}
		GetTree().ChangeSceneToFile("res://scenes/core/game_manager.tscn");
	}

	private void BuildCaption()
	{
		var caption = new PanelContainer { MouseFilter = MouseFilterEnum.Ignore };
		caption.AddThemeStyleboxOverride("panel", UiKit.Box("333b30", 10));
		AddChild(caption);
		caption.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomRight);
		caption.OffsetLeft = -236;
		caption.OffsetRight = -28;
		caption.OffsetTop = -56;
		caption.OffsetBottom = -24;
		captionText = UiKit.Text(scenario.LevelName.ToUpperInvariant(), 12, false);
		captionText.AddThemeColorOverride("font_color", new Color("e5dcc4"));
		caption.AddChild(captionText);
	}

	public string GetCaseId() => scenario.Id;

	private void BuildHousePreview()
	{
		var container = new SubViewportContainer { Name = "HousePreview", Stretch = true, MouseFilter = MouseFilterEnum.Ignore };
		housePreview = container;
		AddChild(container);
		container.AnchorLeft = 0.365f;
		container.AnchorRight = 1;
		container.AnchorBottom = 1;
		var viewport = new SubViewport { Size = new Vector2I(730, 648), TransparentBg = true, GuiDisableInput = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
		container.AddChild(viewport);
		var house = GD.Load<PackedScene>(scenario.LevelScene).Instantiate<Node2D>();
		house.Modulate = new Color(0.83f, 0.82f, 0.75f);
		viewport.AddChild(house);
		var frames = new SpriteFrames();
		frames.RemoveAnimation("default");
		frames.AddAnimation("Idle");
		var texture = GD.Load<Texture2D>("res://assets/characters/Bob_idle_anim.png");

		for (int i = 0; i < 6; i++)
		{
			frames.AddFrame("Idle", new AtlasTexture { Atlas = texture, Region = new Rect2((18 + i) * 16, 0, 16, 32) });
		}
		frames.SetAnimationSpeed("Idle", 4);
		var investigator = new AnimatedSprite2D { Name = "Investigator", Position = house.GetNodeOrNull<Marker2D>("PlayerSpawn")?.Position ?? new Vector2(336, 408), Offset = new Vector2(-8, -32), Centered = false, SpriteFrames = frames, TextureFilter = TextureFilterEnum.Nearest };
		house.AddChild(investigator);
		investigator.Play("Idle");

		foreach (Sprite2D sprite in ElectronicDevice.ResolveAll(house, scenario.Appliances))
		{
			sprite.AddChild(new ApplianceIndicator { DeviceSize = sprite.GetRect().Size, DeviceOrigin = sprite.GetRect().Position, ZIndex = 5 });
		}
	}

	private void ShowCaseFile()
	{
		if (caseFilePage != null)
		{
			return;
		}

		caseFilePage = UiKit.Fill(new Control { Name = "CaseFilePage" });
		AddChild(caseFilePage);
		caseFilePage.AddChild(UiKit.Fill(new ColorRect { Color = new Color(0.07f, 0.06f, 0.04f, 0.75f) }));
		var panel = new PanelContainer();
		caseFilePage.AddChild(panel);
		panel.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
		panel.OffsetLeft = -310;
		panel.OffsetRight = 310;
		panel.OffsetTop = -250;
		panel.OffsetBottom = 250;
		var column = new VBoxContainer();
		panel.AddChild(column);
		column.AddChild(UiKit.Text(randomSituation ? "Your next investigation" : scenario.CaseLabel, 27));
		column.AddChild(UiKit.Text(randomSituation ? "A random case from this household will be assigned when you enter. Its full situation brief appears before you start exploring." : scenario.Brief, 17));
		column.AddChild(new HSeparator());
		column.AddChild(UiKit.Text("Your assignment", 20));
		column.AddChild(UiKit.Text("Enter the rooms and observe what is running. Approach a highlighted appliance and press E. Spend an inspection to read its wattage, operating hours and history, then decide whether any corrective action is justified."));
		column.AddChild(UiKit.Text($"Find and fix {scenario.RequiredFindings} major problems. Incorrect actions and unhelpful inspections reduce your score. The case ends when all required problems are resolved."));
		column.AddChild(UiKit.Text("WASD / arrows - move     E - examine\nN - collected notes     Esc - close a page or pause", 13));
		var close = UiKit.Button("Back", CloseCaseFile);
		column.AddChild(close);
		close.GrabFocus();
	}

	private void CloseCaseFile()
	{
		if (caseFilePage == null)
		{
			return;
		}

		caseFilePage.QueueFree();
		caseFilePage = null;
		startButton.GrabFocus();
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape && caseFilePage != null)
		{
			CloseCaseFile();
		}
	}
}
