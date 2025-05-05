namespace WasapiListener;

public class AudioChunk(byte[] data)
{
    public static readonly int ChunkSize = 1024;

    public byte[] Data { get; } = data;
}