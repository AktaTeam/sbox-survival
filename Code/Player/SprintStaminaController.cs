using Sandbox;

namespace SboxSurvival;

/// <summary>
/// Bridges the native PlayerController to the project's SurvivalStats system.
/// Drains stamina while the player is sprinting, and on jump events.
///
/// Designed as a sibling Component on the same GameObject as PlayerController.
/// Implements PlayerController.IEvents to react to native events (e.g. OnJumped).
///
/// Phase Player.2a: created alongside the legacy Player.cs (which still owns
/// the GameObject in the scene). This component does nothing until attached
/// in Phase Player.2b. Player.cs is removed in Phase Player.2c.
///
/// Game design choice (audit 2026-04-28): no strict block on sprint or jump.
/// Stamina drains, clamps at 0, but the player can still attempt the action.
/// Re-evaluate if game feel reveals issues.
///
/// Note: we read Input.Down("run") to detect the native sprint state.
/// The "run" binding matches the "Alt Move Button" defined in the
/// PlayerController inspector (Input tab, Running section). We initially
/// tried Controller.IsRunning() but it collides with Component.IsRunning(Doo)
/// — an inherited method with different semantics. Direct input read is
/// the cleanest workaround and stays aligned with player-configurable bindings.
///
/// Crouch (Ducked Speed/Height in native inspector) is NOT handled in this
/// iteration. Future phase: optional crouch-recovery bonus if game feel needs it.
/// </summary>
public sealed class SprintStaminaController : Component, PlayerController.IEvents
{
	[Property] public PlayerController Controller { get; set; }
	[Property] public SurvivalStats Stats { get; set; }

	[Property, Range( 1f, 30f )]
	public float StaminaDrainPerSecond { get; set; } = 12f;

	[Property, Range( 1f, 50f )]
	public float JumpStaminaCost { get; set; } = 10f;

	protected override void OnStart()
	{
		Controller ??= Components.Get<PlayerController>();
		Stats ??= Components.Get<SurvivalStats>();
	}

	protected override void OnFixedUpdate()
	{
		// Owner-authoritative simulation only. Proxies observe state via [Sync].
		if ( IsProxy ) return;

		if ( Controller is null || Stats is null ) return;

		// Drain stamina while the native PlayerController reports it's sprinting.
		if ( Input.Down( "run" ) && Stats.CanAffordStamina( 0.01f ) )
		{
			Stats.DrainStamina( StaminaDrainPerSecond * Time.Delta );
		}
	}

	/// <summary>
	/// Called by the native PlayerController immediately after a successful jump.
	/// We drain stamina post-jump (no pre-jump block in this iteration — see audit notes).
	/// </summary>
	public void OnJumped()
	{
		if ( IsProxy ) return;

		Stats?.DrainStamina( JumpStaminaCost );
	}
}
