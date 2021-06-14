using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShopsUtility
{
    /// <summary>
    /// Interaction logic for DatabaseWindow.xaml
    /// </summary>
    public partial class DatabaseWindow : MetroWindow
    {
        public DatabaseWindow()
        {
            InitializeComponent();

            DataContext = this;

            Activated += (sender, args) => UpdateConnectionString();
        }

        public string HostName { get; set; } = "";

        public int Port { get; set; } = 3306;

        public string Database { get; set; } = "";

        public string Username { get; set; } = "";

        public string Password { get; set; } = "";

        public string ConnectionString { get; set; } = "";

        public const string ConnectionStringFormat = "Server={0}; Port={1}; Database={2}; User={3}; Password={4}";

        public bool ValidConnectionString { get; private set; }

        public string GetConnectionString()
        {
            ShowDialog();

            return ValidConnectionString ? ConnectionString : null;
        }

        private void UpdateConnectionString()
        {
            DatabaseConnectionString.Text =
                string.Format(ConnectionStringFormat,
                    HostName.Trim(), Port, Database.Trim(), Username.Trim(), Password.Trim());
        }

        private void OnDatabaseDetailsTextChanged(object sender, TextChangedEventArgs e)
        {
            if (DatabaseConnectionString.Equals(e.Source))
            {
                return;
            }

            UpdateConnectionString();
        }

        private void OnOkButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ValidConnectionString = true;

            Close();
        }

        private void OnCancelButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ValidConnectionString = true;

                Close();
            }
        }
    }
}
