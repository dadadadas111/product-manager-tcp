using Client.MVVM.View;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            //Application.Current.DispatcherUnhandledException += (sender, e) =>
            //{
            //    HandleError(e.Exception);
            //    e.Handled = true;
            //};
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleError(e.Exception);
            e.Handled = true;
        }

        private void HandleError(Exception exception)
        {
            // Log the error if needed
            string errorMessage = $"An unexpected error occurred: {exception.Message}\n\n{exception.StackTrace}";

            // Show error dialog
            ErrorDialog errorDialog = new ErrorDialog(errorMessage);
            errorDialog.ShowDialog();
        }
    }

}
