using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Client.Net.IO
{
    internal class PackageReader : BinaryReader
    {
        private NetworkStream _stream;

        public PackageReader(NetworkStream stream) : base(stream)
        {
            _stream = stream;
        }

        public string ReadMessage()
        {
            byte[] msgBuffer;
            var length = ReadInt32();
            msgBuffer = new byte[length];
            _stream.Read(msgBuffer, 0, length);
            return Encoding.ASCII.GetString(msgBuffer);
        }
    }
}
