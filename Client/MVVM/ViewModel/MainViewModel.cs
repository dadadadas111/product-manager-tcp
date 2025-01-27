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
                    _selectedProduct = new ProductModel(value.Id, value.Name, value.Price, value.Stock, value.CategoryId);
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
        public RelayCommand DeleteProductCommand { get; }
        public RelayCommand UpdateProductCommand { get; }
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
            _server.OnProductAdded += ProductsAdded;
            _server.OnProductRemoved += ProductDeleted;
            _server.OnProductUpdated += ProductUpdated;
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
                    if (!IsValidProduct())
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

            DeleteProductCommand = new RelayCommand(
                _ =>
                {
                    var confirm = MessageBox.Show("Are you sure you want to delete this product?", "Delete Product", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm == MessageBoxResult.No)
                        return;
                    if (SelectedProduct == null || SelectedProduct.Id == null)
                        return;
                    _server.SendData((byte)OpCode.DeleteProduct, Username, SelectedProduct.Id);
                },
                _ => !(SelectedProduct == null || SelectedProduct.Id == null) && _server.IsConnected
            );

            UpdateProductCommand = new RelayCommand(
                _ =>
                {
                    if (SelectedCategory == null)
                        return;
                    if (SelectedProduct == null)
                        return;
                    if (!IsValidProduct())
                        return;
                    _server.SendLongData(
                        (byte)OpCode.UpdateProduct,
                        Username,
                        [
                            SelectedProduct.Id,
                            SelectedProduct.Name,
                            SelectedProduct.Price.ToString(),
                            SelectedProduct.Stock.ToString(),
                            SelectedProduct.CategoryId
                        ]);
                },
                _ => !(SelectedProduct == null || SelectedProduct.Id == null) && _server.IsConnected
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

        private void ProductsAdded()
        {
            //throw new System.NotImplementedException();
            var id = _server.PackageReader!.ReadMessage();
            var name = _server.PackageReader!.ReadMessage();
            var price = decimal.Parse(_server.PackageReader!.ReadMessage());
            var stock = int.Parse(_server.PackageReader!.ReadMessage());
            var categoryId = _server.PackageReader!.ReadMessage();
            if (SelectedCategory == null || SelectedCategory.Id != categoryId)
                return;
            var product = new ProductModel(id, name, price, stock, categoryId);
            //SyncSelectedProduct(product);
            Application.Current.Dispatcher.Invoke(() => Products.Add(product));
        }

        private void ProductDeleted()
        {
            //throw new System.NotImplementedException();
            var id = _server.PackageReader!.ReadMessage();
            var product = Products.FirstOrDefault(x => x.Id == id);
            if (product != null)
            {
                //SyncSelectedProduct(new ProductModel());
                Application.Current.Dispatcher.Invoke(() => Products.Remove(product));
            }
        }

        private void ProductUpdated()
        {
            //throw new System.NotImplementedException();
            var id = _server.PackageReader!.ReadMessage();
            var name = _server.PackageReader!.ReadMessage();
            var price = decimal.Parse(_server.PackageReader!.ReadMessage());
            var stock = int.Parse(_server.PackageReader!.ReadMessage());
            var categoryId = _server.PackageReader!.ReadMessage();
            var product = Products.FirstOrDefault(x => x.Id == id);
            if (product != null)
            {
                var updatedProduct = new ProductModel(id, name, price, stock, categoryId);
                //SyncSelectedProduct(updatedProduct);
                Application.Current.Dispatcher.Invoke(() => Products[Products.IndexOf(product)] = updatedProduct);
                SelectedProduct = updatedProduct;
            }
        }

        private bool IsValidProduct()
        {
            if (string.IsNullOrEmpty(SelectedProduct!.Name))
            {
                MessageBox.Show("Product name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (SelectedProduct!.Price <= 0)
            {
                MessageBox.Show("Price must be greater than 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (SelectedProduct!.Stock < 0)
            {
                MessageBox.Show("Stock must be greater than or equal to 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
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
