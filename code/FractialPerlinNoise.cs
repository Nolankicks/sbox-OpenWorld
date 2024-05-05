using Sandbox.Utility;

namespace Sandbox.Sdf.Noise
{
    public record struct FractialPerlinNoise(int Seed, Vector3 Position, Vector3 SizeOfArea, int Octaves, float Persistence) : ISdf3D
{
    public BBox? Bounds => new BBox(Position, Position + SizeOfArea);
    
    public float this[Vector3 pos]
    {
        get
        {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0
            for(int i=0;i<Octaves;i++) {
                var xNoise = ((pos.x - Position.x) * 0.125f * frequency) / 10;
                var yNoise = ((pos.y - Position.y) * 0.125f * frequency) / 10;
                var noiseValue = Utility.Noise.Perlin(xNoise + Seed, yNoise + Seed);
                
                total += noiseValue * amplitude;
                
                maxValue += amplitude;
                
                amplitude *= Persistence;
                frequency *= 2;
            }
            
            total /= maxValue;
            
            total *= SizeOfArea.z;
            return pos.z - (Position.z + total);
        }
    }

        public void WriteRaw(ref ByteStream writer, Dictionary<TypeDescription, int> sdfTypes)
        {
            writer.Write(Seed);
            writer.Write(Position);
            writer.Write(SizeOfArea);
			writer.Write(Octaves);
			writer.Write(Persistence);
        }

        public static FractialPerlinNoise ReadRaw(ref ByteStream reader, IReadOnlyDictionary<int, SdfReader<ISdf3D>> sdfTypes)
        {
            return new FractialPerlinNoise(reader.Read<int>(), reader.Read<Vector3>(), reader.Read<Vector3>(), reader.Read<int>(), reader.Read<float>());
        }
    }
}
