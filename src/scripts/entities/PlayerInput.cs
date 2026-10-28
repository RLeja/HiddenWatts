using Godot;

namespace HiddenWatts.Scripts.Entities;

public partial class PlayerInput : Node
{
    public Vector2 ReadDirection() => Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
}
