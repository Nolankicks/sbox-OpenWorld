namespace Sandbox.Sdf.Noise
{
    public record struct PerlinNoiseSdf3D(int Seed, Vector3 Position, Vector3 SizeOfArea, float[,] Noise) : ISdf3D
    {
        public PerlinNoiseSdf3D(int Seed, Vector3 Position, Vector3 SizeOfArea)
        : this(Seed, Position, SizeOfArea, CreateNoise(Seed, SizeOfArea))
        {
        }

        //Stole Kick's code, line 180
        //https://github.com/Nolankicks/sbox-OpenWorld/blob/main/code/Sdftest.cs
        public static float[,] CreateNoise(int seed, Vector3 SizeOfArea)
        {
            var width = (int)((SizeOfArea.x / 16) + 1);
            var length = (int)((SizeOfArea.y / 16) + 1);
            float[,] noiseMap = new float[width, length];

            for (int y = 0; y < width; y ++)
            {
                for (int x = 0; x < length; x ++)
                {
                    float sampleX = (x + seed);
                    float sampleY = (y + seed);

                    float noise = Utility.Noise.Perlin(sampleX, sampleY);

                    noiseMap[x, y] = (noise * 0.5f) + 0.5f;
                    Log.Info($"{x},{y}: {noise}");
                }
            }

            return noiseMap;
        }
		public BBox? Bounds => new BBox(Position, Position + SizeOfArea);

public float this[Vector3 pos]
{
    get
    {
        var noiseX = (int)((pos.x - Position.x) / 16);
        var noiseY = (int)((pos.y - Position.y) / 16);

        // Wrap the noise coordinates around to the other side of the noise map
        noiseX = (noiseX % (int)(SizeOfArea.x / 16) + (int)(SizeOfArea.x / 16)) % (int)(SizeOfArea.x / 16);
        noiseY = (noiseY % (int)(SizeOfArea.y / 16) + (int)(SizeOfArea.y / 16)) % (int)(SizeOfArea.y / 16);

        return pos.z - (Position.z + (Noise[noiseX, noiseY] * SizeOfArea.z));
    }
}

        public void WriteRaw(ref ByteStream writer, Dictionary<TypeDescription, int> sdfTypes)
        {
            writer.Write(Seed);
            writer.Write(Position);
            writer.Write(SizeOfArea);
        }

        public static PerlinNoiseSdf3D ReadRaw(ref ByteStream reader, IReadOnlyDictionary<int, SdfReader<ISdf3D>> sdfTypes)
        {
            return new PerlinNoiseSdf3D(reader.Read<int>(), reader.Read<Vector3>(), reader.Read<Vector3>());
        }
    }
}
