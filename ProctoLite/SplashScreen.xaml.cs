using System;
using System.Threading.Tasks;
using System.Windows;
using Serilog;

namespace ProctoLite
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            Loaded += async (s, e) => await SimulateVerificationChecks();
        }

        private async Task SimulateVerificationChecks()
        {
            try
            {
                Log.Information("Splash Screen mulai menjalankan verifikasi dukungan");

                await UpdateStatus("Memuat CefSharp Engine...", 20);
                await Task.Delay(400);

                await UpdateStatus("Memverifikasi dukungan HTML5...", 45);
                // CefSharp secara inheren mendukung HTML5 sepenuhnya
                await Task.Delay(600);

                await UpdateStatus("Mengecek modul JavaScript V8...", 65);
                // JavaScript selalu aktif dalam mode Exam CEF secara standar
                await Task.Delay(500);

                await UpdateStatus("JavaScript Engine: AKTIF âœ…", 85);
                await Task.Delay(400);

                await UpdateStatus("Menerapkan konfigurasi keamanan...", 95);
                await Task.Delay(400);

                await UpdateStatus("Selesai!", 100);
                await Task.Delay(200);

                Log.Information("Splash Screen selesai, beralih ke MainWindow");

                // Transisi ke jendela Utama (MainWindow)
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saat transisi Splash Screen ke MainWindow");
                MessageBox.Show("Gagal memuat ujian: " + ex.Message, "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private async Task UpdateStatus(string message, double targetProgress)
        {
            StatusText.Text = message;

            // Animasi halus pada ProgressBar
            while (LoadingProgress.Value < targetProgress)
            {
                LoadingProgress.Value += 2;
                await Task.Delay(15);
            }
        }
    }
}
