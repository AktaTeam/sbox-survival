using Sandbox;

namespace SboxSurvival;

// TODO(architecture): This component duplicates several PlayerController native
// behaviors (input sampling, eye angles, camera rotation, movement). Phase Player.2
// will dissolve this into SprintStaminaController via PlayerController.IEvents.
// See audit report from session 2026-04-28 for the full reasoning.
public sealed class Player : Component
{
	[Property] public CharacterController Controller { get; set; }
	[Property] public GameObject Camera { get; set; }
	[Property] public SurvivalStats Stats { get; set; }

	[Property, Range( 50f, 400f )] public float WalkSpeed { get; set; } = 130f;
	[Property, Range( 50f, 600f )] public float SprintSpeed { get; set; } = 220f;
	[Property, Range( 100f, 500f )] public float JumpStrength { get; set; } = 300f;

	private Angles _eyeAngles;

	protected override void OnStart()
	{
		Stats ??= GetComponent<SurvivalStats>();
		Controller ??= GetComponent<CharacterController>();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		_eyeAngles += Input.AnalogLook;
		_eyeAngles.pitch = _eyeAngles.pitch.Clamp( -89f, 89f );
		_eyeAngles.roll = 0f;

		if ( Camera is not null )
			Camera.WorldRotation = _eyeAngles.ToRotation();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		if ( Controller is null ) return;

		var sprinting = Input.Down( "Run" ) && Stats?.CanSprint == true;
		var speed = sprinting ? SprintSpeed : WalkSpeed;
		var wish = Input.AnalogMove.Normal * speed;
		var move = _eyeAngles.WithPitch( 0f ).ToRotation() * wish;

		if ( Controller.IsOnGround )
		{
			Controller.Velocity = Controller.Velocity.WithZ( 0 );
			Controller.Accelerate( move );
			Controller.ApplyFriction( 4f );

			if ( Input.Pressed( "Jump" ) && Stats?.CanAffordStamina( 10f ) == true )
			{
				Stats.DrainStamina( 10f );
				Controller.Punch( Vector3.Up * JumpStrength );
			}
		}
		else
		{
			Controller.Velocity += Vector3.Down * 800f * Time.Delta;
			Controller.Accelerate( move.ClampLength( 50f ) );
		}

		Controller.Move();

		if ( sprinting && wish.LengthSquared > 0.01f )
			Stats?.DrainStamina( 12f * Time.Delta );
	}
}
