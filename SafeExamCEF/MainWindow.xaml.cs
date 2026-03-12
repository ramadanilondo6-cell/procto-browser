using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;
using Serilog;

namespace Procto
{
    public partial class MainWindow : Window
    {
        private LockdownEngine _lockdown;
        private DispatcherTimer _clockTimer;
        private readonly string _startUrl;
        private readonly string _browserTitle;
        private readonly string _quitPassword;
        private readonly bool _allowClipboard;
        private readonly bool _allowPrint;
        private System.Windows.Forms.Timer _brightnessTimer;
        private double _targetBrightness;
        private double _currentBrightness = 1.0;
        private VolumeController _volumeController;

        public MainWindow()
        {
            try
            {
                Log.Information("MainWindow constructor started");

                // CefSharp is initialized in Program.cs
                Log.Information("Checking CefSharp status: {Status}", Cef.IsInitialized);
                
                if (!Cef.IsInitialized)
                {
                    Log.Error("CefSharp is not initialized");
                    throw new Exception("CefSharp initialization failed in Program.cs");
                }

                // Get config from App
                _startUrl = App.Config?.StartUrl ?? "https://www.google.com";
                _browserTitle = App.Config?.BrowserTitle ?? "Procto";
                _quitPassword = App.Config?.QuitPassword ?? "admin";
                _allowClipboard = App.Config?.AllowClipboard ?? false;
                _allowPrint = App.Config?.AllowPrint ?? false;

                Log.Information("Config loaded - Title: {Title}, StartURL: {Url}", _browserTitle, _startUrl);

                InitializeComponent();

                Log.Information("InitializeComponent completed");

                // Set window title
                Title = _browserTitle;
                TitleText.Text = _browserTitle;

                // Set window icon via code (safer than XAML for packed resources)
                try
                {
                    var iconUri = new Uri("pack://application:,,,/app.ico", UriKind.Absolute);
                    Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
                }
                catch { /* Icon kosmetik saja, tidak fatal */ }

                Log.Information("Window title set to: {Title}", _browserTitle);

                // Initialize brightness timer for smooth transitions
                _brightnessTimer = new System.Windows.Forms.Timer();
                _brightnessTimer.Interval = 50;
                _brightnessTimer.Tick += BrightnessTimer_Tick;

                // Initialize volume controller
                _volumeController = new VolumeController();

                // Initialize lockdown engine with config
                _lockdown = new LockdownEngine(
                    App.Config?.ForbiddenProcesses,
                    _quitPassword,
                    _allowClipboard,
                    _allowPrint,
                    App.ConfigKeyHash
                );
                _lockdown.Enable();

                Log.Information("LockdownEngine enabled");

                // Setup URL filter
                Browser.RequestHandler = new UrlFilterHandler(App.Config?.AllowedUrls, App.ConfigKeyHash);

                // Setup keyboard handler for browser
                Browser.KeyboardHandler = new KeyboardHandler(_allowClipboard, _allowPrint);

                // Disable context menu
                Browser.MenuHandler = new CustomMenuHandler();

                // Handle popups
                Browser.LifeSpanHandler = new CustomLifeSpanHandler();

                Log.Information("Browser handlers configured");

                // Setup process violation handler
                _lockdown.ProcessViolationDetected += (message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Log.Warning("Process violation detected: {Message}", message);
                        var dialog = new AlertDialog("PERINGATAN", message);
                        dialog.Owner = this;
                        dialog.ShowDialog();
                    });
                };

                _lockdown.ProcessThresholdReached += () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Log.Error("Process violation threshold reached - closing exam session");
                        MessageBox.Show(
                            "Terlalu banyak pelanggaran terdeteksi. Ujian akan ditutup.",
                            "Ujian Dihentikan",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        _lockdown.Disable();
                        Close();
                    });
                };

                // Setup clock timer
                SetupClockTimer();

                Log.Information("Clock timer started");

                // Navigate to start URL
                Browser.Address = _startUrl;

                Log.Information("Browser navigating to: {Url}", _startUrl);
                Log.Information("MainWindow setup completed successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal error in MainWindow constructor");
                MessageBox.Show(
                    $"Fatal error: {ex.Message}\n\n{ex.StackTrace}",
                    "Application Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        private void SetupClockTimer()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) =>
            {
                ClockText.Text = DateTime.Now.ToString("HH:mm:ss - dd MMM yyyy");
            };
            _clockTimer.Start();
            Log.Debug("Clock timer started");
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("User clicked quit button");

            var dialog = new QuitDialog(_quitPassword);
            dialog.Owner = this;

            var result = dialog.ShowDialog();

            if (result == true && dialog.QuitConfirmed)
            {
                Log.Information("User confirmed quit - exiting exam session");
                _lockdown.Disable();
                _clockTimer?.Stop();
                Close();
            }
            else
            {
                Log.Information("User cancelled quit");
            }
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Home button clicked - navigating to: {Url}", _startUrl);
            Browser.Address = _startUrl;
        }

        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Reload button clicked");
            if (Browser.IsBrowserInitialized)
            {
                Browser.Reload();
            }
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BrightnessValue != null)
            {
                BrightnessValue.Text = $"{(int)e.NewValue}%";
            }

            // Set target brightness (0.0 to 1.0)
            _targetBrightness = e.NewValue / 100.0;

            // Start smooth transition if timer not already running
            if (_brightnessTimer != null && !_brightnessTimer.Enabled)
            {
                _brightnessTimer.Start();
            }

            Log.Debug("Brightness changed to: {Brightness}%", (int)e.NewValue);
        }

        private void BrightnessTimer_Tick(object sender, EventArgs e)
        {
            // Smooth brightness transition
            double difference = _targetBrightness - _currentBrightness;
            
            if (Math.Abs(difference) < 0.01)
            {
                _currentBrightness = _targetBrightness;
                _brightnessTimer.Stop();
            }
            else
            {
                _currentBrightness += difference * 0.1;
            }

            // Apply brightness using WPF opacity effect on the overlay
            try
            {
                if (DarkOverlay != null)
                {
                    // 1.0 Brightness = 0.0 Overlay Opacity (Normal)
                    // 0.15 Brightness = 0.85 Overlay Opacity (Dark)
                    DarkOverlay.Opacity = 1.0 - _currentBrightness;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Error applying brightness overlay");
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeValue != null)
            {
                VolumeValue.Text = $"{(int)e.NewValue}%";
            }

            // Set system volume
            try
            {
                SetSystemVolume((int)e.NewValue);
                Log.Debug("Volume changed to: {Volume}%", (int)e.NewValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error setting system volume");
            }
        }

        private void SetSystemVolume(int volume)
        {
            try
            {
                _volumeController?.SetVolume(volume);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error setting system volume");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Log.Information("Window closing event triggered");

            // If not already confirmed through quit dialog, show confirmation
            if (_lockdown != null && _lockdown.IsActive)
            {
                var result = MessageBox.Show(
                    "Apakah Anda yakin ingin keluar?",
                    "Konfirmasi",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    Log.Information("User cancelled window close");
                    return;
                }

                _lockdown.Disable();
            }

            _brightnessTimer?.Stop();
            _volumeController?.Dispose();
            _clockTimer?.Stop();
            Log.Information("Window closing - cleaning up resources");
        }
    }

    public class CustomMenuHandler : CefSharp.Handler.ContextMenuHandler
    {
        protected override void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
        }
    }

    public class CustomLifeSpanHandler : CefSharp.Handler.LifeSpanHandler
    {
        protected override bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            Log.Debug("Popup blocked: {Url}", targetUrl);
            return true; // Cancel popup
        }
    }
}
