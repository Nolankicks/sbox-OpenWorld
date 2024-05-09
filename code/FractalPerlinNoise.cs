using Sandbox.Utility;

namespace Sandbox.Sdf.Noise
{
    public record struct FractalPerlinNoise(int Seed, Vector3 GlobalPosition, Vector3 ChunkPosition, Vector3 SizeOfArea, int Octaves, float Persistence, float amp = 1.5f) : ISdf3D
{
    public BBox? Bounds => new BBox(ChunkPosition, ChunkPosition + SizeOfArea);
    
    public float this[Vector3 pos]
    {
        get
        {
            float total = 0;
            float frequency = 1;
            float amplitude = amp;
            float maxValue = 0;
            for(int i=0;i<Octaves;i++) {
                var xNoise = (pos.x * 0.125f * frequency) / 10;
                var yNoise = (pos.y * 0.125f * frequency) / 10;
                var noiseValue = Utility.Noise.Perlin(xNoise + Seed, yNoise + Seed);
                
                total += noiseValue * amplitude;
                
                maxValue += amplitude;
                
                amplitude *= Persistence;
                frequency *= 2;
            }
            
            total /= maxValue;
            
            total *= SizeOfArea.z;
            return pos.z - (ChunkPosition.z + total);
        }
    }

    public void WriteRaw(ref ByteStream writer, Dictionary<TypeDescription, int> sdfTypes)
    {
        writer.Write(Seed);
        writer.Write(GlobalPosition);
        writer.Write(ChunkPosition);
        writer.Write(SizeOfArea);
        writer.Write(Octaves);
        writer.Write(Persistence);
		writer.Write(amp);
    }

    public static FractalPerlinNoise ReadRaw(ref ByteStream reader, IReadOnlyDictionary<int, SdfReader<ISdf3D>> sdfTypes)
    {
        return new FractalPerlinNoise(reader.Read<int>(), reader.Read<Vector3>(), reader.Read<Vector3>(), reader.Read<Vector3>(), reader.Read<int>(), reader.Read<float>());
    }
}
}
