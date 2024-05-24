using Kicks;
using Sandbox;

public sealed class WaveSpawner : Component
{
	[Property] public GameObject ZombiePrefab { get; set; }
	[Property] public bool CoolDown { get; set; } = false;
	[Property] public int NumberOfWaves { get; set; }
	[Property] public List<GameObject> CurrentWave { get; set; } = new();
	[Property] public int CurrentWaveIndex { get; set; } = 0;
	[Property] public GameObject WaveTrigger { get; set; }
	[Property] public bool IsSpawning { get; set; } = false;
	[Property] public List<GameObject> WaveSpawnLocations { get; set; }
	protected override void OnStart()
	{
		var spawns = Scene.GetAllComponents<SpawnPoint>().Where(x => x.Tags.Has("zombiespawn")).ToList();
		foreach (var spawn in spawns)
		{
			WaveSpawnLocations.Add(spawn.GameObject);
		}
	}

	public async Task StartWave(int NumberOfZombies)
	{
		for (int i = 0; i < NumberOfZombies; i++)
		{
			var spawnLocation = Game.Random.FromList(WaveSpawnLocations);
			var zombie = ZombiePrefab.Clone(spawnLocation.Transform.Position);
			CurrentWave.Add(zombie);
			await GameTask.Delay(1);
		}
		while (CurrentWave.Count > 0)
		{
			await GameTask.DelaySeconds(1);
		}
		CurrentWaveIndex++;
		CurrentWave.Clear();
		
	}

	public async Task AwaitPlayer()
	{

		var player = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy );
		while (Vector3.DistanceBetween(player.Transform.Position, WaveTrigger.Transform.Position) > 150)
		{
			await GameTask.DelaySeconds(1);
		}
		IsSpawning = true;
		}
		
	public TimeUntil NextWave = 0;
	public async Task WaveCoolDown(int Seconds)
	{
		CoolDown = true;
		NextWave = Seconds;
		await GameTask.DelaySeconds(Seconds);
		CoolDown = false;
		}

	public void RemoveZombie(GameObject Zombie)
	{
		if (CurrentWave.Contains(Zombie))
		{
			CurrentWave.Remove(Zombie);
		}
	}
}
