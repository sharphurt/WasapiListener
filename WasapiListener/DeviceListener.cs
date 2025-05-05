using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace WasapiListener;

public class DeviceListener
{
    private readonly MMDevice _device;
    private readonly WasapiCapture _capture;

    private readonly AudioStreamConsumer _consumer;

    private DeviceListener(MMDevice device, WasapiCapture capture)
    {
        _device = device;
        _capture = capture;
        _consumer = new AudioStreamConsumer(device.AudioClient.MixFormat.Channels);
    }

    public static DeviceListener CreateMicListener()
    {
        var enumerator = new MMDeviceEnumerator();
        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
        var capture = new WasapiCapture(device);

        var sampleRate = device.AudioClient.MixFormat.SampleRate;
        var channels = device.AudioClient.MixFormat.Channels;
        capture.WaveFormat = new WaveFormat(sampleRate, channels);

        return new DeviceListener(device, capture);
    }
    
    public static DeviceListener CreateLoopbackListener()
    {
        var enumerator = new MMDeviceEnumerator();
        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        var capture = new WasapiLoopbackCapture(device);

        var sampleRate = device.AudioClient.MixFormat.SampleRate;
        var channels = device.AudioClient.MixFormat.Channels;
        capture.WaveFormat = new WaveFormat(sampleRate, channels);

        return new DeviceListener(device, capture);
    }

    public void StartListening()
    {
        _capture.DataAvailable += OnDataAvailable;
        _capture.StartRecording();
    }

    public void StopListening()
    {
        _capture.DataAvailable -= OnDataAvailable;
        _capture.StopRecording();
    }

    public void SaveToFile()
    {
        var writer = new WaveFileWriter($"{_device.FriendlyName}.wav", new WaveFormat(48000, 1));
        foreach (var chunk in _consumer.ReadChunk())
        {
            writer.Write(chunk.Data, 0, AudioChunk.ChunkSize);
        }
        
        writer.Dispose();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        _consumer.Add(e.Buffer, e.BytesRecorded);
    }
}