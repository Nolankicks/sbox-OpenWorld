using System.Net.Http;
using Sandbox;
using Sandbox.Sdf;
using Sandbox.Utility;
using System.Threading.Tasks;
public sealed class Sdftest : Component
{

	[Property] public Sdf3DWorld World { get; set; }
	[Property] public Sdf3DVolume Volume { get; set; }
	[Property] public GameObject NetworkManager { get; set; }
	public float[,] PerlinValues { get; set; }

	protected override void OnStart()
	{
		World.GameObject.NetworkSpawn();
		_ = CreateWorld(World, Volume, 5);
	}

public async Task CreateWorld(Sdf3DWorld world, Sdf3DVolume volume, float scale)
{
    var cube = new BoxSdf3D(0, 1000 * scale);
    //world.AddAsync(cube, volume);

    var noiseMap = CreateNoise((int)(10 * scale), (int)(10 * scale), 1, 0, 0);

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
	NetworkManager.Clone();

}
public float[,] CreateNoise(int width, int height, float scale, int offsetX, int offsetY)
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

		noiseMap[x, y] = noise;
	}
}

    return noiseMap;
}
}
