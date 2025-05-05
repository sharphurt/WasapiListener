namespace WasapiListener;

public class AudioStreamConsumer(int channels)
{
    private readonly Queue<AudioChunk> _data = new();
    private List<byte> _buffer = [];

    private const int BytesPerFrame = 2;

    public void Add(byte[] data, int length)
    {
        var normalized = NormalizeAudio(data[..length]);
        _buffer.AddRange(normalized);

        if (!TryComposeToChunks(out var chunks))
            return;

        foreach (var chunk in chunks)
            _data.Enqueue(chunk);
    }

    public IEnumerable<AudioChunk> ReadChunk()
    {
        while (_data.Count > 0)
            yield return _data.Dequeue();
    }

    private bool TryComposeToChunks(out List<AudioChunk> chunks)
    {
        if (_buffer.Count < AudioChunk.ChunkSize)
        {
            chunks = [];
            return false;
        }

        var result = new List<AudioChunk>();
        var fullChunksCount = _buffer.Count / AudioChunk.ChunkSize;
        var remainedBytesCount = _buffer.Count % AudioChunk.ChunkSize;

        for (var chunk = 0; chunk < fullChunksCount; chunk++)
        {
            var bytesForChunk = _buffer.Slice(chunk * AudioChunk.ChunkSize, AudioChunk.ChunkSize).ToArray();
            result.Add(new AudioChunk(bytesForChunk));
        }


        _buffer = _buffer.Slice(fullChunksCount * AudioChunk.ChunkSize, remainedBytesCount);

        chunks = result;
        return true;
    }

    private byte[] NormalizeAudio(byte[] data)
    {
        return channels switch
        {
            1 => data,
            2 => ConvertStereoToMono(data),
            _ => throw new NotSupportedException()
        };
    }

    private byte[] ConvertStereoToMono(byte[] data)
    {
        var dataLength = data.Length;
        var outputOffset = 0;

        var result = new byte[dataLength / channels];

        for (var frameStartIndex = 0; frameStartIndex < dataLength; frameStartIndex += BytesPerFrame * channels)
        {
            var left = BitConverter.ToInt16(data, frameStartIndex);
            var right = BitConverter.ToInt16(data, frameStartIndex + BytesPerFrame);

            var mono = (short)((left + right) / 2);
            var monoBytes = BitConverter.GetBytes(mono);
            result[outputOffset] = monoBytes[0];
            result[outputOffset + 1] = monoBytes[1];

            outputOffset += BytesPerFrame;
        }

        return result;
    }
}