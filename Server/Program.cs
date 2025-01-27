using Client.Net.IO;
using Server.Data;
using Server.Net.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Models;

namespace Server
{
    class Program
    {
        static TcpListener? _listener;
        static CustomLogger? _logger;
        static List<Client>? _clients;
        static ApplicationDbContext? _context { get; set; }

        static void Main(string[] args)
        {
            _logger = new CustomLogger("Server");

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));
                })
                .Build();

            _context = host.Services.GetRequiredService<ApplicationDbContext>();

            _context.Database.Migrate();

            _logger.Success("Database migration complete.");

            try
            {
                var Ip = "127.0.0.1";
                var Port = 3000;
                _clients = new List<Client>();
                _listener = new TcpListener(IPAddress.Parse(Ip), Port);
                _listener.Start();

                _logger.Success($"TCP listener started on {Ip}:{Port}");
                _logger.Warning("Waiting for connections...");

                while (true)
                {
                    var client = new Client(_listener.AcceptTcpClient());
                    // check name duplication
                    if (_clients.Exists(x => x.Name == client.Name))
                    {
                        _logger.Error($"Client with name {client.Name} already exists. Disconnecting...");
                        client.ClientSocket.Close();
                        continue;
                    }
                    _clients.Add(client);
                    BroadcastConnections();
                    BroadcastMessages("[Server]", $"{client.Name} has connected.");
                    SendAllCategories(client.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            finally
            {
                _listener?.Stop();
                _listener?.Dispose();
                _listener = null;
                _logger?.Warning("TCP listener stopped.");
            }
        }

        // broadcast to all clients

        public static void BroadcastConnections()
        {
            if (_clients == null)
                return;

            foreach (var client in _clients)
            {
                foreach (var clientToSend in _clients)
                {
                    var broadcastPackage = new PackageBuilder();
                    broadcastPackage.WriteOpCode((byte)OpCode.UserOnline);
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
            //BroadcastMessages("[Server]", $"{disconnectedClient.Name} has disconnected.");
            //BroadcastConnections();
        }

        internal static void SendAllCategories(string sender)
        {
            if (_clients == null)
                return;

            var requestClient = _clients.FirstOrDefault(x => x.Name == sender);

            if (requestClient == null)
                return;

            var categories = _context?.Categories.ToList();

            if (categories == null)
                return;

            var package = new PackageBuilder();
            package.WriteOpCode((byte)OpCode.SendCategories);
            package.WriteString(categories.ToArray().Length.ToString());
            foreach (var category in categories)
            {
                package.WriteString(category.Id.ToString());
                package.WriteString(category.Name);
            }

            //requestClient.ClientSocket.Client.Send(package.GetPacketBytes());
            requestClient.ClientSocket.GetStream().Write(package.GetPacketBytes(), 0, package.GetPacketBytes().Length);
        }

        internal static void SendProductsByCategoryId(string sender, string categoryId)
        {
            if (_clients == null)
                return;
            var requestClient = _clients.FirstOrDefault(x => x.Name == sender);
            if (requestClient == null)
                return;
            var products = _context?.Products.Where(x => x.CategoryId.ToString() == categoryId).ToList();
            if (products == null)
                return;
            var package = new PackageBuilder();
            package.WriteOpCode((byte)OpCode.SendProducts);
            package.WriteString(products.ToArray().Length.ToString());
            foreach (var product in products)
            {
                package.WriteString(product.Id.ToString());
                package.WriteString(product.Name);
                package.WriteString(product.Price.ToString());
                package.WriteString(product.Stock.ToString());
                package.WriteString(product.CategoryId.ToString());
            }
            //requestClient.ClientSocket.Client.Send(package.GetPacketBytes());
            requestClient.ClientSocket.GetStream().Write(package.GetPacketBytes(), 0, package.GetPacketBytes().Length);
        }

        internal static async Task AddProduct(
            string sender,
            string name,
            string price,
            string stock,
            string categoryId
            )
        {
            try
            {
                if (_clients == null)
                    return;
                var requestClient = _clients.FirstOrDefault(x => x.Name == sender);
                if (requestClient == null)
                    return;
                var product = new Product(name, decimal.Parse(price), int.Parse(stock), Guid.Parse(categoryId));
                await _context?.AddProductAsync(product);
                _context?.SaveChanges();

                // resend products to the client
                SendProductsByCategoryId(sender, categoryId);
                BroadcastMessages("[Server]", $"{sender} added a new product: {name}");
            }
            catch (Exception ex)
            {
                _logger?.Error(ex.Message);
            }
        }
    }
}
