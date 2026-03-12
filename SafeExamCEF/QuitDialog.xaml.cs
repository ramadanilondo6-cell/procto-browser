using System.Windows;
using System.Windows.Input;
using Serilog;

namespace Procto
{
    public partial class QuitDialog : Window
    {
        private readonly string _expectedPassword;

        public bool QuitConfirmed { get; private set; } = false;

        public QuitDialog(string password)
        {
            InitializeComponent();
            _expectedPassword = password;
            
            Log.Information("QuitDialog opened - user attempting to exit exam");
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnConfirm_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                BtnCancel_Click(sender, e);
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var enteredPassword = PasswordBox.Password;

            if (enteredPassword == _expectedPassword)
            {
                Log.Information("QuitDialog - Password verified successfully, exit confirmed");
                QuitConfirmed = true;
                DialogResult = true;
                Close();
            }
            else
            {
                Log.Warning("QuitDialog - Incorrect password attempt");
                MessageBox.Show(
                    "Password salah. Percobaan ini telah dicatat.",
                    "Akses Ditolak",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                PasswordBox.Password = string.Empty;
                PasswordBox.Focus();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("QuitDialog - User cancelled exit");
            QuitConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}
