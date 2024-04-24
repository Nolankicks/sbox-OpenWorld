using System.Threading.Tasks;
using Sandbox;

public sealed class AmmoSpawner : Component
{
	[Property] public List<GameObject> AmmoSpawnLocations { get; set; } = new();
	[Property] public GameObject AmmoPrefab { get; set; }
	protected override void OnStart()
	{
		_ = SpawnAmmo();
	}

	public async Task SpawnAmmo()
	{
		while (true)
		{
		if (AmmoSpawnLocations.Count == 0) return;
		var randomSpawn = Game.Random.FromList(AmmoSpawnLocations);
		var ammo = AmmoPrefab.Clone(randomSpawn.Transform.Position);
		ammo.NetworkSpawn(null);
		await Task.DelaySeconds(10);
		}
	}
}
