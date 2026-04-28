using Sandbox;

namespace SboxSurvival;

[GameResource( "Item Definition", "item", "Defines an inventory item.", Icon = "inventory_2" )]
public sealed class ItemDefinition : GameResource
{
	public string DisplayName { get; set; } = "Unnamed";
	public string Description { get; set; } = "";
	public Texture Icon { get; set; }
	public int MaxStack { get; set; } = 1;

	public ItemCategory Category { get; set; } = ItemCategory.Misc;

	public float HungerRestore { get; set; } = 0f;
	public float ThirstRestore { get; set; } = 0f;
	public float HealthRestore { get; set; } = 0f;
}

public enum ItemCategory
{
	Misc,
	Food,
	Drink,
	Tool,
	Weapon,
	Material,
	Buildable,
}
