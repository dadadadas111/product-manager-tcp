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
        public ObservableCollection<string> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; }
        public RelayCommand SendToServerCommand { get; }
        public string? Username { get; set; }
        public string? Message { get; set; }
        private readonly Server _server;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();

            Users.Add(new UserModel("Currently Onlines: ", "1234"));
            Messages.Add("Chat:");

            _server = new Server();
            _server.OnUserConnect += UserConnected;
            _server.OnReceiveMessage += MessageReceived;
            _server.OnUserDisconnect += UserDisconnected;
            string guest = GenerateGuestUsername();

            ConnectToServerCommand = new RelayCommand(
                _ => _server.Connect("127.0.0.1", 3000, string.IsNullOrWhiteSpace(Username) ? guest : Username));

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

        private static string GenerateGuestUsername()
        {
            return $"Guest{new Random().Next(1000, 9999)}";
        }

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
            var formattedMessage = $"[{DateTime.Now}] {sender}: {message}";
            Application.Current.Dispatcher.Invoke(() => Messages.Add(formattedMessage));
        }

        private void UserDisconnected()
        {
            var user = Users.FirstOrDefault(x => x.Uid == _server.PackageReader!.ReadMessage());
            //Application.Current.Dispatcher.Invoke(() => Users.Add(new UserModel("test_disconnect", "1234")));
            if (user != null)
            {
                Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
            }
        }
    }
}
