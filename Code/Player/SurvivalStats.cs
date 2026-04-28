using Sandbox;
using System;

namespace SboxSurvival;

// TODO(multi): Callers of the removed TrySpendStamina (e.g. Player.cs sprint logic)
// must migrate to: CanAffordStamina (local check) + DrainStamina (host RPC).
// Audit Player.cs next pass to complete the migration.
public sealed class SurvivalStats : Component
{
	[Property, Range( 1f, 200f )] public float MaxHealth { get; set; } = 100f;
	[Property, Range( 1f, 200f )] public float MaxHunger { get; set; } = 100f;
	[Property, Range( 1f, 200f )] public float MaxThirst { get; set; } = 100f;
	[Property, Range( 1f, 200f )] public float MaxStamina { get; set; } = 100f;

	[Property] public float HungerDrainPerSecond { get; set; } = 0.15f;
	[Property] public float ThirstDrainPerSecond { get; set; } = 0.22f;
	[Property] public float StaminaRegenPerSecond { get; set; } = 8f;
	[Property] public float StarvationDamagePerSecond { get; set; } = 1f;

	[Sync] public float Health { get; private set; }
	[Sync] public float Hunger { get; private set; }
	[Sync] public float Thirst { get; private set; }
	[Sync] public float Stamina { get; private set; }

	public bool IsAlive => Health > 0f;
	public bool CanSprint => Stamina > 5f && IsAlive;

	// TODO(multi): Currently fires only on host (ApplyDamage is [Rpc.Host]).
	// When a consumer subscribes (death screen, ragdoll, killfeed), wrap this in
	// an [Rpc.Broadcast] dispatcher so all clients receive the death signal.
	public event Action OnDied;

	protected override void OnStart()
	{
		if ( IsProxy ) return;

		Health = MaxHealth;
		Hunger = MaxHunger;
		Thirst = MaxThirst;
		Stamina = MaxStamina;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		if ( !IsAlive ) return;

		Hunger = MathF.Max( 0f, Hunger - HungerDrainPerSecond * Time.Delta );
		Thirst = MathF.Max( 0f, Thirst - ThirstDrainPerSecond * Time.Delta );
		Stamina = MathF.Min( MaxStamina, Stamina + StaminaRegenPerSecond * Time.Delta );

		var starving = Hunger <= 0f || Thirst <= 0f;
		if ( starving )
			ApplyDamage( StarvationDamagePerSecond * Time.Delta );
	}

	private bool IsCallerAuthorized()
	{
		if ( Rpc.Caller.IsHost ) return true;
		return Rpc.Caller.CanRefreshObjects && Rpc.Caller == Network.Owner;
	}

	[Rpc.Host]
	public void ApplyDamage( float amount )
	{
		if ( !IsAlive ) return;
		Health = MathF.Max( 0f, Health - amount );
		if ( Health <= 0f ) OnDied?.Invoke();
	}

	[Rpc.Host]
	public void Heal( float amount )
	{
		if ( !IsCallerAuthorized() )
		{
			Log.Warning( $"Unauthorized Heal from {Rpc.Caller.DisplayName}" );
			return;
		}
		if ( !IsAlive ) return;
		Health = MathF.Min( MaxHealth, Health + amount );
	}

	[Rpc.Host]
	public void Eat( float amount )
	{
		if ( !IsCallerAuthorized() )
		{
			Log.Warning( $"Unauthorized Eat from {Rpc.Caller.DisplayName}" );
			return;
		}
		if ( !IsAlive ) return;
		Hunger = MathF.Min( MaxHunger, Hunger + amount );
	}

	[Rpc.Host]
	public void Drink( float amount )
	{
		if ( !IsCallerAuthorized() )
		{
			Log.Warning( $"Unauthorized Drink from {Rpc.Caller.DisplayName}" );
			return;
		}
		if ( !IsAlive ) return;
		Thirst = MathF.Min( MaxThirst, Thirst + amount );
	}

	// Local read for client-side prediction. Use before calling DrainStamina.
	public bool CanAffordStamina( float amount ) => Stamina >= amount;

	[Rpc.Host]
	public void DrainStamina( float amount )
	{
		if ( !IsCallerAuthorized() )
		{
			Log.Warning( $"Unauthorized DrainStamina from {Rpc.Caller.DisplayName}" );
			return;
		}
		Stamina = MathF.Max( 0f, Stamina - amount );
	}
}
