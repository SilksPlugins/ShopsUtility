using System.Windows;
using Microsoft.Xaml.Behaviors.Core;
using ShopsUtility.Database;
using System.Windows.Input;

namespace ShopsUtility.Tabs
{
    public class AuthGroupTab
    {
        public MainWindow Window { get; }

        public ShopsDbContext DbContext { get; }

        public ICommand GroupRemovedCommand { get; }

        protected AuthGroupTab(MainWindow window, string connectionString)
        {
            Window = window;

            DbContext = new ShopsDbContext(connectionString);

            GroupRemovedCommand = new ActionCommand(x => GroupRemoved((string)x));
        }

        private void GroupRemoved(string group)
        {
            
        }
    }
}
