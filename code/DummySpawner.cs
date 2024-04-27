using System.Threading.Tasks;
using Kicks;
using Sandbox;

public sealed class DummySpawner : Component
{
	[Property] public GameObject Dummy { get; set; }
	[Property] public List<GameObject> Dummies { get; set; } = new();
	[Property] public List<GameObject> SpawnLocations { get; set; } = new();
	[Property] public bool DestroyAll { get; set; }
	[Property] public bool UseNavMesh { get; set; } = false;

	protected override void OnStart()
	{
		_ = SpawnDummy();
	}

	public async Task SpawnDummy()
	{
		while (true)
		{
			if (Connection.All.Count < 1) return;
			var pos = UseNavMesh ? Scene.NavMesh.GetRandomPoint().GetValueOrDefault() :  Game.Random.FromList(SpawnLocations).Transform.Position;			
			var dummy = Dummy.Clone(pos);
			Dummies.Add(dummy);
			try
			{
				await Task.DelaySeconds(2);
			}
			catch (TaskCanceledException)
			{
				return;
			}
		}
	}

	protected override void OnUpdate()
	{
		if (Connection.All.Count > 1 || DestroyAll)
		{
				DestroyDummies();
		}
	}
	[Broadcast]
	public void DestroyDummies()
	{
		if (IsProxy) return;
		foreach (var dummy in Dummies)
		{
			dummy.Destroy();
		}
		Dummies.Clear();
	}
}
