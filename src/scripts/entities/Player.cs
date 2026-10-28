using Godot;

namespace HiddenWatts.Scripts.Entities;

public partial class Player : CharacterBody2D
{
	private PlayerInput playerInput;
	private PlayerMovement movement;
	private AnimatedSprite2D sprite;
	private RayCast2D facingRay;
	private CollisionShape2D collider;
	private string facingName = "Down";

	[Export(PropertyHint.Range, "1,64,1")]
	public float FacingDistance { get; set; } = 20.0f;

	public Vector2 FacingDirection { get; private set; } = Vector2.Down;

	public Vector2 FeetPosition => collider.GlobalPosition;

	public override void _Ready()
	{
		playerInput = GetNode<PlayerInput>("Input");
		movement = GetNode<PlayerMovement>("Movement");
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		facingRay = GetNode<RayCast2D>("CollisionRayCast");
		collider = GetNode<CollisionShape2D>("Collider");
		sprite.Play("IdleDown");
		facingRay.TargetPosition = FacingDirection * FacingDistance;
		var camera = GetNode<Camera2D>("Camera2D");

		if (camera.Enabled)
		{
			camera.MakeCurrent();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = playerInput.ReadDirection();
		bool isMoving = !direction.IsZeroApprox();

		if (isMoving)
		{
			UpdateFacing(direction);
		}
		movement.Move(this, direction);
		PlayAnimation(isMoving);
	}

	private void UpdateFacing(Vector2 direction)
	{
		if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
		{
			facingName = direction.X > 0 ? "Right" : "Left";
			FacingDirection = direction.X > 0 ? Vector2.Right : Vector2.Left;
		}
		else
		{
			facingName = direction.Y > 0 ? "Down" : "Up";
			FacingDirection = direction.Y > 0 ? Vector2.Down : Vector2.Up;
		}
		facingRay.TargetPosition = FacingDirection * FacingDistance;
	}

	private void PlayAnimation(bool isMoving)
	{
		StringName animation = (isMoving ? "Walk" : "Idle") + facingName;

		if (sprite.Animation != animation || !sprite.IsPlaying())
		{
			sprite.Play(animation);
		}
	}

	public void SetMovementEnabled(bool enabled)
	{
		SetPhysicsProcess(enabled);

		if (!enabled)
		{
			Velocity = Vector2.Zero;
			PlayAnimation(false);
		}
	}
}
