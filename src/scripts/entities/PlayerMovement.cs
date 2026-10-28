using Godot;

namespace HiddenWatts.Scripts.Entities;

public partial class PlayerMovement : Node
{
    [Export(PropertyHint.Range, "0,300,1")] public float Speed { get; set; } = 90.0f;
    public void Move(CharacterBody2D body, Vector2 direction)
    {
        body.Velocity = direction.LimitLength() * Speed;
        body.MoveAndSlide();
    }
}
