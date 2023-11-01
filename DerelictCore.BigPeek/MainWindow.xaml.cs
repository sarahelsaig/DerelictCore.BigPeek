using DerelictCore.BigPeek.Exceptions;
using DerelictCore.BigPeek.Services;
using System;
using System.Windows;
using System.Windows.Interop;

namespace DerelictCore.BigPeek
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PeekService _peekService;
        
        public MainWindow()
        {
            _peekService = new PeekService();
            InitializeComponent();
        }
        private async void PickWindow_OnClick(object sender, RoutedEventArgs e)
        {
            PickWindow.IsEnabled = false;

            try
            {
                var currentWindow = new WindowInteropHelper(this).Handle;
                var (windowWidth, windowHeight) = _peekService.GetScreenSize(currentWindow);

                var target = await _peekService.PickWindowAsync(currentWindow);
                var targetTitle = _peekService.GetWindowTitle(target);
                StatusBox.Text += $"{Environment.NewLine}Magnifying window [{targetTitle}]...";

                _peekService.MagnifyWindow(target, windowWidth, windowHeight);
            }
            catch (ApiFailureException exception)
            {
                StatusBox.Text += $"{Environment.NewLine}[{exception.ApiName}] @ {exception.Caller} : {exception.Message}";
            }
            catch (Exception exception)
            {
                StatusBox.Text += $"\n[{exception.GetType().Name}] : {exception.Message}\n{exception.StackTrace}\n\n"
                    .Replace("\n", Environment.NewLine);
            }

            PickWindow.IsEnabled = true;
        }

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            try
            {
                _peekService.Dispose();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    exception.ToString(),
                    $"Failed to dispose {nameof(PeekService)}!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        private void AppendStatusText(string text)
        {
            StatusBox.Text += $"\n{text}".Replace("\n", Environment.NewLine);
        }
    }
}