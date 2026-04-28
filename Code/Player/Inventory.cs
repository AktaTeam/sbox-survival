using Sandbox;
using System.Collections.Generic;

namespace SboxSurvival;

public sealed class Inventory : Component
{
	[Property, Range( 1, 64 )] public int SlotCount { get; set; } = 24;

	private readonly List<ItemStack> _slots = new();

	protected override void OnStart()
	{
		_slots.Clear();
		for ( int i = 0; i < SlotCount; i++ )
			_slots.Add( ItemStack.Empty );
	}

	public IReadOnlyList<ItemStack> Slots => _slots;

	public bool TryAdd( ItemDefinition def, int count = 1 )
	{
		if ( def is null || count <= 0 ) return false;

		for ( int i = 0; i < _slots.Count; i++ )
		{
			var s = _slots[i];
			if ( !s.IsEmpty && s.Definition == def && s.Count < def.MaxStack )
			{
				var space = def.MaxStack - s.Count;
				var add = System.Math.Min( space, count );
				_slots[i] = s with { Count = s.Count + add };
				count -= add;
				if ( count <= 0 ) return true;
			}
		}

		for ( int i = 0; i < _slots.Count && count > 0; i++ )
		{
			if ( _slots[i].IsEmpty )
			{
				var add = System.Math.Min( def.MaxStack, count );
				_slots[i] = new ItemStack( def, add );
				count -= add;
			}
		}

		return count <= 0;
	}

	public bool Remove( ItemDefinition def, int count = 1 )
	{
		var remaining = count;
		for ( int i = 0; i < _slots.Count && remaining > 0; i++ )
		{
			var s = _slots[i];
			if ( s.Definition != def ) continue;
			var take = System.Math.Min( s.Count, remaining );
			remaining -= take;
			_slots[i] = s.Count - take <= 0 ? ItemStack.Empty : s with { Count = s.Count - take };
		}
		return remaining <= 0;
	}
}

public readonly record struct ItemStack( ItemDefinition Definition, int Count )
{
	public static ItemStack Empty => new( null, 0 );
	public bool IsEmpty => Definition is null || Count <= 0;
}
