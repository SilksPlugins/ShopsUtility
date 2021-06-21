using ShopsUtility.Database;

namespace ShopsUtility.Tabs
{
    public abstract class GroupTabBase
    {
        public MainWindow Window { get; }

        public ShopsDbContext DbContext { get; }

        protected GroupTabBase(MainWindow window, string connectionString)
        {
            Window = window;

            DbContext = new ShopsDbContext(connectionString);
        }
    }
}
