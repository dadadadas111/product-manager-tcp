using Client.Net.IO;
using Server.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client
    {
        public string? Name { get; set; }
        public Guid Uid { get; set; }
        public TcpClient ClientSocket { get; set; }

        Logger _logger;
        PackageReader? _packageReader;

        public Client(TcpClient clientSocket)
        {
            _logger = new Logger("Client");

            ClientSocket = clientSocket;
            Uid = Guid.NewGuid();
            _packageReader = new PackageReader(ClientSocket.GetStream());

            var opCode = _packageReader.ReadByte();

            if (opCode == (byte)OpCode.Connect)
            {
                Name = _packageReader.ReadMessage();
                _logger.Log($"Client connected. Uid: {Uid}, Name: {Name}");
            }

            Task.Run(() => Process());
        }

        void Process()
        {
            try
            {
                while (ClientSocket.Connected && _packageReader != null)
                {
                    var opCode = _packageReader.ReadByte();
                    switch (opCode)
                    {
                        case (byte)OpCode.SendMessage:
                            var sender = _packageReader.ReadMessage();
                            var message = _packageReader.ReadMessage();
                            _logger.Log($"Received message from {sender}: {message}");
                            Program.BroadcastMessages(sender, message);
                            break;
                        case (byte)OpCode.Ping:
                            _logger.Log("Received ping.");
                            break;
                        case (byte)OpCode.Disconnect:
                            _logger.Log("Client disconnected.");
                            break;
                        default:
                            _logger.Warning($"Unknown opcode: {opCode}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Warning($"Client disconnected. Uid: {Uid.ToString()}, Name: {Name}");
                Program.BroadcastDisconnect(Uid.ToString());
                ClientSocket.Close();
            }
        }
    }
}
