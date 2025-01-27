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

        public static void BroadcastProductChanges(Product product, OpCode opCode)
        {
            if (_clients == null)
                return;
            foreach (var client in _clients)
            {
                var broadcastPackage = new PackageBuilder();
                broadcastPackage.WriteOpCode((byte)opCode);
                broadcastPackage.WriteString(product.Id.ToString());
                if (opCode != OpCode.ProductDeleted)
                {
                    broadcastPackage.WriteString(product.Name);
                    broadcastPackage.WriteString(product.Price.ToString());
                    broadcastPackage.WriteString(product.Stock.ToString());
                    broadcastPackage.WriteString(product.CategoryId.ToString());
                }
                client.ClientSocket.Client.Send(broadcastPackage.GetPacketBytes());
            }
        }

        internal static async Task SendAllCategories(string sender)
        {
            if (_clients == null)
                return;

            var requestClient = _clients.FirstOrDefault(x => x.Name == sender);

            if (requestClient == null)
                return;

            //var categories = _context?.Categories.ToList();
            var categories = await _context?.GetAllCategoriesAsync();

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

        internal static async Task SendProductsByCategoryId(string sender, string categoryId)
        {
            if (_clients == null)
                return;
            var requestClient = _clients.FirstOrDefault(x => x.Name == sender);
            if (requestClient == null)
                return;
            //var products = _context?.Products.Where(x => x.CategoryId.ToString() == categoryId).ToList();
            var products = await _context?.GetProductsByCategoryIdAsync(Guid.Parse(categoryId));
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
                var addedProduct = await _context?.AddProductAsync(product);
                if (addedProduct == null)
                    return;
                //await SendProductsByCategoryId(sender, categoryId);
                BroadcastProductChanges(addedProduct, OpCode.ProductAdded);
                BroadcastMessages("[Server]", $"{sender} added a new product: {name}");
            }
            catch (Exception ex)
            {
                _logger?.Error(ex.Message);
            }
        }

        internal static async Task DeleteProduct(string sender, string productId)
        {
            try
            {
                if (_clients == null)
                    return;
                var requestClient = _clients.FirstOrDefault(x => x.Name == sender);
                if (requestClient == null)
                    return;
                var deletedProduct = await _context?.DeleteProductAsync(Guid.Parse(productId));
                if (deletedProduct == null)
                    return;
                //await SendProductsByCategoryId(sender, deletedProduct.CategoryId.ToString());
                BroadcastProductChanges(deletedProduct, OpCode.ProductDeleted);
                BroadcastMessages("[Server]", $"{sender} deleted a product: {deletedProduct.Name}");
            }
            catch (Exception ex)
            {
                _logger?.Error(ex.Message);
            }
        }

        internal static async Task UpdateProduct(
            string sender,
            string productId,
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
                var product = await _context?.GetProductByIdAsync(Guid.Parse(productId));
                if (product == null)
                    return;
                product.Name = name;
                product.Price = decimal.Parse(price);
                product.Stock = int.Parse(stock);
                product.CategoryId = Guid.Parse(categoryId);
                var updatedProduct = await _context?.UpdateProductAsync(product);
                if (updatedProduct == null)
                    return;
                //await SendProductsByCategoryId(sender, updatedProduct.CategoryId.ToString());
                BroadcastProductChanges(updatedProduct, OpCode.ProductUpdated);
                BroadcastMessages("[Server]", $"{sender} updated a product: {name}");
            }
            catch (Exception ex)
            {
                _logger?.Error(ex.Message);
            }
        }
    }
}
