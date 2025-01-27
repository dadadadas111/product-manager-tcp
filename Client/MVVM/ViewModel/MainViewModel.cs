using Client.MVVM.Core;
using Client.MVVM.Model;
using Client.Net;
using Client.Net.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Client.MVVM.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<CategoryModel> Categories { get; set; }
        public ObservableCollection<ProductModel> Products { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        public CategoryModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value && value != null)
                {
                    _selectedCategory = value;
                    OnCategoryChanged();
                }
            }
        }
        public ProductModel? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (_selectedProduct != value && value != null)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(nameof(SelectedProduct));
                }
            }
        }
        public string? Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        public RelayCommand ConnectToServerCommand { get; }
        public RelayCommand SendMessageToServerCommand { get; }
        public RelayCommand AddProductCommand { get; }
        public string? Username { get; set; }
        private readonly Server _server;
        private CategoryModel? _selectedCategory;
        private ProductModel? _selectedProduct;
        private string? _message;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Categories = new ObservableCollection<CategoryModel>();
            Products = new ObservableCollection<ProductModel>();
            Messages = new ObservableCollection<string>();

            Users.Add(new UserModel("Users Online: ", ""));
            Messages.Add("Chat:");
            SelectedProduct = new ProductModel("", "", 0, 0, "");

            _server = new Server();
            _server.OnUserConnect += UserConnected;
            _server.OnReceiveMessage += MessageReceived;
            _server.OnUserDisconnect += UserDisconnected;
            _server.OnReceiveCategories += CategoriesReceived;
            _server.OnReceiveProducts += ProductsReceived;
            //string guest = GenerateGuestUsername();

            ConnectToServerCommand = new RelayCommand(
                _ => _server.Connect("127.0.0.1", 3000, Username),
                // username is not empty, and can only contain letters, numbers, and underscores
                _ => !string.IsNullOrEmpty(Username) &&
                Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$") &&
                !_server.IsConnected
                );

            SendMessageToServerCommand = new RelayCommand(
                _ =>
                {
                    if (string.IsNullOrWhiteSpace(Message))
                        return;
                    _server.SendData((byte)OpCode.SendMessage, Username, Message);
                    Message = string.Empty;
                },
                _ => !string.IsNullOrEmpty(Username) && !string.IsNullOrWhiteSpace(Message) && _server.IsConnected
            );

            AddProductCommand = new RelayCommand(
                _ =>
                {
                    if (SelectedCategory == null)
                        return;
                    if (SelectedProduct == null)
                        return;
                    _server.SendLongData(
                        (byte)OpCode.AddProduct,
                        Username,
                        [
                            SelectedProduct.Name,
                            SelectedProduct.Price.ToString(),
                            SelectedProduct.Stock.ToString(),
                            SelectedCategory.Id
                        ]);
                },
                _ => SelectedCategory != null && SelectedProduct != null && _server.IsConnected
            );
        }

        // events from the client

        private void OnCategoryChanged()
        {
            if (_selectedCategory == null)
                return;
            Products.Clear();
            _server.SendData((byte)OpCode.GetProductsByCategory, Username, _selectedCategory.Id);
        }


        // events from the server

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

        private void ProductsReceived()
        {
            //throw new System.NotImplementedException();
            Application.Current.Dispatcher.Invoke(() => Products.Clear());
            var length = int.Parse(_server.PackageReader!.ReadMessage());
            for (int i = 0; i < length; i++)
            {
                var id = _server.PackageReader!.ReadMessage();
                var name = _server.PackageReader!.ReadMessage();
                var price = decimal.Parse(_server.PackageReader!.ReadMessage());
                var stock = int.Parse(_server.PackageReader!.ReadMessage());
                var categoryId = _server.PackageReader!.ReadMessage();
                var product = new ProductModel(id, name, price, stock, categoryId);
                Application.Current.Dispatcher.Invoke(() => Products.Add(product));
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
