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
	[Property, Category("Volumes")] public Sdf3DVolume Sand { get; set; }
	[Property, Category("Volumes")] public Sdf3DVolume Water { get; set; }
	[Property, Category("Volumes")] public Sdf3DVolume Snow { get; set; }
	[Property, Category("Volumes")] public Sdf3DVolume Stone { get; set; }
	[Property, Category("Volumes")] public Sdf3DVolume Swamp { get; set; }
	public float[,] PerlinValues { get; set; }
    [Property, Category("World Properties")] public float Scale { get; set; }
	[Property, Category("World Properties")] public float Amplitude { get; set; }
	public ProcGenUi ProcGenUi { get; set; }
	[Property] public GameObject PlayerPrefab { get; set; }
    public delegate Task OnWorldSpawnedDel(Sdftest SDFManager);
	[Property] public GameObject WaterTrigger { get; set; }
	[Property, Category("World Properties")] public int WorldSize { get; set; } = 20000;
	[Property, Category("World Properties")] public int OceanSize { get; set; } = 20000;
	[Property, Category("World Properties")] public int OceanHeight { get; set; } = 1500;
	[Property] public OnWorldSpawnedDel OnWorldSpawned { get; set; }
	public enum BiomeType
	{
		Grass,
		Sand,
		Snow,
		Stone,
		Swamp,
	}
	[Property, Category("World Properties")] public BiomeType Biome { get; set; }
	public Sdf3DVolume GetVolume()
	{
		switch (Biome)
		{
			case BiomeType.Grass:
				return Grass;
			case BiomeType.Sand:
				return Sand;
			case BiomeType.Snow:
				return Snow;
			case BiomeType.Stone:
				return Stone;
			case BiomeType.Swamp:
				return Swamp;
			default:
				return Grass;
		}
	}
	public bool WaterBool()
	{
		switch (Biome)
		{
			case BiomeType.Grass:
				return true;
			case BiomeType.Sand:
				return true;
			case BiomeType.Snow:
				return false;
			case BiomeType.Stone:
				return true;
			case BiomeType.Swamp:
				return true;
			default:
				return false;
		}
	}
	protected override void OnStart()
	{
		_ = TaskBuildWorld();
	}
	public async Task TaskBuildWorld()
	{
		World.GameObject.NetworkSpawn();
		WaterWorld.GameObject.NetworkSpawn();
		var biomeString = Sandbox.FileSystem.Data.ReadAllText("biome.txt");
		if (biomeString != null && biomeString != "")
		{
			Biome = (BiomeType)Enum.Parse(typeof(BiomeType), biomeString);
		}
		else
		{
			Biome = BiomeType.Grass;
		}
		await CreateWorld(World, GetVolume(), Scale, Random.Shared.Int(0, 10000));
		WaterTrigger.Transform.Position = new Vector3(WorldSize / 2, WorldSize / 2, 0);
		await OnWorldSpawned?.Invoke(this);
		if (Scene.GetAllComponents<SpawnPoint>().ToList().Count == 0) return;
		GameNetworkSystem.CreateLobby();
	}
public async Task CreateWorld(Sdf3DWorld world, Sdf3DVolume volume, float scale, int seed)
{
    if (world is null || volume is null || Water is null) return;

    int chunkSize = 1000; // Define the size of each chunk
    int numChunks = WorldSize / chunkSize; // Calculate the number of chunks

    for (int i = 0; i < numChunks; i++)
    {
        for (int j = 0; j < numChunks; j++)
        {
            Vector3 chunkPosition = new Vector3(i * chunkSize, j * chunkSize, 0);
            await world.AddAsync(new FractalPerlinNoise(seed, Vector3.Zero, chunkPosition, (Vector3.One * chunkSize).WithZ(WorldHeight), 4, 0.5f), volume);
        }
    }

    if (WaterBool())
    {
        var waterSDF = new BoxSdf3D(Vector3.Zero, new Vector3(WorldSize, WorldSize, 2500));
        await WaterWorld.AddAsync(waterSDF, Water);
    }
}
public async Task AddCube(Sdf3DWorld world, Vector3 pos, Vector3 size, Sdf3DVolume volume)
{
	var cube = new BoxSdf3D(Vector3.Zero, size).Transform(pos);
	await world.AddAsync(cube, volume);
}
public async Task SubtractCube(Sdf3DWorld world, Vector3 pos, Vector3 size, Sdf3DVolume volume)
{
	var cube = new BoxSdf3D(Vector3.Zero, size).Transform(pos);
	await world.SubtractAsync(cube, volume);
}
public async Task AddSphere(Sdf3DWorld world, Vector3 pos, float radius, Sdf3DVolume volume)
{
	var sphere = new SphereSdf3D(Vector3.Zero, radius).Transform(pos);
	await world.AddAsync(sphere, volume);
}
public async Task SubtractSphere(Sdf3DWorld world, Vector3 pos, float radius, Sdf3DVolume volume)
{
	var sphere = new SphereSdf3D(Vector3.Zero, radius).Transform(pos);
	await world.SubtractAsync(sphere, volume);
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





public async Task SpawnItem(GameObject gameObject, float propbiability, int times, bool Offset = false, bool NetworkSpawn = true)
{
    for (int i = 0; i < times; i++)
    {
        if (GetRandom(0, 1) < propbiability)
        {
            var pos = await GetBounds(Offset).ContinueWith(Task => Task.Result);
            var clone = gameObject.Clone(pos);
			Log.Info(clone);
			if (NetworkSpawn)
			{
				clone.NetworkSpawn(null);
			}
        }
    }
}
public async Task<Vector3> GetBounds( bool Offset = false)
{
	while (true)
	{
	Vector3 dim =  new Vector3(WorldSize - 200, WorldSize - 200, WorldHeight - 200);
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
