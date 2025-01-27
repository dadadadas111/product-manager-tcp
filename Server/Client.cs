using Client.Net.IO;
using Server.Data;
using Server.Net.IO;
using System.Net.Sockets;

namespace Server
{
    class Client
    {
        public string? Name { get; set; }
        public Guid Uid { get; set; }
        public TcpClient ClientSocket { get; set; }

        CustomLogger _logger;
        PackageReader? _packageReader;

        public Client(TcpClient clientSocket)
        {
            _logger = new CustomLogger("Client");

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
                            {
                                var sender = _packageReader.ReadMessage();
                                var message = _packageReader.ReadMessage();
                                _logger.Log($"Message from {sender}: {message}");
                                Program.BroadcastMessages(sender, message);
                                break;
                            }
                        case (byte)OpCode.Disconnect:
                            {
                                _logger.Log("Client disconnected.");
                                break;
                            }
                        case (byte)OpCode.GetAllCategories:
                            {
                                var sender = _packageReader.ReadMessage();
                                _logger.Log($"{sender} requested all categories.");
                                Program.SendAllCategories(sender);
                                _logger.Success($"{sender} received all categories.");
                                break;
                            }
                        case (byte)OpCode.GetProductsByCategory:
                            {
                                var sender = _packageReader.ReadMessage();
                                var categoryId = _packageReader.ReadMessage();
                                _logger.Log($"{sender} requested category with id {categoryId}.");
                                Program.SendProductsByCategoryId(sender, categoryId);
                                _logger.Success($"{sender} received category with id {categoryId}.");
                                break;
                            }
                        default:
                            _logger.Warning($"Unknown opcode: {opCode}");
                            break;
                    }
                }
            }
            catch (Exception)
            {
                _logger?.Warning($"Client disconnected. Uid: {Uid.ToString()}, Name: {Name}");
                Program.BroadcastDisconnect(Uid.ToString());
                Program.BroadcastMessages("[Server]", $"{Name} has disconnected.");
                Program.BroadcastConnections();
                ClientSocket.Close();
            }
        }
    }
}
