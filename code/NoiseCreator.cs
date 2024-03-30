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
	public Vector3[] vertices;
	protected override void OnAwake()
{
    // Define the size of the grid
    int gridSizeX = 5;
    int gridSizeY = 5;

    // Define the size of each chunk
    int chunkSizeX = mapWidth;
    int chunkSizeY = mapHeight;

    // Generate a larger noise map
    var noise = CreateNoise(gridSizeX * chunkSizeX, gridSizeY * chunkSizeY, noiseScale);

    // Create a grid of chunks
    for (int y = 0; y < gridSizeY; y++)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            // Extract the noise values for this chunk
            float[,] chunkNoise = new float[chunkSizeX, chunkSizeY];
            for (int cy = 0; cy < chunkSizeY; cy++)
            {
                for (int cx = 0; cx < chunkSizeX; cx++)
                {
                    chunkNoise[cx, cy] = noise[x * chunkSizeX + cx, y * chunkSizeY + cy];
                }
            }

            // Create the chunk
            Vector3 chunkPos = new Vector3(x * chunkSizeX, y * chunkSizeY, 0);
            CreateChunk(chunkPos, chunkNoise);
        }
    }
}

public void CreateChunk(Vector3 pos, float[,] noiseValues)
{
    // Create a texture and mesh for the chunk
    texture = noiseTexture(noiseValues, mapWidth, mapHeight);
    spriteRenderer.Texture = texture;
    Luminance = noiseValues;
    string chunkName = "Chunk" + pos.x + pos.y;
    CreateMesh(pos, material, chunkName);
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
        float sampleX = x / scale;
        float sampleY = y / scale;

        float perlinValue = Noise.Perlin(sampleX, sampleY) * 2 - 1; // Adjust range to [-1, 1]
        Luminance[x, y] = perlinValue;

        // If this is not the first row or first column, adjust the noise value to be the average of this cell and the adjacent cells
        if (x > 0 && y > 0)
        {
            Luminance[x, y] = (Luminance[x, y] + Luminance[x - 1, y] + Luminance[x, y - 1] + Luminance[x - 1, y - 1]) / 4;
        }
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

	public void CreateMesh(Vector3 pos, Material chunkMaterial, string name)
{
    Mesh mesh = new Mesh();
    VertexBuffer vertexBuffer = new VertexBuffer();
    for (int y = 0; y < mapHeight; y++)
{
    for (int x = 0; x < mapWidth; x++)
    {
        // Get the luminance values for the four corners of the quad
        float lum1 = MathF.Round(Luminance[x, y] * 50, 2);
        float lum2 = x < mapWidth - 1 ? MathF.Round(Luminance[x + 1, y] * 50, 2) : lum1;
        float lum3 = y < mapHeight - 1 ? MathF.Round(Luminance[x, y + 1] * 50, 2) : lum1;
        float lum4 = x < mapWidth - 1 && y < mapHeight - 1 ? MathF.Round(Luminance[x + 1, y + 1] * 50, 2) : lum1;

        // Create the four vertices of the quad
        Vertex v1 = new Vertex(new Vector3(x, y, lum1), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
        Vertex v2 = new Vertex(new Vector3(x + 1, y, lum2), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
        Vertex v3 = new Vertex(new Vector3(x, y + 1, lum3), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));
        Vertex v4 = new Vertex(new Vector3(x + 1, y + 1, lum4), new Vector4(1, 1, 1, 1), new Vector3(0, 0, 1), new Vector4(1, 0, 0, 0));

        // Add two triangles to form the quad
        vertexBuffer.AddTriangle(v1, v2, v3);
        vertexBuffer.AddTriangle(v2, v4, v3);
    }
}

    mesh.CreateBuffers(vertexBuffer);
    mesh.Material = chunkMaterial;
    ModelBuilder builder = new ModelBuilder();
    builder.AddMesh(mesh);
    model = builder.Create();
    var newGameObject = new GameObject();
    newGameObject.Components.Create<ModelRenderer>();
    var modelRenderer = newGameObject.Components.Get<ModelRenderer>();
    modelRenderer.Model = model;
    newGameObject.Name = name;
    newGameObject.Transform.Position = pos;
}
}
