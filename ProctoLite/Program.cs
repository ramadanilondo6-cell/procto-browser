using System;
using System.Linq;
using CefSharp;

namespace ProctoLite
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            // Apabila ini adalah proses CEF (Renderer, GPU, dll), jalankan dan keluar.
            if (args.Any(arg => arg.StartsWith("--type=")))
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cef_subprocess.log"), "Subprocess Started: " + string.Join(" ", args) + "\n");
                int result = Cef.ExecuteProcess();
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cef_subprocess.log"), "Cef.ExecuteProcess returned: " + result + "\n");
                return result;
            }

            // Initialize CefSharp before WPF application starts to prevent deadlocks
            var settings = new CefSharp.Wpf.CefSettings();
            settings.RemoteDebuggingPort = 0;
            settings.LogSeverity = LogSeverity.Warning;
            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            settings.CefCommandLineArgs.Add("disable-software-rasterizer", "1");
            settings.CefCommandLineArgs.Add("disable-extensions", "1");
            settings.CachePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
            settings.BrowserSubprocessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            settings.ResourcesDirPath = AppContext.BaseDirectory;
            settings.LocalesDirPath = System.IO.Path.Combine(AppContext.BaseDirectory, "locales");

            Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);

            // Jika bukan, ini adalah instance utama aplikasi, jalankan aplikasi WPF.
            var app = new App();
            app.InitializeComponent();
            app.Run();

            return 0;
        }
    }
}
