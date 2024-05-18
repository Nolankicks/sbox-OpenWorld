using Sandbox;
using System.Threading.Tasks;
public sealed class ProcGenZombieSpawner : Component
{
	[Property] public GameObject Zombie { get; set; }
	public Vector3 transform { get; set; }
	protected override void OnStart()
	{
		_ = SpawnZombie();
	}

	public async Task SpawnZombie()
	{
		while (true)
		{
			Log.Info("Spawn");
			var transform = Scene.NavMesh.GetRandomPoint().GetValueOrDefault();
			var zombie = Zombie.Clone(transform);
			zombie.NetworkSpawn(null);
			await Task.DelaySeconds(5);
		}
	}
}
