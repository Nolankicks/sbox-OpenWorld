using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Security.Cryptography.X509Certificates;
using Sandbox;
using Sandbox.Utility;

public sealed class NoiseCreator : Component
{
	public float[,] Luminance { get; private set; }
	[Property] public Model model { get; set; }
	[Property] public PlayerController player { get; set; }
	[Property] public Model Rock { get; set; }
	[Property] public Material material { get; set; }
	[Property] public int mapHeight { get; set; }
	[Property] public int mapWidth { get; set; }
	[Property] public float noiseScale { get; set; }
	[Property] public GameObject chunksHolder { get; set; }
	protected override void OnAwake()
	{
		  int gridSizeX = 150;
   		 int gridSizeY = 150;

    // Define the size of each chunk
   		 int chunkSizeX = mapWidth;
    	int chunkSizeY = mapHeight;

    // Create a grid of chunks
for ( int y = 0; y < gridSizeY; y++ )
{
	for ( int x = 0; x < gridSizeX; x++ )
	{
		// Generate the noise map for this chunk
		var noise = CreateNoise( chunkSizeX, chunkSizeY, noiseScale, x * chunkSizeX, y * chunkSizeY );

		// Create the chunk data and add it to the list
		Vector3 chunkPos = new Vector3( x * chunkSizeX, y * chunkSizeY, 0 );
		string chunkName = "Chunk" + chunkPos.x + chunkPos.y;
		//var texture = noiseTexture(noise, mapWidth, mapHeight);
		chunkDataList.Add(new ChunkData { Position = chunkPos, NoiseValues = noise, Material = material, Name = chunkName, rockCount = 1, rockLocations = new List<Vector3>() });
	}
}
	}
	protected override void OnStart()
	{
		player = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy );
	}
	public class ChunkData
{
    public Vector3 Position { get; set; }
    public float[,] NoiseValues { get; set; }
    public Texture Texture { get; set; }
    public Material Material { get; set; }
    public string Name { get; set; }
	public List<Vector3> rockLocations = new List<Vector3>();
	public int rockCount;
}
	private List<ChunkData> chunkDataList = new List<ChunkData>();
	
	public void CreateChunk(Vector3 pos, float[,] noiseValues, Material chunkMaterial, List<Vector3> vector3s, string name)
{
    // Create a texture and mesh for the chunk
    Luminance = noiseValues;
    CreateMesh(pos, chunkMaterial, vector3s, name);
}
	protected override void OnUpdate()
	{
		List<Vector3> rockLocations = new List<Vector3>(); // Declare rockLocations variable
		rockLocations.Add(new Vector3(10, 10, 10)); // Add a rock location
		for (int i = chunkDataList.Count - 1; i >= 0; i--)
		{
			var chunkData = chunkDataList[i];
			if (Vector3.DistanceBetween(player.GameObject.Transform.Position, chunkData.Position) < 1000)
			{
				CreateChunk(chunkData.Position, chunkData.NoiseValues, chunkData.Material, rockLocations, chunkData.Name);
				chunkDataList.RemoveAt(i);
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

	public Texture noiseTexture( float[,] noiseValue, int width, int height )
	{

		List<Byte> bytes = new List<Byte>();

		for ( int y = 0; y < width; y++ )
		{
			for ( int x = 0; x < height; x++ )
			{
				float lum = noiseValue[x, y] * 255f;
				lum = lum.CeilToInt().Clamp( 0, 255 );

				byte val = byte.Parse( lum.ToString() );
				bytes.Add( val );
				bytes.Add( val );
				bytes.Add( val );
				bytes.Add( 255 );
			}
		}

		var texture = Texture.Create( height, width ).WithFormat( ImageFormat.RGBA8888 ).WithData( bytes.ToArray() ).Finish();

		return texture;
	}
	public void CreateNoiseTexture()
	{
		int octavas = 4;
		float persistence = 0.5f;
		float lacunarity = 2;
		int offsetX = 0; // Define the offsetX
		int offsetY = 0; // Define the offsetY
		var noise = CreateNoise( mapWidth, mapHeight, noiseScale, offsetX, offsetY ); // Use the offsets
		//texture = noiseTexture( noise, mapWidth, mapHeight );
	}

	public void CreateMesh( Vector3 pos, Material chunkMaterial, List<Vector3> rockLocations, string name )
	{
		Mesh mesh = new Mesh();
		VertexBuffer vertexBuffer = new VertexBuffer();
		for ( int y = 0; y < mapHeight; y++ )
		{
			for ( int x = 0; x < mapWidth; x++ )
			{
				// Get the luminance values for the four corners of the quad
				float lum1 = Luminance[x, y] * 100;
				float lum2 = Luminance[x + 1, y] * 100;
				float lum3 = Luminance[x, y + 1] * 100;
				float lum4 = Luminance[x + 1, y + 1] * 100;

				// Create the four vertices of the quad
				Vertex v1 = new Vertex( RoundVector3( new Vector3( x, y, lum1 ) ), new Vector4( 1, 1, 1, 1 ), new Vector3( 0, 0, 1 ), new Vector4( 1, 0, 0, 0 ) );
				Vertex v2 = new Vertex( RoundVector3( new Vector3( x + 1, y, lum2 ) ), new Vector4( 1, 1, 1, 1 ), new Vector3( 0, 0, 1 ), new Vector4( 1, 0, 0, 0 ) );
				Vertex v3 = new Vertex( RoundVector3( new Vector3( x, y + 1, lum3 ) ), new Vector4( 1, 1, 1, 1 ), new Vector3( 0, 0, 1 ), new Vector4( 1, 0, 0, 0 ) );
				Vertex v4 = new Vertex( RoundVector3( new Vector3( x + 1, y + 1, lum4 ) ), new Vector4( 1, 1, 1, 1 ), new Vector3( 0, 0, 1 ), new Vector4( 1, 0, 0, 0 ) );

				// Add two triangles to form the quad
				vertexBuffer.AddTriangle( v1, v2, v3 );
				vertexBuffer.AddTriangle( v2, v4, v3 );
			}
		}
		//make a buffer
		mesh.CreateBuffers( vertexBuffer );
		//Create the rocks
		/*foreach ( var rockLocation in rockLocations )
		{
			var rock = new GameObject();
			var rockModelRender = rock.Components.Create<ModelRenderer>();
			rockModelRender.Model = Rock;
			rock.Transform.Position = rockLocation;
		}*/
		//Create the model
		mesh.Material = chunkMaterial;
		ModelBuilder builder = new ModelBuilder();
		builder.AddMesh(mesh);
		builder.AddCollisionBox(mapWidth, mapHeight);
		model = builder.Create();
		//Create the GameObject
		var newGameObject = new GameObject();
		var modelRenderer = newGameObject.Components.Create<ModelRenderer>();
		var chunk = newGameObject.Components.Create<Chunk>();
		chunk.player = player.GameObject;
		var modelCollider = newGameObject.Components.Create<ModelCollider>();
		modelCollider.Model = model; 
		chunk.modelRenderer = modelRenderer;
		modelRenderer.Model = model;
		newGameObject.Name = name;
		newGameObject.Transform.Position = pos;
		newGameObject.Parent = chunksHolder;
		var hitBox = newGameObject.Components.Create<BoxCollider>();
	}
	private Vector3 RoundVector3( Vector3 vector )
	{
		return new Vector3( MathF.Round( vector.x, 4 ), MathF.Round( vector.y, 4 ), MathF.Round( vector.z, 4 ) );
	}
}
