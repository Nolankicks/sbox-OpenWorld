using System.Net.Http;
using Sandbox;
using Sandbox.Sdf;
using Sandbox.Utility;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

[Title("SDF Manager")]
[Icon("dashboard")]
public sealed class Sdftest : Component
{

	[Property] public Sdf3DWorld World { get; set; }
	[Property] public Sdf3DVolume Volume { get; set; }
	[Property] public List<GameObject> ItemsToSpawnAfterWorld { get; set; } = new();
	public float[,] PerlinValues { get; set; }
    [Property] public GameObject TreePrefab { get; set; }
	[Property] public GameObject RockPrefab { get; set; }
    [Property] public float Scale { get; set; }
	[Property] public float Amplitude { get; set; }

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
	foreach(var gameObject in ItemsToSpawnAfterWorld)
	{
		gameObject.Clone();
	}
    for (int i = 0; i < 150; i++)
    {
		CreateSpawnPoints(world);
        CreateTree(TreePrefab, world);	
		CreateRock(RockPrefab, world);
    }

}
void CreateSpawnPoints(Sdf3DWorld world)
{
	var pos = GetBounds(world);
	var spawn = new GameObject();
	spawn.Components.Create<SpawnPoint>();
	spawn.Clone(pos);
}

void CreateTree(GameObject TreePrefab, Sdf3DWorld world)
{
    TreePrefab.Clone(GetBounds(world));
}

void CreateRock(GameObject RockPrefab, Sdf3DWorld world)
{
	RockPrefab.Clone(GetBounds(world));
}

public Vector3 GetBounds(Sdf3DWorld world)
{
    Vector3 dim = world.Dimensions * 10000 * Scale;
    while (true)
    {
        var x = GetRandom(0, dim.x);
        var y = GetRandom(0, dim.y);
        var trace = Scene.Trace.Ray(new Vector3(x, y, 5000), Vector3.Down * 10000).Run();
        if (trace.Hit && !trace.GameObject.Tags.Has("world"))
        {
            return trace.HitPosition;
        }
    }
}

void RandomEvent(float propbiability, GameObject prefab)
{
    if (GetRandom(0, 1) < propbiability)
    {
        prefab.Clone(GetBounds(World));
    }
}
[Title("Random Event")]
[Icon("water")]
public class RandomEventComponent : Component
{
	/// <summary>
    /// A number between 0 and 1, with 1 being certain and 0 being impossible
    /// </summary>
   	[Property, Range(0, 1)]  public float Probability { get; set; }

    [Property] public GameObject Prefab { get; set; }
	[Property] public Action OnSpawn { get; set; }
    public Sdftest Sdftest { get; set; }
	[Property, Range(1, 100)] public int TimesInvoked { get; set; }

	protected override void OnStart()
	{
		Sdftest = Scene.GetAllComponents<Sdftest>().FirstOrDefault();
		_ = SpawnEvent();
	}

	public async Task SpawnEvent()
	{
		await Sdftest.CreateWorld(Sdftest.World, Sdftest.Volume, Sdftest.Scale);
		for (int i = 0; i < TimesInvoked; i++)
		{
		Sdftest.RandomEvent(Probability, Prefab);
		OnSpawn?.Invoke();
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
