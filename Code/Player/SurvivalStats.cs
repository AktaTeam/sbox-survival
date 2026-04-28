using Sandbox;
using System;

namespace SboxSurvival;

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

	public float Health { get; private set; }
	public float Hunger { get; private set; }
	public float Thirst { get; private set; }
	public float Stamina { get; private set; }

	public bool IsAlive => Health > 0f;
	public bool CanSprint => Stamina > 5f && IsAlive;

	public event Action OnDied;

	protected override void OnStart()
	{
		Health = MaxHealth;
		Hunger = MaxHunger;
		Thirst = MaxThirst;
		Stamina = MaxStamina;
	}

	protected override void OnFixedUpdate()
	{
		if ( !IsAlive ) return;

		Hunger = MathF.Max( 0f, Hunger - HungerDrainPerSecond * Time.Delta );
		Thirst = MathF.Max( 0f, Thirst - ThirstDrainPerSecond * Time.Delta );
		Stamina = MathF.Min( MaxStamina, Stamina + StaminaRegenPerSecond * Time.Delta );

		var starving = Hunger <= 0f || Thirst <= 0f;
		if ( starving )
			ApplyDamage( StarvationDamagePerSecond * Time.Delta );
	}

	public void ApplyDamage( float amount )
	{
		if ( !IsAlive ) return;
		Health = MathF.Max( 0f, Health - amount );
		if ( Health <= 0f ) OnDied?.Invoke();
	}

	public void Heal( float amount ) => Health = MathF.Min( MaxHealth, Health + amount );
	public void Eat( float amount ) => Hunger = MathF.Min( MaxHunger, Hunger + amount );
	public void Drink( float amount ) => Thirst = MathF.Min( MaxThirst, Thirst + amount );

	public void DrainStamina( float amount ) => Stamina = MathF.Max( 0f, Stamina - amount );

	public bool TrySpendStamina( float amount )
	{
		if ( Stamina < amount ) return false;
		Stamina -= amount;
		return true;
	}
}
