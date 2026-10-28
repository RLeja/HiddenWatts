using System.Linq;
using System;
using Godot;
using HiddenWatts.Scripts.Gameplay.Data;
using HiddenWatts.Scripts.Gameplay;

namespace HiddenWatts.Scripts.Ui;

public partial class GameHud : Control
{
    private Label status;
    private Label promptText;
    private PanelContainer prompt;
    private PanelContainer toast;
    private Label toastText;
    private float toastTime;
    private Control modal;
    private Label title;
    private Button closeButton;
    private VBoxContainer body;
    private ScrollContainer scroll;
    private HBoxContainer footer;
    private Button notesButton;

    public event Action InspectRequested;
    public event Action<int> ActionRequested;
    public event Action CloseRequested;
    public event Action NotesRequested;
    public event Action ResumeRequested;
    public event Action RestartRequested;
    public event Action MenuRequested;
    public event Action FinishRequested;

    public CasePanel OpenPanel { get; private set; } = CasePanel.None;

    public bool ModalOpen => OpenPanel != CasePanel.None;

    public override void _Ready()
    {
        Theme = UiKit.CreateTheme();
        MouseFilter = MouseFilterEnum.Ignore;
        BuildCounter();
        BuildNotesButton();
        BuildPrompt();
        BuildToast();
        BuildModal();
    }

    private void BuildCounter()
    {
        var counter = new PanelContainer
        {
            Name = "InspectionCounter",
            Position = new Vector2(16, 16)
        };
        counter.AddThemeStyleboxOverride("panel", UiKit.Box("efe7d3", 9));
        status = UiKit.Text("", 13, false);
        counter.AddChild(status);
        AddChild(counter);
    }

    private void BuildNotesButton()
    {
        notesButton = UiKit.Button("[N] Case notes", () => NotesRequested?.Invoke());
        notesButton.Name = "OpenNotes";
        notesButton.TooltipText = "Open collected evidence. Esc pauses the investigation.";
        AddChild(notesButton);
        notesButton.SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight);
        notesButton.OffsetLeft = -164;
        notesButton.OffsetRight = -16;
        notesButton.OffsetTop = 16;
        notesButton.OffsetBottom = 52;
    }

    private void BuildPrompt()
    {
        prompt = new PanelContainer
        {
            Name = "InteractionPrompt",
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(prompt);
        prompt.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterBottom);
        prompt.OffsetLeft = -220;
        prompt.OffsetRight = 220;
        prompt.OffsetTop = -64;
        prompt.OffsetBottom = -20;
        prompt.AddThemeStyleboxOverride("panel", UiKit.Box("efe7d3", 10));
        promptText = UiKit.Text("", 15, false);
        promptText.HorizontalAlignment = HorizontalAlignment.Center;
        prompt.AddChild(promptText);
    }

    private void BuildToast()
    {
        toast = new PanelContainer
        {
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(toast);
        toast.SetAnchorsAndOffsetsPreset(LayoutPreset.CenterTop);
        toast.OffsetLeft = -190;
        toast.OffsetRight = 190;
        toast.OffsetTop = 16;
        toast.OffsetBottom = 56;
        toastText = UiKit.Text("", 12);
        toast.AddChild(toastText);
    }

    private void BuildModal()
    {
        modal = UiKit.Fill(new Control
        {
            Name = "Modal",
            Visible = false,
            MouseFilter = MouseFilterEnum.Stop
        });
        AddChild(modal);
        modal.AddChild(UiKit.Fill(new ColorRect
        {
            Color = new Color(0.07f, 0.06f, 0.04f, 0.72f)
        }));
        var paper = new PanelContainer { Name = "CasePaper" };
        modal.AddChild(paper);
        paper.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        paper.OffsetLeft = -350;
        paper.OffsetRight = 350;
        paper.OffsetTop = -282;
        paper.OffsetBottom = 282;
        var stack = new VBoxContainer();
        var header = new HBoxContainer();
        paper.AddChild(stack);
        stack.AddChild(header);
        title = UiKit.Text("", 25);
        title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        header.AddChild(title);
        closeButton = UiKit.Button("Close [Esc]", () => CloseRequested?.Invoke());
        closeButton.Name = "ClosePanel";
        header.AddChild(closeButton);
        stack.AddChild(new HSeparator());
        scroll = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        stack.AddChild(scroll);
        body = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        scroll.AddChild(body);
        footer = new HBoxContainer();
        stack.AddChild(footer);
    }

    public override void _Process(double delta)
    {
        if (toastTime <= 0)
        {
            return;
        }

        toastTime -= (float)delta;

        if (toastTime <= 0)
        {
            toast.Visible = false;
        }
    }

    public void RefreshStatus(HouseholdCase session)
    {
        ScenarioData scenario = session.Scenario;
        status.Text = $"Inspections {session.InspectionsRemaining}/{scenario.InspectionBudget}   /   Problems {session.Resolved.Count}/{scenario.RequiredFindings}";
    }

    public void SetNearby(string applianceName, bool inspected)
    {
        prompt.Visible = applianceName != null && !ModalOpen;

        if (applianceName != null)
        {
            string action = inspected ? "Review" : "Examine";
            promptText.Text = $"[E] {action} {applianceName}";
        }
    }

    public void Message(string text)
    {
        toastText.Text = text;
        toast.Visible = true;
        toastTime = 5;
    }

    private void Open(CasePanel panel, string heading)
    {
        ClearChildren(body);
        ClearChildren(footer);
        OpenPanel = panel;
        modal.Visible = true;
        prompt.Visible = false;
        title.Text = heading;
        closeButton.Visible = panel != CasePanel.Results;
        scroll.ScrollVertical = 0;

        if (closeButton.Visible)
        {
            closeButton.GrabFocus();
        }
    }

    private static void ClearChildren(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            parent.RemoveChild(child);
            child.QueueFree();
        }
    }

    public void HideModal()
    {
        OpenPanel = CasePanel.None;
        modal.Visible = false;
    }

    public void ShowDevice(HouseholdCase session, int selected, string feedback = "")
    {
        ApplianceData appliance = session.Appliances[selected];
        Open(CasePanel.Device, appliance.Name);
        body.AddChild(UiKit.Text(appliance.Room + " / appliance examination", 12));

        if (!string.IsNullOrEmpty(feedback))
        {
            var note = UiKit.Text(feedback, 14);
            note.Modulate = new Color("725621");
            body.AddChild(note);
        }

        if (!session.Inspected.Contains(appliance.Id))
        {
            ShowUninspectedDevice(session, appliance);
            return;
        }

        ShowInspectedDevice(session, appliance, feedback);
    }

    private void ShowUninspectedDevice(HouseholdCase session, ApplianceData appliance)
    {
        body.AddChild(UiKit.Text(appliance.Observation, 17));
        body.AddChild(UiKit.Text("First impression", 18));
        var hint = UiKit.Text(appliance.PreInspectionHint);
        hint.Name = "PreInspectionHint";
        body.AddChild(hint);
        body.AddChild(UiKit.Text("These observations are free. They can help you choose what to investigate, but do not confirm how much electricity is being wasted.", 12));
        body.AddChild(new HSeparator());
        body.AddChild(UiKit.Text("A detailed inspection reveals power draw, operating hours and recent usage history. Decide whether this device is worth one of your remaining inspections."));
        var inspect = UiKit.Button($"[E] Inspect device / {session.InspectionsRemaining} inspections left", () => InspectRequested?.Invoke());
        inspect.Name = "InspectButton";
        inspect.Disabled = session.InspectionsRemaining == 0;
        body.AddChild(inspect);

        if (inspect.Disabled)
        {
            body.AddChild(UiKit.Text("No inspections remain. Use your saved readings to resolve the problems you have already investigated."));
        }
        body.AddChild(UiKit.Text("Closing this examination without inspecting costs nothing.", 12));
    }

    private void ShowInspectedDevice(HouseholdCase session, ApplianceData appliance, string feedback)
    {
        body.AddChild(UiKit.Text(GetDeviceStatus(session, appliance), 12));
        var tabs = new TabContainer
        {
            Name = "DeviceTabs",
            CustomMinimumSize = new Vector2(0, 355)
        };
        body.AddChild(tabs);
        BuildReadingsTab(tabs, appliance);
        BuildHistoryTab(tabs, appliance);
        BuildActionsTab(tabs, session, appliance);

        if (!string.IsNullOrEmpty(feedback))
        {
            tabs.CurrentTab = 2;
        }
    }

    private static string GetDeviceStatus(HouseholdCase session, ApplianceData appliance)
    {
        if (session.Resolved.Contains(appliance.Id))
        {
            return "RESOLVED / readings below were captured before the fix";
        }

        if (session.Cleared.Contains(appliance.Id))
        {
            return "CHECKED / no major problem found";
        }

        return "Inspection saved / reviewing it is free";
    }

    private static void BuildReadingsTab(TabContainer tabs, ApplianceData appliance)
    {
        VBoxContainer readings = CreateTab(tabs, "Readings");
        UsageData usage = appliance.Usage;
        var watts = UiKit.Text($"{usage.Watts:N0} W", 32);
        watts.Name = "PowerReading";
        readings.AddChild(watts);
        readings.AddChild(UiKit.Text("Power while active. Energy use also depends on running time.", 12));
        readings.AddChild(UiKit.Text("Operating time: " + usage.OperatingTime));
        readings.AddChild(UiKit.Text("Recent change: " + usage.RecentChange));
        readings.AddChild(UiKit.Text("Overnight: " + usage.Overnight));
        readings.AddChild(UiKit.Text("Standby: " + usage.Standby));
        readings.AddChild(new HSeparator());
        readings.AddChild(UiKit.Text(appliance.Evidence));
    }

    private static void BuildHistoryTab(TabContainer tabs, ApplianceData appliance)
    {
        VBoxContainer history = CreateTab(tabs, "History");
        history.AddChild(new UsageHistoryChart
        {
            Name = "DailyHistory",
            Values = appliance.Usage.DailyKwh,
            CustomMinimumSize = new Vector2(0, 110)
        });

        foreach (string entry in appliance.Usage.History)
        {
            history.AddChild(UiKit.Text(entry));
        }
        history.AddChild(UiKit.Text("kWh = (watts / 1,000) x hours. A high wattage alone does not prove unnecessary use.", 12));
    }

    private void BuildActionsTab(TabContainer tabs, HouseholdCase session, ApplianceData appliance)
    {
        VBoxContainer actions = CreateTab(tabs, "Take action");
        actions.AddChild(UiKit.Text("What should change?", 18));
        actions.AddChild(UiKit.Text($"Use the readings and household routine to choose a fix. An incorrect action costs {HouseholdCase.WrongActionPenalty} points.", 13));
        bool checkedDevice = session.Resolved.Contains(appliance.Id) || session.Cleared.Contains(appliance.Id);

        for (int index = 0; index < appliance.Actions.Count; index++)
        {
            int actionIndex = index;
            ActionData action = appliance.Actions[index];
            bool tried = session.WasActionTried(appliance.Id, index);
            var button = UiKit.Button(action.Text, () => ActionRequested?.Invoke(actionIndex));
            button.Name = "Action" + index;
            button.Alignment = HorizontalAlignment.Left;
            button.TooltipText = action.Text;
            button.Disabled = checkedDevice || tried;

            if (tried && !action.IsCorrect)
            {
                button.Text += "  (tried)";
            }
            actions.AddChild(button);
        }
        actions.AddChild(UiKit.Button("Leave it for now", () => CloseRequested?.Invoke()));
    }

    private static VBoxContainer CreateTab(TabContainer tabs, string heading)
    {
        var margin = new MarginContainer { Name = heading };
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 12);
        margin.AddThemeConstantOverride("margin_bottom", 12);
        tabs.AddChild(margin);
        var column = new VBoxContainer();
        margin.AddChild(column);
        return column;
    }

    public void ShowNotes(HouseholdCase session)
    {
        ScenarioData scenario = session.Scenario;
        Open(CasePanel.Notes, scenario.CaseLabel);
        body.AddChild(UiKit.Text(scenario.Brief, 16));
        body.AddChild(UiKit.Text($"{session.Resolved.Count}/{scenario.RequiredFindings} major problems resolved / {session.InspectionsRemaining}/{scenario.InspectionBudget} inspections left"));
        body.AddChild(UiKit.Text("Walk with WASD or the arrow keys. Glowing edges mark devices you can examine. Approach one and press E. Read its history before taking action.", 13));
        body.AddChild(new HSeparator());
        body.AddChild(UiKit.Text("Collected evidence", 20));

        if (session.Inspected.Count == 0)
        {
            body.AddChild(UiKit.Text("No readings yet. Look around the house before choosing your first inspection."));
        }

        foreach (ApplianceData appliance in session.Appliances.Where(appliance => session.Inspected.Contains(appliance.Id)))
        {
            AddNotebookEntry(session, appliance);
        }

        if (session.InspectionsRemaining == 0)
        {
            body.AddChild(new HSeparator());
            body.AddChild(UiKit.Text("No inspections left. You can still act on devices you have inspected, or close this case with the problems resolved so far.", 13));
            body.AddChild(UiKit.Button("Close investigation with current results", () => FinishRequested?.Invoke()));
        }
    }

    private void AddNotebookEntry(HouseholdCase session, ApplianceData appliance)
    {
        string statusSuffix = "";

        if (session.Resolved.Contains(appliance.Id))
        {
            statusSuffix = " / RESOLVED";
        }
        else if (session.Cleared.Contains(appliance.Id))
        {
            statusSuffix = " / CHECKED";
        }
        body.AddChild(UiKit.Text(appliance.Name + " / " + appliance.Room + statusSuffix, 17));
        body.AddChild(UiKit.Text($"{appliance.Usage.Watts} W / {appliance.Usage.OperatingTime}\nChange: {appliance.Usage.RecentChange}\nOvernight: {appliance.Usage.Overnight}\n{appliance.Evidence}", 13));
    }

    public void ShowPause()
    {
        Open(CasePanel.Pause, "Investigation paused");
        body.AddChild(UiKit.Text("WASD / arrows - move\nE - examine a nearby device\nN - case notes\nEsc - close a page or pause", 17));
        body.AddChild(UiKit.Button("Continue investigation", () => ResumeRequested?.Invoke()));
        body.AddChild(UiKit.Button("Start this case again", () => RestartRequested?.Invoke()));
        body.AddChild(UiKit.Button("Return to main menu", () => MenuRequested?.Invoke()));
    }

    public void ShowResults(HouseholdCase session, CaseResult result)
    {
        ScenarioData scenario = session.Scenario;
        Open(CasePanel.Results, session.Solved ? "Case solved" : "Investigation closed");
        body.AddChild(UiKit.Text($"{result.Score} points", 30));
        body.AddChild(UiKit.Text($"{result.Correct}/{scenario.RequiredFindings} major problems resolved / {result.InspectionsUsed}/{scenario.InspectionBudget} inspections used\nIncorrect actions: {result.Wrong}"));
        body.AddChild(new HSeparator());
        body.AddChild(UiKit.Text("Estimated monthly savings", 20));
        body.AddChild(UiKit.Text($"{result.MonthlyKwh:0.0} kWh     EUR {result.MonthlyCost:0.00}     {result.MonthlyEmissions:0.0} kg CO2", 22));
        body.AddChild(UiKit.Text($"Illustrative case values: 30 days, EUR {scenario.PricePerKwh:0.00}/kWh, {scenario.EmissionsPerKwh:0.00} kg CO2/kWh.", 11));

        foreach (ApplianceData appliance in session.Appliances.Where(appliance => appliance.MajorProblem))
        {
            string verdict = session.Resolved.Contains(appliance.Id) ? "RESOLVED" : "MISSED";
            body.AddChild(UiKit.Text($"{verdict} / {appliance.Name}\n{appliance.CorrectAction.Text}", 13));
        }
        body.AddChild(UiKit.Text($"+{HouseholdCase.PointsPerProblem} per major problem resolved, +{HouseholdCase.UnusedInspectionBonus} per unused inspection after resolving a problem; -{HouseholdCase.WrongActionPenalty} per incorrect action, -{HouseholdCase.UnhelpfulInspectionPenalty} per inspection of a non-major device. Score cannot fall below zero.", 11));
        AddResultButtons();
    }

    private void AddResultButtons()
    {
        var retry = UiKit.Button("Investigate again", () => RestartRequested?.Invoke());
        retry.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        footer.AddChild(retry);
        var menu = UiKit.Button("Main menu", () => MenuRequested?.Invoke());
        menu.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        footer.AddChild(menu);
        retry.GrabFocus();
    }

    public bool BlocksWorldClick(Vector2 point)
    {
        return ModalOpen || notesButton.GetGlobalRect().HasPoint(point);
    }
}
