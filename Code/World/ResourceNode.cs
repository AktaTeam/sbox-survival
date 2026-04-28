using Sandbox;

namespace SboxSurvival;

public sealed class ResourceNode : Component
{
	[Property] public ItemDefinition Yield { get; set; }
	[Property, Range( 1, 50 )] public int YieldCount { get; set; } = 3;
	[Property, Range( 1f, 200f )] public float Health { get; set; } = 30f;
	[Property] public float RespawnSeconds { get; set; } = 60f;
	[Property] public ToolKind RequiredTool { get; set; } = ToolKind.None;

	private float _currentHealth;
	private TimeSince _harvestedAt;
	private bool _depleted;

	protected override void OnStart()
	{
		_currentHealth = Health;
	}

	protected override void OnUpdate()
	{
		if ( _depleted && _harvestedAt >= RespawnSeconds )
			Respawn();
	}

	public bool TryHarvest( Inventory inv, ToolKind tool, float damage )
	{
		if ( _depleted || inv is null ) return false;
		if ( RequiredTool != ToolKind.None && tool != RequiredTool ) return false;

		_currentHealth -= damage;
		if ( _currentHealth > 0f ) return true;

		if ( Yield is not null )
			inv.TryAdd( Yield, YieldCount );

		_depleted = true;
		_harvestedAt = 0f;
		GameObject.Enabled = false;
		return true;
	}

	private void Respawn()
	{
		_currentHealth = Health;
		_depleted = false;
		GameObject.Enabled = true;
	}
}

public enum ToolKind
{
	None,
	Hand,
	Axe,
	Pickaxe,
	Knife,
}
