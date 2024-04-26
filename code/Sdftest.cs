using System.Net.Http;
using Sandbox;
using Sandbox.Sdf;
using Sandbox.Utility;

public sealed class Sdftest : Component
{

	[Property] public Sdf3DWorld World { get; set; }
	[Property] public Sdf3DVolume Volume { get; set; }
	[Property] public int WorldLength { get; set; }
	[Property] public int WorldWidth { get; set; }
	public float[,] PerlinValues { get; set; }

	protected override void OnStart()
	{
		World.GameObject.NetworkSpawn();
		CreateWorld(World, Volume);
	}

public void CreateWorld(Sdf3DWorld world, Sdf3DVolume volume)
{
    var cube = new BoxSdf3D(0, 1000);
    world.AddAsync(cube, volume);

    var noiseMap = CreateNoise(10, 10, 1, 0, 0);

    for (int i = 0; i < 10; i++)
    {
        for (int j = 0; j < 10; j++)
        {
            var z = noiseMap[i, j] * 1000; // Scale the noise value to match your world's scale
            Log.Info(1000 - z);
            var pos = new Vector3(i * 100, j * 100, 1000 - z); // Subtract z from the height of the SDF
            var sphere = new SphereSdf3D(pos, 100);
            world.SubtractAsync(sphere, volume);
        }
    }
}
	public float[,] CreateNoise( int width, int height, float scale, int offsetX, int offsetY )
	{
		float[,] noiseMap = new float[width + 1, height + 1];

		for ( int y = 0; y < height + 1; y++ )
		{
			for ( int x = 0; x < width + 1; x++ )
			{
				float sampleX = (x + offsetX) / scale;
				float sampleY = (y + offsetY) / scale;

				float noise = Noise.Perlin( sampleX, sampleY );

				noiseMap[x, y] = noise;
			}
		}

		return noiseMap; // return noiseMap instead of Luminance
	}
}
