using Sandbox;

namespace SboxSurvival;

public sealed class GameManager : Component
{
	public static GameManager Current { get; private set; }

	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpawnPoint { get; set; }

	[Property, Range( 0f, 24f )] public float StartingHour { get; set; } = 7f;
	[Property, Range( 1f, 240f )] public float MinutesPerDay { get; set; } = 20f;

	public float TimeOfDay { get; private set; }

	protected override void OnAwake()
	{
		Current = this;
		TimeOfDay = StartingHour;
	}

	protected override void OnStart()
	{
		if ( PlayerPrefab is not null && SpawnPoint is not null )
		{
			var spawn = PlayerPrefab.Clone( SpawnPoint.WorldPosition, SpawnPoint.WorldRotation );
			spawn.NetworkSpawn();
		}
	}

	protected override void OnUpdate()
	{
		var hoursPerSecond = 24f / (MinutesPerDay * 60f);
		TimeOfDay = (TimeOfDay + Time.Delta * hoursPerSecond) % 24f;
	}

	public bool IsNight => TimeOfDay < 6f || TimeOfDay > 20f;
}
