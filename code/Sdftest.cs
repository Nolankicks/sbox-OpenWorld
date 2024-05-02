using System.Net.Http;
using Sandbox;
using Sandbox.Sdf;
using Sandbox.Utility;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Sandbox.Network;

[Title("SDF Manager")]
[Icon("dashboard")]
public sealed class Sdftest : Component, Component.INetworkListener
{

	[Property] public Sdf3DWorld World { get; set; }
	[Property] public Sdf3DVolume Volume { get; set; }
	public float[,] PerlinValues { get; set; }
    [Property] public GameObject TreePrefab { get; set; }
	[Property] public GameObject RockPrefab { get; set; }
    [Property] public float Scale { get; set; }
	[Property] public float Amplitude { get; set; }
	[Property] public ProcGenUi ProcGenUi { get; set; }
    [Property] public GameObject SpawnPoint { get; set; }
	[Property] public bool StartServer { get; set; } = false;
	[Property] public GameObject PlayerPrefab { get; set; }
	public List<Biome> biomes { get; set; }
    public delegate void OnWorldSpawnedDel(Sdftest SDFManager, Sdf3DWorld world);
	[Property] public OnWorldSpawnedDel OnWorldSpawned { get; set; }
	protected override void OnStart()
	{
		biomes = new List<Biome>();
		World.GameObject.NetworkSpawn();
		_ = CreateWorld(World, Volume, Scale);
	}

public async Task CreateWorld(Sdf3DWorld world, Sdf3DVolume volume, float scale)
{
    var noiseMap = CreateNoise((int)(10 * scale), (int)(10 * scale), 1, 0, 0, Amplitude);

    for (int i = 0; i < 10 * scale; i++)
    {
        for (int j = 0; j < 10 * scale; j++)
        {
            var z = noiseMap[i, j] * 1000;
            var pos = new Vector3(i * 100, j * 100, z);
            var sphere = new SphereSdf3D(pos * scale, 100 * scale); // Adjust the radius based on the scale
			string biomeType;
            if (noiseMap[i, j] < -0.5)
            {
                biomeType = "ocean";
            }
            else if (noiseMap[i, j] < 0.5)
            {
                biomeType = "plains";
            }
            else
            {
                biomeType = "mountains";
            }

            biomes.Add(new Biome(pos, biomeType));
            await world.AddAsync(sphere, volume);
        }
    }
	LoadingScreen.Title = "Creating world...";
	await Task.DelaySeconds(1);
	OnWorldSpawned?.Invoke(this, world);

	LoadingScreen.Title = "Spawning items...";
	await Task.DelaySeconds(3);
    if (ProcGenUi is not null)
    {
        ProcGenUi.Destroy();
    }
	await Task.DelaySeconds(1);
	GameNetworkSystem.CreateLobby();
	await Task.DelaySeconds(1);
    

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



public void SpawnItem(GameObject gameObject, Sdf3DWorld world, float propbiability, int times, bool Offset, string BiomeType = "")
{
    for (int i = 0; i < times; i++)
    {
        if (GetRandom(0, 1) < propbiability)
        {
            var pos = GetBounds(world, Offset);
            var clone = gameObject.Clone(pos);
            clone.NetworkSpawn(null);
        }
    }
}
public Vector3 GetBounds(Sdf3DWorld world, bool Offset = false, string BiomeType = "")
{
    Vector3 dim = world.Dimensions * 10000 * Scale;
    int buffer = 200; // Increase buffer size

    while (true)
    {
        // Generate random position within the adjusted boundaries
        var x = GetRandom(buffer, dim.x - buffer);
        var y = GetRandom(buffer, dim.y - buffer);

        // Check if the position is valid by casting a downward ray from a higher position
		if (BiomeType == "")
		{
			        var trace = Scene.Trace.Ray(new Vector3(x, y, dim.z + 10000), Vector3.Down * 10000000).Run();

        // Validate the position
        if (trace.Hit && trace.HitPosition.x > buffer && trace.HitPosition.x < dim.x - buffer &&
            trace.HitPosition.y > buffer && trace.HitPosition.y < dim.y - buffer &&
            trace.HitPosition.z < dim.z - buffer && !trace.GameObject.Tags.Has("world")) // Make sure it's not too close to the top boundary
        {
            return trace.HitPosition + (Offset ? trace.Normal * 100 : Vector3.Zero);
        }
   		}
		else
		{
			List<Biome> filteredBiomes = biomes;
		var chosenBiome = filteredBiomes[Random.Shared.Next(filteredBiomes.Count)];

        // Check if the position is valid by casting a downward ray from a higher position
        var trace = Scene.Trace.Ray(chosenBiome.Location + new Vector3(0, 0, dim.z + 10000), Vector3.Down * 10000000).Run();

        // Validate the position
        if (trace.Hit && trace.HitPosition.x > buffer && trace.HitPosition.x < dim.x - buffer &&
            trace.HitPosition.y > buffer && trace.HitPosition.y < dim.y - buffer &&
            trace.HitPosition.z < dim.z - buffer && !trace.GameObject.Tags.Has("world")) // Make sure it's not too close to the top boundary
        {
            return trace.HitPosition + (Offset ? trace.Normal * 100 : Vector3.Zero);
        }
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
