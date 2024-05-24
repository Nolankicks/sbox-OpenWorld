using Sandbox;

public sealed class DebugSpawner : Component
{
	[Property] public GameObject Prefab { get; set; }

	[Button("Spawn")]
	public void Spawn()
	{
		if (Game.ActiveScene is not null)
		{
		var spawns = Scene.GetAllComponents<SpawnPoint>().ToList();
		var spawn = Game.Random.FromList(spawns);
		var obj = Prefab.Clone(spawn.Transform.World);
		obj.NetworkSpawn(null);
		}
	}
}
