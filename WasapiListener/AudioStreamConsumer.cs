namespace WasapiListener;

public class AudioStreamConsumer
{
    private readonly Queue<AudioChunk> _data = new();
    private List<byte> _buffer = [];
    private readonly int _channels;

    public AudioStreamConsumer(int channels)
    {
        if (channels <= 0 || channels > 2)
        {
            throw new ArgumentException("Channels must be 1 or 2");
        }
        
        _channels = channels;
    }

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
        if (_channels < 2)
            return NormalizeVolume(data);
        
        var mono = ConvertToMono(data);
        return NormalizeVolume(mono);
    }

    private byte[] NormalizeVolume(byte[] data, float targetAmplitude = 25000f)
    {
        var dataLength = data.Length;
        
        var result = new byte[dataLength];
        var maxAmplitude = float.MinValue;
        
        for (var frameStart = 0; frameStart < dataLength; frameStart += BytesPerFrame)
        {
            var frame = Math.Abs(BitConverter.ToInt16(data, frameStart));
            if (frame > maxAmplitude) 
                maxAmplitude = frame;
        }
        
        var coefficient = targetAmplitude / maxAmplitude;

        for (var frameStart = 0; frameStart < dataLength; frameStart += BytesPerFrame)
        {
            var frame = BitConverter.ToInt16(data, frameStart);
            var adjusted = (short) Math.Clamp(frame * coefficient, short.MinValue, short.MaxValue);
            var adjustedBytes = BitConverter.GetBytes(adjusted);
            result[frameStart] = adjustedBytes[0];
            result[frameStart + 1] = adjustedBytes[1];
        }

        return result;
    }

    private byte[] ConvertToMono(byte[] data)
    {
        var dataLength = data.Length;
        var outputOffset = 0;

        var result = new byte[dataLength / _channels];

        for (var frameStart = 0; frameStart < dataLength; frameStart += BytesPerFrame * _channels)
        {
            var left = BitConverter.ToInt16(data, frameStart);
            var right = BitConverter.ToInt16(data, frameStart + BytesPerFrame);

            var mono = (short)((left + right) / 2);
            var monoBytes = BitConverter.GetBytes(mono);
            result[outputOffset] = monoBytes[0];
            result[outputOffset + 1] = monoBytes[1];

            outputOffset += BytesPerFrame;
        }

        return result;
    }
}