using System;
using Godot;
using HiddenWatts.Scripts.Entities;
using HiddenWatts.Scripts.Gameplay.Data;
using HiddenWatts.Scripts.Ui;

namespace HiddenWatts.Scripts.Gameplay;

public partial class Investigation : Node
{
	private const float InspectionReach = 36.0f;
	private HouseholdCase currentCase;
	private GameHud hud;
	private Player player;
	private Sprite2D[] devices;
	private ApplianceIndicator[] indicators;
	private SubViewport world;
	private SubViewportContainer viewportContainer;
	private int selectedDeviceIndex = -1;
	private int nearbyDeviceIndex = -1;

	[Export]
	public Godot.Json ScenarioFile { get; set; }

	public override void _Ready()
	{
		ScenarioCatalog.Initialize(ScenarioFile);
		currentCase = new HouseholdCase(ScenarioCatalog.GetSelectedScenario());
		FindSceneNodes();
		CreateIndicators();
		ConnectInterfaceEvents();
		ActivatePlayerCamera();
		hud.RefreshStatus(currentCase);
		hud.Message($"Find {currentCase.Scenario.RequiredFindings} major problems. [N] opens your case file.");
	}

	private void FindSceneNodes()
	{
		viewportContainer = GetNode<SubViewportContainer>("Control/SubViewportContainer");
		world = viewportContainer.GetNode<SubViewport>("SubViewport");
		var level = world.GetNode<Node2D>("Level");
		player = level.GetNode<Player>("Player");
		hud = GetNode<GameHud>("Interface/GameHud");
		devices = new Sprite2D[currentCase.Appliances.Count];

		for (int index = 0; index < devices.Length; index++)
		{
			ApplianceData appliance = currentCase.Appliances[index];
			devices[index] = level.GetNodeOrNull<Sprite2D>(appliance.NodePath);

			if (devices[index] == null)
			{
				throw new InvalidOperationException($"Case '{currentCase.Scenario.Id}' references a missing appliance: {appliance.NodePath}");
			}
		}
	}

	private void CreateIndicators()
	{
		indicators = new ApplianceIndicator[devices.Length];

		for (int index = 0; index < devices.Length; index++)
		{
			Rect2 bounds = devices[index].GetRect();
			var indicator = new ApplianceIndicator
			{
				Name = "InvestigationGlow",
				DeviceSize = bounds.Size,
				DeviceOrigin = bounds.Position,
				ZIndex = 5
			};
			devices[index].AddChild(indicator);
			indicators[index] = indicator;
		}
	}

	private void ConnectInterfaceEvents()
	{
		hud.InspectRequested += InspectSelected;
		hud.ActionRequested += RecordDiagnosis;
		hud.CloseRequested += ClosePanel;
		hud.NotesRequested += OpenNotes;
		hud.ResumeRequested += ClosePanel;
		hud.RestartRequested += RestartCase;
		hud.MenuRequested += ReturnToMenu;
		hud.FinishRequested += SubmitCase;
	}

	private void ActivatePlayerCamera()
	{
		var camera = player.GetNode<Camera2D>("Camera2D");
		camera.Enabled = true;
		camera.MakeCurrent();
		camera.ResetSmoothing();
	}

	private float DistanceTo(Sprite2D device)
	{
		Rect2 bounds = device.GetRect();
		Vector2 localFeet = device.ToLocal(player.FeetPosition);
		Vector2 nearestPoint = localFeet.Clamp(bounds.Position, bounds.End);
		return device.ToGlobal(nearestPoint).DistanceTo(player.FeetPosition);
	}

	private bool HasLineOfSight(Sprite2D device)
	{
		Rect2 bounds = device.GetRect();
		Vector2 start = player.FeetPosition;
		Vector2 closestPoint = device.ToLocal(start).Clamp(bounds.Position, bounds.End);
		Vector2 end = device.ToGlobal(closestPoint);

		if (start.IsEqualApprox(end))
		{
			return true;
		}

		var excludedBodies = new Godot.Collections.Array<Rid> { player.GetRid() };
		var query = PhysicsRayQueryParameters2D.Create(start, end, 1, excludedBodies);
		var hit = world.World2D.DirectSpaceState.IntersectRay(query);

		if (hit.Count == 0)
		{
			return true;
		}

		var collider = hit["collider"].AsGodotObject() as Node;

		if (collider != null && device.IsAncestorOf(collider))
		{
			return true;
		}

		if (collider?.GetParent() is Sprite2D support)
		{
			return support.GetRect().Grow(1).HasPoint(support.ToLocal(end));
		}

		return false;
	}

	private bool IsWithinReach(Sprite2D device)
	{
		return DistanceTo(device) <= InspectionReach && HasLineOfSight(device);
	}

	private int FindNearestDevice()
	{
		int nearestIndex = -1;
		float nearestDistance = float.MaxValue;

		for (int index = 0; index < devices.Length; index++)
		{
			float distance = DistanceTo(devices[index]);

			if (distance > InspectionReach || distance >= nearestDistance)
			{
				continue;
			}

			if (!HasLineOfSight(devices[index]))
			{
				continue;
			}
			nearestIndex = index;
			nearestDistance = distance;
		}
		return nearestIndex;
	}

	public override void _Process(double delta)
	{
		if (hud.ModalOpen || currentCase.Finished)
		{
			return;
		}

		int nearestIndex = FindNearestDevice();

		if (nearestIndex == nearbyDeviceIndex)
		{
			return;
		}

		nearbyDeviceIndex = nearestIndex;

		for (int index = 0; index < indicators.Length; index++)
		{
			indicators[index].Nearby = index == nearestIndex;
		}

		if (nearestIndex < 0)
		{
			hud.SetNearby(null, false);
			return;
		}

		ApplianceData appliance = currentCase.Appliances[nearestIndex];
		hud.SetNearby(appliance.Name, currentCase.Inspected.Contains(appliance.Id));
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (currentCase.Finished)
		{
			return;
		}

		if (inputEvent is InputEventKey key && key.Pressed && !key.Echo)
		{
			if (HandleKey(key))
			{
				GetViewport().SetInputAsHandled();
			}
			return;
		}

		if (inputEvent is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
		{
			HandleClick(mouse.Position);
		}
	}

	private bool HandleKey(InputEventKey key)
	{
		Key pressedKey = key.PhysicalKeycode != Key.None ? key.PhysicalKeycode : key.Keycode;

		switch (pressedKey)
		{
			case Key.Escape:

				if (hud.ModalOpen)
				{
					ClosePanel();
				}
				else
				{
					TogglePause();
				}
				return true;
			case Key.N:

				if (hud.OpenPanel == CasePanel.Notes)
				{
					ClosePanel();
				}
				else if (!hud.ModalOpen)
				{
					OpenNotes();
				}
				return true;
			case Key.E:

				if (!hud.ModalOpen)
				{
					InteractNearest();
				}
				else if (hud.OpenPanel == CasePanel.Device && selectedDeviceIndex >= 0
					&& !currentCase.Inspected.Contains(currentCase.Appliances[selectedDeviceIndex].Id))
				{
					InspectSelected();
				}
				return true;
			default:
				return false;
		}
	}

	private void HandleClick(Vector2 position)
	{
		if (hud.BlocksWorldClick(position) || !viewportContainer.GetGlobalRect().HasPoint(position))
		{
			return;
		}

		Vector2 localPoint = position - viewportContainer.GlobalPosition;
		Vector2 viewportPoint = localPoint / viewportContainer.Size * (Vector2)world.Size;
		Vector2 worldPoint = world.CanvasTransform.AffineInverse() * viewportPoint;

		for (int index = devices.Length - 1; index >= 0; index--)
		{
			if (devices[index].GetRect().HasPoint(devices[index].ToLocal(worldPoint)))
			{
				SelectDevice(index);
				return;
			}
		}
	}

	public void InteractNearest()
	{
		if (hud.ModalOpen || currentCase.Finished)
		{
			return;
		}

		int nearestIndex = FindNearestDevice();

		if (nearestIndex < 0)
		{
			hud.Message("Walk up to a highlighted appliance to examine it.");
			return;
		}

		SelectDevice(nearestIndex);
	}

	public void SelectDevice(int index)
	{
		if (currentCase.Finished || hud.ModalOpen || index < 0 || index >= devices.Length)
		{
			return;
		}

		if (!IsWithinReach(devices[index]))
		{
			hud.Message("Walk closer to examine that appliance.");
			return;
		}

		selectedDeviceIndex = index;
		player.SetMovementEnabled(false);
		hud.ShowDevice(currentCase, index);
	}

	private bool CanUseSelectedDevice()
	{
		return !currentCase.Finished
			&& hud.OpenPanel == CasePanel.Device
			&& selectedDeviceIndex >= 0
			&& IsWithinReach(devices[selectedDeviceIndex]);
	}

	public void InspectSelected()
	{
		if (!CanUseSelectedDevice())
		{
			return;
		}

		ApplianceData appliance = currentCase.Appliances[selectedDeviceIndex];

		if (!currentCase.Inspect(appliance.Id))
		{
			hud.ShowDevice(currentCase, selectedDeviceIndex, "No inspections remain. Review the readings you have already collected.");
			return;
		}

		hud.RefreshStatus(currentCase);
		hud.ShowDevice(currentCase, selectedDeviceIndex);
	}

	public void RecordDiagnosis(int actionIndex)
	{
		if (!CanUseSelectedDevice())
		{
			return;
		}

		ApplianceData appliance = currentCase.Appliances[selectedDeviceIndex];
		ActionOutcome outcome = currentCase.ApplyAction(appliance.Id, actionIndex);

		if (!outcome.Accepted)
		{
			return;
		}

		indicators[selectedDeviceIndex].Resolved = currentCase.Resolved.Contains(appliance.Id);
		indicators[selectedDeviceIndex].Checked = currentCase.Cleared.Contains(appliance.Id);
		hud.RefreshStatus(currentCase);

		if (currentCase.Solved)
		{
			FinishCase();
			return;
		}

		hud.ShowDevice(currentCase, selectedDeviceIndex, outcome.Message);
	}

	public void ClosePanel()
	{
		if (currentCase.Finished)
		{
			return;
		}

		hud.HideModal();
		hud.SetNearby(null, false);
		selectedDeviceIndex = -1;
		nearbyDeviceIndex = -2;
		player.SetMovementEnabled(true);
	}

	public void OpenNotes()
	{
		if (currentCase.Finished || hud.ModalOpen)
		{
			return;
		}

		player.SetMovementEnabled(false);
		hud.ShowNotes(currentCase);
	}

	public void TogglePause()
	{
		if (currentCase.Finished)
		{
			return;
		}

		if (hud.OpenPanel == CasePanel.Pause)
		{
			ClosePanel();
			return;
		}

		if (hud.ModalOpen)
		{
			return;
		}

		player.SetMovementEnabled(false);
		hud.ShowPause();
	}

	public void SubmitCase()
	{
		if (!currentCase.Finished && (currentCase.Solved || currentCase.InspectionsRemaining == 0))
		{
			FinishCase();
		}
	}

	private void FinishCase()
	{
		CaseResult result = currentCase.Submit();
		player.SetMovementEnabled(false);
		hud.SetNearby(null, false);
		hud.ShowResults(currentCase, result);
	}

	private void RestartCase()
	{
		GetTree().ReloadCurrentScene();
	}

	private void ReturnToMenu()
	{
		GetTree().ChangeSceneToFile("res://scenes/ui/main_menu.tscn");
	}

	public int GetInspectionsRemaining() => currentCase.InspectionsRemaining;

	public int GetFindingCount() => currentCase.Resolved.Count;

	public int GetWrongActions() => currentCase.WrongActions;

	public bool IsCaseFinished() => currentCase.Finished;

	public int GetFinalScore() => currentCase.Finished ? currentCase.Submit().Score : 0;

	public string GetOpenPanel() => hud.OpenPanel.ToString();

	public string GetCaseId() => currentCase.Scenario.Id;
}
