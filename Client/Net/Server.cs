using Client.Net.IO;
using System;
using System.Net.Sockets;

namespace Client.Net
{
    class Server
    {
        private TcpClient _client;
        private NetworkStream? _stream;

        public PackageReader? PackageReader;
        public bool IsConnected => false || _client.Connected;

        // events on User changes
        public event Action? OnUserConnect;
        public event Action? OnUserDisconnect;

        // common events
        public event Action? OnReceiveMessage;
        public event Action? OnReceiveCategories;
        public event Action? OnReceiveProducts;

        // events on Product changes
        public event Action? OnProductAdded;
        public event Action? OnProductRemoved;
        public event Action? OnProductUpdated;

        public Server()
        {
            _client = new TcpClient();
        }

        public void Connect(string ip, int port, string username)
        {
            if (_client.Connected)
                return;
            _client = new TcpClient();
            _client.Connect(ip, port);
            _stream = _client.GetStream();
            PackageReader = new PackageReader(_stream);

            if (!string.IsNullOrEmpty(username))
            {
                var connectPackage = new PackageBuilder();
                connectPackage.WriteOpCode((byte)OpCode.Connect);
                connectPackage.WriteString(username);


                _stream.Write(connectPackage.GetPacketBytes(), 0, connectPackage.GetPacketBytes().Length);
                //_client.Client.Send(connectPackage.GetPacketBytes());

                connectPackage.Dispose();
            }

            ReadPackets();
        }

        public void SendData(byte opCode, string username, string data)
        {
            if (!_client.Connected || _stream == null)
            {
                throw new InvalidOperationException("Client is not connected.");
            }
            var package = new PackageBuilder();
            package.WriteOpCode(opCode);
            package.WriteString(username);
            package.WriteString(data);
            _stream.Write(package.GetPacketBytes(), 0, package.GetPacketBytes().Length);
            package.Dispose();
        }

        public void SendLongData(byte opCode, string username, string[] data)
        {
            if (!_client.Connected || _stream == null)
            {
                throw new InvalidOperationException("Client is not connected.");
            }
            var package = new PackageBuilder();
            package.WriteOpCode(opCode);
            package.WriteString(username);
            foreach (var item in data)
            {
                package.WriteString(item);
            }
            _stream.Write(package.GetPacketBytes(), 0, package.GetPacketBytes().Length);
            package.Dispose();
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (_client.Connected && _stream != null)
                {
                    PackageReader = new PackageReader(_stream);
                    var opCode = PackageReader!.ReadByte();
                    switch ((OpCode)opCode)
                    {
                        case OpCode.UserOnline:
                            OnUserConnect?.Invoke();
                            break;
                        case OpCode.ReceiveMessage:
                            OnReceiveMessage?.Invoke();
                            break;
                        case OpCode.Disconnect:
                            OnUserDisconnect?.Invoke();
                            break;
                        case OpCode.SendCategories:
                            OnReceiveCategories?.Invoke();
                            break;
                        case OpCode.SendProducts:
                            OnReceiveProducts?.Invoke();
                            break;
                        case OpCode.ProductAdded:
                            OnProductAdded?.Invoke();
                            break;
                        case OpCode.ProductDeleted:
                            OnProductRemoved?.Invoke();
                            break;
                        case OpCode.ProductUpdated:
                            OnProductUpdated?.Invoke();
                            break;
                        default:
                            Console.WriteLine($"Unknown OpCode: {opCode}");
                            break;
                    }
                }
                throw new InvalidOperationException("Client disconnected.");
            });
        }

        public void Disconnect()
        {
            if (_client.Connected)
            {
                _stream?.Close();
                _client.Close();
            }
        }
    }
}
