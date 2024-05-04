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
	[Property] public int WorldHeight { get; set; } = 3000;
	[Property, Category("Volumes")] public Sdf3DVolume Grass { get; set; }
	[Property, Category("Volumes")] public Sdf3DVolume Stone { get; set; }
	[Property, Category("Volumes")] public Sdf3DVolume Water { get; set; }
	public float[,] PerlinValues { get; set; }
    [Property] public float Scale { get; set; }
	[Property] public float Amplitude { get; set; }
	public ProcGenUi ProcGenUi { get; set; }
    [Property] public GameObject SpawnPoint { get; set; }
	[Property] public bool StartServer { get; set; } = false;
    /// <summary>
    /// Distance between each SDF, deafult value is 30
    /// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }
	public List<Biome> biomes { get; set; }
    public delegate void OnWorldSpawnedDel(Sdftest SDFManager, Sdf3DWorld world);
	[Property] public OnWorldSpawnedDel OnWorldSpawned { get; set; }
	protected override void OnStart()
	{
		biomes = new List<Biome>();
		_ = CreateWorld(World, Grass, Scale);
	}
public async Task CreateWorld(Sdf3DWorld world, Sdf3DVolume volume, float scale)
{
	World.GameObject.NetworkSpawn();
	WaterWorld.GameObject.NetworkSpawn();
	await Task.DelaySeconds(1);
    var noiseMap = CreateNoise((int)(20 * scale), (int)(20 * scale), 1, 0, 0, Amplitude);
	var heightmap = new PerlinNoiseSdf3D(Random.Shared.Int(0, 100000), 0.125f, Vector3.Zero, (Vector3.One * 10000).WithZ(WorldHeight));
	await world.AddAsync(heightmap, volume);
	var waterSDF = new BoxSdf3D(Vector3.Zero, new Vector3(10000, 10000, 1500));
	await WaterWorld.AddAsync(waterSDF, Water);
	await Task.DelaySeconds(1);
	OnWorldSpawned?.Invoke(this, world);
	GameNetworkSystem.CreateLobby();
}
public class Biome
{
    public Vector3 Location { get; set; }
    public string Type { get; set; }

    public Biome(Vector3 location, string type)
    {
        Location = location;
        Type = type;
    }
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



void CreateTree(GameObject TreePrefab, Sdf3DWorld world)
{
    TreePrefab.Clone(GetBounds(world));
}



public void SpawnItem(GameObject gameObject, Sdf3DWorld world, float propbiability, int times, bool Offset = false, string BiomeType = "", bool NetworkSpawn = true)
{
    for (int i = 0; i < times; i++)
    {
        if (GetRandom(0, 1) < propbiability)
        {
            var pos = GetBounds(world, Offset);
            var clone = gameObject.Clone(pos);
			if (NetworkSpawn)
			{
				clone.NetworkSpawn(null);
			}
        }
    }
}
public Vector3 GetBounds(Sdf3DWorld world, bool Offset = false)
{
	while (true)
	{
	Vector3 dim =  new Vector3(10000 - 200, 10000 - 200, 5000 - 200);
    var x = GetRandom(0, dim.x);
    var y = GetRandom(0, dim.y);

    var trace = Scene.Trace.Ray(new Vector3(x, y, dim.z + 10000), Vector3.Down * 10000000).Run();

    Log.Info($"Ray cast hit: {trace.Hit}, GameObject tag: {trace.GameObject?.Tags}");
    if (trace.Hit && !trace.GameObject.Tags.Has("world") && !trace.GameObject.Tags.Has("water"))
    {
        return trace.HitPosition + (Offset ? trace.Normal * 100 : Vector3.Zero);
    }
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
