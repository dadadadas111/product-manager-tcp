﻿using Client.Net.IO;
using Server.Data;
using Server.Net.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Server
{
    class Program
    {
        static TcpListener? _listener;
        static CustomLogger? _logger;
        static List<Client>? _clients;

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

            var dbContext = host.Services.GetRequiredService<ApplicationDbContext>();

            dbContext.Database.Migrate();

            _logger.Success("Database migration complete.");

            try
            {
                _clients = new List<Client>();
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 3000);
                _listener.Start();

                _logger.Success("TCP listener started.");
                _logger.Warning("Waiting for connections...");

                while (true)
                {
                    var client = new Client(_listener.AcceptTcpClient());
                    _clients.Add(client);
                    BroadcastConnections();
                    BroadcastMessages("[Server]", $"{client.Name} has connected.");
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
            Console.WriteLine("Broadcasting disconnect");
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
    }
}
