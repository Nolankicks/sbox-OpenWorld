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
	protected override void OnStart()
	{
		for (int y = 0; y < 10; y++)
		{
			for (int x = 0; x < 10; x++)
			{
			CreateNoiseTexture();
			spriteRenderer.Texture = texture;
			CreateMesh(new Vector3(x * 1600 , y * 1600, 0));
			}
		}
	}

	public float[,] CreateNoise(int size = 32, int res = 1)
{
    int pixelsize = size * res;
    Luminance = new float[pixelsize, pixelsize];

    Random random = new Random();

    float offsetX = (float)random.NextDouble() * 10000;
    float offsetY = (float)random.NextDouble() * 10000;

    for (int y = 0; y < pixelsize; y++)
    {
        for (int x = 0; x < pixelsize; x++)
        {
            float px = x * 10 + offsetX;
            float py = y * 10 + offsetY;
            float noise = Noise.Perlin(px, py);
            Luminance[x, y] = noise;
        }
    }
    return Luminance;
}

	public Texture noiseTexture(float[,] noiseValue)
	{
		List<Byte> bytes = new List<Byte>();

		for (int y = 0; y < 32; y++)
		{
			for (int x = 0; x < 32; x++)
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

		var texture = Texture.Create(32, 32).WithFormat(ImageFormat.RGBA8888).WithData(bytes.ToArray()).Finish();

		return texture;
	}
	public void CreateNoiseTexture()
	{
		var noise = CreateNoise();
		texture = noiseTexture(noise);
	}

	public void CreateMesh(Vector3 pos)
	{
		Mesh mesh = new Mesh();
		VertexBuffer vertexBuffer = new VertexBuffer();
	for (int y = 0; y < 31; y++)
{
    for (int x = 0; x < 31; x++)
    {
        // Get the luminance values for the four corners of the quad
        float lum1 = Luminance[x, y];
        float lum2 = Luminance[x + 1, y];
        float lum3 = Luminance[x, y + 1];
        float lum4 = Luminance[x + 1, y + 1];

        // Create the four vertices of the quad
        Vertex v1 = new Vertex(new Vector3(x * 50, y * 50, lum1 * 500));
        Vertex v2 = new Vertex(new Vector3((x + 1) * 50, y * 50, lum2 * 500));
        Vertex v3 = new Vertex(new Vector3(x * 50, (y + 1) * 50, lum3 * 500));
        Vertex v4 = new Vertex(new Vector3((x + 1) * 50, (y + 1) * 50, lum4 * 500));

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
