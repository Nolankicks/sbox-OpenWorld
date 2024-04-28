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
	[Property] public List<GameObject> ItemsToSpawnAfterWorld { get; set; } = new();
	public float[,] PerlinValues { get; set; }
    [Property] public GameObject TreePrefab { get; set; }
	[Property] public GameObject RockPrefab { get; set; }
    [Property] public float Scale { get; set; }
	[Property] public float Amplitude { get; set; }
	[Property] public ProcGenUi ProcGenUi { get; set; }
    [Property] public GameObject SpawnPoint { get; set; }
	[Property] public bool StartServer { get; set; } = false;
	public delegate void RandomAction(Sdftest SDFManager, Sdf3DWorld world);
	[Property] public List<RandomAction> randomActions { get; set; } = new();
	[Property] public GameObject PlayerPrefab { get; set; }
    [Property] public Action OnWorldSpawned { get; set; }
	protected override void OnStart()
	{
		World.GameObject.NetworkSpawn();
		_ = CreateWorld(World, Volume, Scale);
	}

public async Task CreateWorld(Sdf3DWorld world, Sdf3DVolume volume, float scale)
{
    var cube = new BoxSdf3D(0, 1000 * scale);
    //world.AddAsync(cube, volume);

    var noiseMap = CreateNoise((int)(10 * scale), (int)(10 * scale), 1, 0, 0, Amplitude);

    for (int i = 0; i < 10 * scale; i++)
    {
        for (int j = 0; j < 10 * scale; j++)
        {
            var z = noiseMap[i, j] * 1000;
            var pos = new Vector3(i * 100, j * 100, z);
            var sphere = new SphereSdf3D(pos * scale, 100 * scale); // Adjust the radius based on the scale
            await world.AddAsync(sphere, volume);
        }
    }
	LoadingScreen.Title = "Creating world...";
	await Task.DelaySeconds(1);
	foreach (var action in randomActions)
	{
		action?.Invoke(this, world);
	}

	LoadingScreen.Title = "Spawning items...";
	await Task.DelaySeconds(3);
   
	foreach(var gameObject in ItemsToSpawnAfterWorld)
	{
		gameObject.Clone();
	}
    if (ProcGenUi is not null)
    {
        ProcGenUi.Destroy();
    }
	await Task.DelaySeconds(1);
	GameNetworkSystem.CreateLobby();
	await Task.DelaySeconds(1);
    OnWorldSpawned?.Invoke();

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



public void SpawnItem(GameObject gameObject, Sdf3DWorld world, float propbiability, int times, bool Offset)
{
    for (int i = 0; i < times; i++)
    {
        if (GetRandom(0, 1) < propbiability)
        {
            var pos = GetBounds(world, Offset);
            var clone = gameObject.Clone(pos);
            Log.Info(clone.Transform.Position);
        }
    }
}
public Vector3 GetBounds(Sdf3DWorld world, bool Offset = false)
{
    Vector3 dim = world.Dimensions * 10000 * Scale;
    int buffer = 200; // Increase buffer size

    while (true)
    {
        // Generate random position within the adjusted boundaries
        var x = GetRandom(buffer, dim.x - buffer);
        var y = GetRandom(buffer, dim.y - buffer);

        // Check if the position is valid by casting a downward ray from a higher position
        var trace = Scene.Trace.Ray(new Vector3(x, y, dim.z + 5000), Vector3.Down * 10000).Run();

        // Validate the position
        if (trace.Hit && trace.HitPosition.x > buffer && trace.HitPosition.x < dim.x - buffer &&
            trace.HitPosition.y > buffer && trace.HitPosition.y < dim.y - buffer &&
            trace.HitPosition.z < dim.z - buffer && !trace.GameObject.Tags.Has("world")) // Make sure it's not too close to the top boundary
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

            noiseMap[x, y] = noise * amplitude; // Scale the noise by the amplitude
        }
    }

    return noiseMap;
}
}
