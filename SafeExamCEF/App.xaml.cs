using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using Serilog;

namespace Procto
{
    public partial class App : Application
    {
        public static ExamConfig Config { get; private set; }
        public static string ConfigKeyHash { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Setup global exception handler
            DispatcherUnhandledException += (s, args) =>
            {
                Log.Error(args.Exception, "Unhandled dispatcher exception");
                MessageBox.Show($"Error: {args.Exception.Message}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                Log.Error(ex, "Unhandled AppDomain exception");
            };

            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                Log.Error(args.Exception, "Unobserved task exception");
                args.SetObserved();
            };

            // Setup Serilog logging
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logPath);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    path: Path.Combine(logPath, "session_.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("=== Procto Starting ===");
            Log.Information("Base Directory: {BaseDir}", AppDomain.CurrentDomain.BaseDirectory);
            Log.Information("Current Directory: {CurrentDir}", Directory.GetCurrentDirectory());

            // Load configuration
            var configManager = new ConfigManager();
            if (!configManager.Load())
            {
                Log.Warning("Configuration load failed or validation failed, using defaults");
            }

            Config = configManager.Config;
            ConfigKeyHash = configManager.ConfigKeyHash;

            Log.Information("Starting exam session with title: {Title}", Config.BrowserTitle);
            Log.Information("Start URL: {Url}", Config.StartUrl);

            base.OnStartup(e);

            // CefSharp will be initialized in MainWindow constructor
            Log.Information("Application startup completed");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("=== Procto Exiting ===");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
