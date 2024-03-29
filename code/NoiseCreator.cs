using System;
using Sandbox;
using Sandbox.Utility;

public sealed class NoiseCreator : Component
{
	public float[,] Luminance { get; private set; }
	[Property] public Texture texture { get; private set; }
	[Property] public SpriteRenderer spriteRenderer { get; set; }
	[Property] public Model model { get; set; }
	[Property] public ModelRenderer modelRenderer { get; set; }
	[Property] public Material material { get; set; }
	[Property] public int mapHeight { get; set; }
	[Property] public int mapWidth { get; set; }
	[Property] public float noiseScale { get; set; }
	protected override void OnStart()
	{
			for (int y = 0; y < 10; y++)
			{
				for (int x = 0; x < 10; x++)
				{
				CreateNoiseTexture();
				spriteRenderer.Texture = texture;
				CreateMesh(new Vector3(x * 3000, y * 3000, 0));
				}
			}

	}
	protected override void OnUpdate()
	{
		CreateNoiseTexture();
		spriteRenderer.Texture = texture;
	}
	public float[,] CreateNoise(int mapWidth, int mapHeight, float scale)
{	
	Luminance = new float[mapWidth, mapHeight];
	if (scale <= 0)
	{
		scale = 0.0001f;
	}
    for (int y = 0; y < mapHeight; y++)
	{
		for (int x = 0; x < mapWidth; x++)
		{
			float sampleX = x * scale;
			float sampleY = y * scale;

			float perlinValue = Noise.Perlin(sampleX, sampleY);
			Luminance[x, y] = perlinValue;
		}
	}
	return Luminance;
}

	public Texture noiseTexture(float[,] noiseValue, int width, int height)
	{

		List<Byte> bytes = new List<Byte>();

		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < height; x++)
			{
  				float lum = noiseValue[x, y] * 255f;
                lum = lum.CeilToInt().Clamp(0, 255);

                byte val = byte.Parse(lum.ToString());
				bytes.Add(val);
				bytes.Add(val);
				bytes.Add(val);
				bytes.Add(255);
			}
		}

		var texture = Texture.Create(height, width).WithFormat(ImageFormat.RGBA8888).WithData(bytes.ToArray()).Finish();

		return texture;
	}
	public void CreateNoiseTexture()
	{	
		int octavas = 4;
		float persistence = 0.5f;
		float lacunarity = 2;
		var noise = CreateNoise(mapWidth, mapHeight, noiseScale);
		texture = noiseTexture(noise, mapWidth, mapHeight);
	}

	public void CreateMesh(Vector3 pos)
{
    Mesh mesh = new Mesh();
    VertexBuffer vertexBuffer = new VertexBuffer();
    for (int y = 0; y < mapHeight - 1; y++)
    {
        for (int x = 0; x < mapWidth - 1; x++)
        {
            // Get the luminance values for the four corners of the quad
            float lum1 = Luminance[x, y];
            float lum2 = Luminance[x + 1, y];
            float lum3 = Luminance[x, y + 1];
            float lum4 = Luminance[x + 1, y + 1];

            // Create the four vertices of the quad
Vertex v1 = new Vertex(new Vector3(x * 100, y * 100, lum1 * 1000), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
Vertex v2 = new Vertex(new Vector3((x + 1) * 100, y * 100, lum2 * 1000), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
Vertex v3 = new Vertex(new Vector3(x * 100, (y + 1) * 100, lum3 * 1000), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
Vertex v4 = new Vertex(new Vector3((x + 1) * 100, (y + 1) * 100, lum4 * 1000), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));

// Add two triangles to form the quad
vertexBuffer.AddTriangle(v1, v2, v3);
vertexBuffer.AddTriangle(v2, v4, v3);
        }
    }


		mesh.CreateBuffers(vertexBuffer);
		mesh.Material = material;
		ModelBuilder builder = new ModelBuilder();
		builder.AddMesh(mesh);
		model = builder.Create();
		var newGameObject = new GameObject();
		newGameObject.Components.Create<ModelRenderer>();
		var modelRenderer = newGameObject.Components.Get<ModelRenderer>();
		modelRenderer.Model = model;
		newGameObject.Transform.Position = pos;
	}
}
