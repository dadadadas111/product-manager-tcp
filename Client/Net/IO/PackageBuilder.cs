using System;
using System.IO;
using System.Text;

namespace Client.Net.IO
{
    internal class PackageBuilder : IDisposable
    {
        private readonly MemoryStream _ms;

        public PackageBuilder()
        {
            _ms = new MemoryStream();
        }

        public void WriteOpCode(byte opCode)
        {
            _ms.WriteByte(opCode);
        }

        public void WriteString(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentException("String cannot be null or empty.", nameof(str));

            var msgLength = BitConverter.GetBytes(str.Length);
            _ms.Write(msgLength, 0, msgLength.Length);

            var msg = Encoding.ASCII.GetBytes(str);
            _ms.Write(msg, 0, msg.Length);
        }

        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }

        public void Dispose()
        {
            _ms.Dispose();
        }
    }
}
