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
			_ = GetNewPos();
			await Task.DelaySeconds(1);
			var zombie = Zombie.Clone(transform);
			await Task.DelaySeconds(5);
		}
	}
	async Task GetNewPos()
	{
		transform = await GetPos();
	}

	public async Task<Vector3> GetPos()
	{
		await Task.DelaySeconds(1);
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToList();
		return Game.Random.FromList(spawnPoints).Transform.Position;
	}
}
