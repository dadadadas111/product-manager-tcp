using Client.MVVM.Core;
using Client.MVVM.Model;
using Client.Net;
using Client.Net.IO;
using System.Collections.ObjectModel;
using System.Windows;

namespace Client.MVVM.ViewModel
{
    internal class MainViewModel
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<CategoryModel> Categories { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; }
        public RelayCommand SendToServerCommand { get; }
        public string? Username { get; set; }
        public string? Message { get; set; }
        private readonly Server _server;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Categories = new ObservableCollection<CategoryModel>();
            Messages = new ObservableCollection<string>();

            Users.Add(new UserModel("Currently Onlines: ", "1234"));
            Messages.Add("Chat:");

            _server = new Server();
            _server.OnUserConnect += UserConnected;
            _server.OnReceiveMessage += MessageReceived;
            _server.OnUserDisconnect += UserDisconnected;
            _server.OnReceiveCategories += CategoriesReceived;
            //string guest = GenerateGuestUsername();

            ConnectToServerCommand = new RelayCommand(
                _ => _server.Connect("127.0.0.1", 3000, Username),
                // username is not empty, and can only contain letters, numbers, and underscores
                _ => !string.IsNullOrEmpty(Username) && System.Text.RegularExpressions.Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$")
                );

            SendToServerCommand = new RelayCommand(
                _ =>
                {
                    if (string.IsNullOrWhiteSpace(Message))
                        return;
                    _server.SendData((byte)OpCode.SendMessage, Username, Message);
                },
                _ => !string.IsNullOrEmpty(Username) && !string.IsNullOrWhiteSpace(Message) && _server.PackageReader != null
            );
        }

        //private static string GenerateGuestUsername()
        //{
        //    return $"Guest{new Random().Next(1000, 9999)}";
        //}

        private void UserConnected()
        {
            var user = new UserModel(
                _server.PackageReader!.ReadMessage(),
                _server.PackageReader!.ReadMessage());

            if (!Users.Any(x => x.Uid == user.Uid))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }

        private void MessageReceived()
        {
            var sender = _server.PackageReader!.ReadMessage();
            var message = _server.PackageReader!.ReadMessage();
            var formattedMessage = $"[{DateTime.Now.ToShortTimeString()}] {sender}: {message}";
            Application.Current.Dispatcher.Invoke(() => Messages.Add(formattedMessage));
        }

        private void CategoriesReceived()
        {
            //throw new System.NotImplementedException();
            var length = int.Parse(_server.PackageReader!.ReadMessage());
            for (int i = 0; i < length; i++)
            {
                var id = _server.PackageReader!.ReadMessage();
                var name = _server.PackageReader!.ReadMessage();
                var category = new CategoryModel(id, name);
                Application.Current.Dispatcher.Invoke(() => Categories.Add(category));
            }
        }

        private void UserDisconnected()
        {
            var user = Users.FirstOrDefault(x => x.Uid == _server.PackageReader!.ReadMessage());
            if (user != null)
            {
                Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
            }
        }
    }
}
