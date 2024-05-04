using System.Net.Http;
using Sandbox;
using Sandbox.Sdf;
using Sandbox.Utility;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Sandbox.Network;
using Sandbox.Sdf.Noise;

[Title("SDF Manager")]
[Icon("dashboard")]
public sealed class Sdftest : Component, Component.INetworkListener
{

	[Property] public Sdf3DWorld World { get; set; }
	[Property] public Sdf3DWorld WaterWorld { get; set; }
	[Property, Category("World Properties")] public int WorldHeight { get; set; } = 3000;
	[Property, Category("Volumes")] public Sdf3DVolume Grass { get; set; }
	[Property, Category("Volumes")] public Sdf3DVolume Stone { get; set; }
	[Property, Category("Volumes")] public Sdf3DVolume Water { get; set; }
	public float[,] PerlinValues { get; set; }
    [Property, Category("World Properties")] public float Scale { get; set; }
	[Property, Category("World Properties")] public float Amplitude { get; set; }
	public ProcGenUi ProcGenUi { get; set; }
	[Property] public GameObject PlayerPrefab { get; set; }
    public delegate Task OnWorldSpawnedDel(Sdftest SDFManager, Sdf3DWorld world);
	[Property] public OnWorldSpawnedDel OnWorldSpawned { get; set; }
	protected override async void OnStart()
	{
		await CreateWorld(World, Grass, Scale);
		GameNetworkSystem.CreateLobby();
	}
public async Task CreateWorld(Sdf3DWorld world, Sdf3DVolume volume, float scale)
{
	World.GameObject.NetworkSpawn();
	WaterWorld.GameObject.NetworkSpawn();
	Log.Info("Network Spaned");
	var heightmap = new PerlinNoiseSdf3D(Random.Shared.Int(0, 100000), Vector3.Zero, (Vector3.One * 10000).WithZ(WorldHeight));
	Log.Info("Heightmap created");
	await world.AddAsync(heightmap, volume);
	Log.Info("Heightmap added to world");
	var waterSDF = new BoxSdf3D(new Vector3(-10000, -10000, 0), new Vector3(20000, 20000, 1500));
	await WaterWorld.AddAsync(waterSDF, Water);
	Log.Info("Water added to world");
	await OnWorldSpawned?.Invoke(this, world);
}
public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' has joined the game" );

		if ( PlayerPrefab is null )
			return;

		//
		// Find a spawn location for this player
		//
		var spawns = Scene.GetAllComponents<SpawnPoint>().ToList();
		var startLocation = Game.Random.FromList( spawns ).Transform.World.WithScale( 1 );

		// Spawn this object and make the client the owner
		var player = PlayerPrefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );
		player.NetworkSpawn( channel );
	}





public async Task SpawnItem(GameObject gameObject, Sdf3DWorld world, float propbiability, int times, bool Offset = false, bool NetworkSpawn = true)
{
    for (int i = 0; i < times; i++)
    {
        if (GetRandom(0, 1) < propbiability)
        {
            var pos = await GetBounds(world, Offset).ContinueWith(Task => Task.Result);
            var clone = gameObject.Clone(pos);
			Log.Info(clone);
			if (NetworkSpawn)
			{
				clone.NetworkSpawn(null);
			}
        }
    }
}
public async Task<Vector3> GetBounds(Sdf3DWorld world, bool Offset = false)
{
	while (true)
	{
	Vector3 dim =  new Vector3(10000 - 200, 10000 - 200, 5000 - 200);
    var x = GetRandom(0, dim.x);
    var y = GetRandom(0, dim.y);
    var trace = Scene.Trace.Ray(new Vector3(x, y, dim.z + 10000), Vector3.Down * 10000000).Run();
    if (trace.Hit && !trace.GameObject.Tags.Has("world") && !trace.GameObject.Tags.Has("water"))
    {
        return trace.HitPosition + (Offset ? trace.Normal * 100 : Vector3.Zero);
    }
	await Task.Delay(1);
	}

}


public float GetRandom(float Min, float Max)
{
    return Random.Shared.Float(Min, Max);
}

public float[,] CreateNoise(int width, int height, float scale, int offsetX, int offsetY, float amplitude)
{
    float[,] noiseMap = new float[width + 1, height + 1];

    // Create a random seed
    var random = new Random();
    var seed = random.NextDouble() * 1000;

    for (int y = 0; y < height + 1; y++)
    {
        for (int x = 0; x < width + 1; x++)
        {
            float sampleX = (float)((x + offsetX + seed) / scale);
            float sampleY = (float)((y + offsetY + seed) / scale);

            float noise = Noise.Perlin(sampleX, sampleY);

            noiseMap[x, y] = noise * amplitude;
        }
    }

    return noiseMap;
}
}
