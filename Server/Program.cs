using Client.Net.IO;
using Server.Net.IO;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static TcpListener? _listener;
        static Logger? _logger;
        static List<Client>? _clients;

        static void Main(string[] args)
        {
            _logger = new Logger("Server");

            try
            {
                _clients = new List<Client>();
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 3000);
                _listener.Start();

                while (true)
                {
                    var client = new Client(_listener.AcceptTcpClient());
                    _clients.Add(client);
                    BroadcastConnections();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        static void BroadcastConnections()
        {
            if (_clients == null)
                return;

            foreach (var client in _clients)
            {
                foreach (var clientToSend in _clients)
                {
                    var broadcastPackage = new PackageBuilder();
                    broadcastPackage.WriteOpCode((byte)OpCode.UserJoined);
                    if (client.Name == null)
                        continue;
                    broadcastPackage.WriteString(client.Name);
                    broadcastPackage.WriteString(client.Uid.ToString());
                    clientToSend.ClientSocket.Client.Send(broadcastPackage.GetPacketBytes());

                }
            }
        }

        public static void BroadcastMessages(string sender, string message)
        {
            if (_clients == null)
                return;
            foreach (var client in _clients)
            {
                var broadcastPackage = new PackageBuilder();
                broadcastPackage.WriteOpCode((byte)OpCode.ReceiveMessage);
                broadcastPackage.WriteString(sender);
                broadcastPackage.WriteString(message);
                client.ClientSocket.Client.Send(broadcastPackage.GetPacketBytes());
            }
        }

        public static void BroadcastDisconnect(string uid)
        {
            if (_clients == null)
                return;

            var disconnectedClient = _clients.FirstOrDefault(x => x.Uid.ToString() == uid);

            if (disconnectedClient == null)
                return;

            _clients.Remove(disconnectedClient);

            foreach (var client in _clients)
            {
                var broadcastPackage = new PackageBuilder();
                broadcastPackage.WriteOpCode((byte)OpCode.Disconnect);
                broadcastPackage.WriteString(uid);
                client.ClientSocket.Client.Send(broadcastPackage.GetPacketBytes());
            }

            //_logger?.Warning($"Client disconnected. Uid: {uid}, Name: {disconnectedClient.Name}");

            BroadcastConnections();
        }
    }
}
