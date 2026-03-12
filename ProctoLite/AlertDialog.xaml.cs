using System.Windows;
using Serilog;

namespace ProctoLite
{
    public partial class AlertDialog : Window
    {
        public AlertDialog(string title, string message)
        {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;

            Log.Warning("AlertDialog displayed: {Title} - {Message}", title, message);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
