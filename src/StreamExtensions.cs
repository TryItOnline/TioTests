using System.IO;

namespace TioTests
{
    public static class StreamExtensions
    {
        public static void Write(this Stream stream, byte[] data)
        {
            stream.Write(data,0,data.Length);
        }
    }
}