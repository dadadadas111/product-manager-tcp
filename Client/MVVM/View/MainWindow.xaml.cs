using Client.MVVM.Model;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //private void DeleteButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // message box to confirm deletion
        //    var result = MessageBox.Show("Are you sure you want to delete this product?", "Delete Product", MessageBoxButton.YesNo, MessageBoxImage.Question);
        //    if (result == MessageBoxResult.No)
        //        return;
        //}

        //private void ValidationBeforeActions(object sender, RoutedEventArgs e)
        //{
        //    // check NameTxt
        //    if (string.IsNullOrWhiteSpace(NameTxt.Text))
        //    {
        //        MessageBox.Show("Name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }
        //    // check PriceTxt
        //    if (!decimal.TryParse(PriceTxt.Text, out _))
        //    {
        //        MessageBox.Show("Price must be a number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }
        //    // check StockTxt
        //    if (!int.TryParse(StockTxt.Text, out _))
        //    {
        //        MessageBox.Show("Stock must be a number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }
        //}
    }
}