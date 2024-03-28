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
	protected override void OnUpdate()
	{
		CreateNoiseTexture();
		spriteRenderer.Texture = texture;
		CreateMesh();
		modelRenderer.Model = model;
	}

	public float[,] CreateNoise(int size = 32, int res = 1)
	{
		int pixelsize = size * res;
		Luminance = new float[pixelsize, pixelsize];

		for (int y = 0; y < pixelsize; y++)
		{
			for (int x = 0; x < pixelsize; x++)
			{
				float px = x * 10;
				float py = y * 10;
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

	public void CreateMesh()
	{
		Mesh mesh = new Mesh();
		VertexBuffer vertexBuffer = new VertexBuffer();
		for (int y = 0; y < 32; y++)
		{
			for (int x = 0; x < 32; x++)
			{
				float lum = Luminance[x, y];
				vertexBuffer.AddCube(new Vector3(x * 50, y * 50, lum * 50), new Vector3(50, 50, 50), Rotation.Identity);
			}
		}
		mesh.CreateBuffers(vertexBuffer);
		mesh.Material = material;
		
		ModelBuilder builder = new ModelBuilder();
		builder.AddMesh(mesh);
		model = builder.Create();
	}
}
