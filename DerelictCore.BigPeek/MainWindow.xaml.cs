using DerelictCore.BigPeek.Exceptions;
using DerelictCore.BigPeek.Services;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using static Vanara.PInvoke.User32.HotKeyModifiers;

namespace DerelictCore.BigPeek
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PeekService _peekService;
        private HotkeyService? _zoomHotkeyService;

        public MainWindow()
        {
            _peekService = new PeekService();
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                var source = PresentationSource.FromVisual(this) as HwndSource ??
                    throw new InvalidOperationException("Could not create hWnd source from window.");
                var handle = new WindowInteropHelper(this).Handle;
                var keyX = (uint)KeyInterop.VirtualKeyFromKey(Key.X);
                _zoomHotkeyService = HotkeyService.Create(9000, handle, source, MOD_CONTROL | MOD_WIN, keyX, _ =>
                    {
                        AppendStatusText("Zooming out...");
                        _peekService.ZoomOut();
                    });
            }
            catch (Exception exception)
            {
                LogError(exception);
            }
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
                AppendStatusText($"Magnifying window 『{targetTitle}』...");

                var info = _peekService.MagnifyWindow(target, windowWidth, windowHeight);
                AppendStatusText($"Magnified window to scale {info.MagnificationFactor}x around {info.RectangleToString()}.");
            }
            catch (Exception exception)
            {
                LogError(exception);
            }

            PickWindow.IsEnabled = true;
        }

        private void LogError(Exception exception)
        {
            if (exception is ApiFailureException apiFailure)
            {
                AppendStatusText($"[{apiFailure.ApiName}] @ {apiFailure.Caller} : {apiFailure.Message}");
            }
            else
            {
                AppendStatusText($"[{exception.GetType().Name}] : {exception.Message}\n{exception.StackTrace}\n\n");
            }
        }

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            try
            {
                _zoomHotkeyService?.Dispose();
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