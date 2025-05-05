using System.Runtime.InteropServices;

namespace WasapiListener;

public static class SpeexAec
{
    [DllImport("libspeexdsp.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr speex_echo_state_init(int frame_size, int filter_length);

    [DllImport("libspeexdsp.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void speex_echo_cancellation(IntPtr st, byte[] rec, byte[] play, byte[] outFrame);

    [DllImport("libspeexdsp.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void speex_echo_state_destroy(IntPtr st);
}