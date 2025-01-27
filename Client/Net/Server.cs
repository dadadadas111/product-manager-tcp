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

        public event Action? OnUserConnect;
        public event Action? OnUserDisconnect;
        public event Action? OnReceiveMessage;
        public event Action? OnReceiveCategories;
        public event Action? OnReceiveProducts;

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

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (_client.Connected)
                {
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
