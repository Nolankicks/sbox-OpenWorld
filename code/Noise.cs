using Sandbox.Utility;

namespace Sandbox.Sdf.Noise
{
    public record struct PerlinNoiseSdf3D(int Seed, float noiseScalar, Vector3 Position, Vector3 SizeOfArea) : ISdf3D
    {
        public BBox? Bounds => new BBox(Position, Position + SizeOfArea);
		
        public float this[Vector3 pos]
        {
            get
            {
                var xNoise = ((pos.x - Position.x) * noiseScalar) / 10;
                var yNoise = ((pos.y - Position.y) * noiseScalar) / 10;
                var noiseValue = Utility.Noise.Perlin(xNoise + Seed, yNoise + Seed);
                noiseValue *= SizeOfArea.z;
                return pos.z - (Position.z + noiseValue);
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
            return new PerlinNoiseSdf3D(reader.Read<int>(), reader.Read<float>(), reader.Read<Vector3>(), reader.Read<Vector3>());
        }
    }
}
