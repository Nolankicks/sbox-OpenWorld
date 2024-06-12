using Kicks;
using Sandbox;

public sealed class WaveSpawner : Component, Component.ITriggerListener
{
	[Property] public GameObject ZombiePrefab { get; set; }
	[Property] public bool CoolDown { get; set; } = false;
	[Property] public int NumberOfWaves { get; set; }
	[Property] public List<GameObject> CurrentWave { get; set; } = new();
	[Property] public int CurrentWaveIndex { get; set; } = 0;
	[Property] public bool IsSpawning { get; set; } = false;
	public List<GameObject> WaveSpawnLocations { get; set; } = new();
	public bool Started { get; set; } = false;
	public delegate void WaveActionDel(WaveSpawner waveSpawner);
	[Property] public WaveActionDel WaveAction { get; set; }
	[Property] public string SpawnLocationString { get; set; } = "zombiespawn";
	protected override void OnStart()
	{
		var spawns = Scene.GetAllComponents<SpawnPoint>().Where(x => x.Tags.Has(SpawnLocationString)).ToList();
		foreach (var spawn in spawns)
		{
			WaveSpawnLocations.Add(spawn.GameObject);
		}
	}

	public async Task StartWave(int NumberOfZombies, bool IsFinalWave = false, bool IsFirstWave = false)
	{
		Log.Info("Wave");
		if (IsFirstWave)
		{
			Log.Info("First Wave");
			IsSpawning = true;
			if (Components.TryGet<Collider>(out var collider, FindMode.EnabledInSelfAndChildren))
			{
				collider.Destroy();
			}
		}
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
		if (IsFinalWave)
		{
			Log.Info("Final Wave");
			IsSpawning = false;
		}
		
	}
	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		if (other.GameObject.Parent.Tags.Has("player") && !Started)
		{
			Log.Info("WaveAction");
			var waveUi = Scene.GetAllComponents<WaveUi>().FirstOrDefault();
			if (waveUi is not null)
			{
				waveUi.Spawner = this;
				WaveAction?.Invoke(this);
			}
			else
			{
				Log.Error("WaveUi not found");
			}
		}
	}
	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{

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

	[ActionGraphNode("ActiveScene"), Pure]
	public static Scene ActiveScene()
	{
		return Game.ActiveScene;
	}

[ActionGraphNode("Get Components First or Default")]
public static T GetAllComponentsFirstOrDefault<T>()
{
    return Game.ActiveScene.GetAllComponents<T>().FirstOrDefault();
}

[ActionGraphNode("Get Current Wave"), Pure]
public static WaveSpawner GetCurrentWave()
{
	return Game.ActiveScene.GetAllComponents<WaveSpawner>().FirstOrDefault( x => x.IsSpawning );
}
}
