using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HiddenWatts.Scripts.Gameplay.Data;

namespace HiddenWatts.Scripts.Gameplay;

public partial class ElectronicDevice : Sprite2D
{
    [Export]
    public string ApplianceId { get; set; } = "";

    public static Sprite2D[] ResolveAll(Node household, IReadOnlyList<ApplianceData> appliances)
    {
        var tagged = new Dictionary<string, ElectronicDevice>();

        foreach (ElectronicDevice device in household.FindChildren("*", "Sprite2D", true, false).OfType<ElectronicDevice>())
        {
            if (string.IsNullOrWhiteSpace(device.ApplianceId) || !tagged.TryAdd(device.ApplianceId, device))
            {
                throw new InvalidOperationException($"Electronics in {household.SceneFilePath} need unique ApplianceId values. Check {device.Name}.");
            }
        }

        var devices = new Sprite2D[appliances.Count];

        for (int index = 0; index < appliances.Count; index++)
        {
            ApplianceData appliance = appliances[index];
            devices[index] = tagged.GetValueOrDefault(appliance.Id)
                ?? household.GetNodeOrNull<Sprite2D>(appliance.NodePath);

            if (devices[index] == null)
            {
                throw new InvalidOperationException($"Missing electronic '{appliance.Id}' in {household.SceneFilePath}. Expected {appliance.NodePath} or a matching ApplianceId.");
            }
        }

        return devices;
    }
}
