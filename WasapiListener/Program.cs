using NAudio.CoreAudioApi;

namespace WasapiListener;

public class Program
{
    public static void Main(string[] args)
    {
        var loopbackListener = DeviceListener.CreateLoopbackListener();
        var micListener = DeviceListener.CreateMicListener();
        
        Console.WriteLine("Listening for microphone and loopback");
        micListener.StartListening();
        loopbackListener.StartListening();
        Console.ReadLine();
        micListener.StopListening();
        loopbackListener.StopListening();
        Console.WriteLine("Stopping listening for device");
        micListener.SaveToFile();
        loopbackListener.SaveToFile();
        Console.WriteLine("Saved to file");
    }
}