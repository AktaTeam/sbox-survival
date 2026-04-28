using Sandbox;

namespace SboxSurvival;

public sealed class Interactor : Component
{
	[Property] public GameObject Eye { get; set; }
	[Property, Range( 30f, 250f )] public float Reach { get; set; } = 90f;
	[Property] public Inventory Inventory { get; set; }
	[Property] public ToolKind EquippedTool { get; set; } = ToolKind.Hand;
	[Property] public float SwingDamage { get; set; } = 10f;

	protected override void OnUpdate()
	{
		if ( !Input.Pressed( "attack1" ) || Eye is null ) return;

		var origin = Eye.WorldPosition;
		var dir = Eye.WorldRotation.Forward;
		var tr = Scene.Trace.Ray( origin, origin + dir * Reach )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !tr.Hit || tr.GameObject is null ) return;

		if ( tr.GameObject.Components.Get<ResourceNode>() is { } node )
			node.TryHarvest( Inventory, EquippedTool, SwingDamage );
	}
}
