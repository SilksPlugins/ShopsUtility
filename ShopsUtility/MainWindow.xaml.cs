using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using ShopsUtility.Assets;
using ShopsUtility.Database;
using ShopsUtility.Database.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ShopsUtility.Tabs;

namespace ShopsUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            DataContext = this;
        }

        public ItemShopTab ItemShopTab { get; private set; }

        public VehicleShopTab VehicleShopTab { get; private set; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var connectionString = new DatabaseWindow().GetConnectionString();

            MainTabControl.Visibility = Visibility.Hidden;
            MainProgressRing.Visibility = Visibility.Visible;

            AsyncHelper.Run(async () =>
            {
                await VerifyConnectionString(connectionString);

                await Dispatcher.InvokeAsync(() =>
                {
                    MainTabControl.Visibility = Visibility.Visible;
                    MainProgressRing.Visibility = Visibility.Hidden;

                    ItemShopTab = new ItemShopTab(this, connectionString);
                    VehicleShopTab = new VehicleShopTab(this, connectionString);

                    ItemShopTabItem.DataContext = ItemShopTab;
                    VehicleShopTabItem.DataContext = VehicleShopTab;

                    ItemShopTab.Load();
                    VehicleShopTab.Load();
                });
            });
        }

        private async Task VerifyConnectionString(string connectionString)
        {
            async Task ShowUnableToConnectMessageAsync()
            {
                var tcs = new TaskCompletionSource<MessageDialogResult>();

                await Dispatcher.InvokeAsync(async () =>
                {
                    var result = await this.ShowMessageAsync("Unable to connect",
                        "Unable to connect to the database using the given connection string.\n" +
                        "The program will exit now.");

                    tcs.SetResult(result);
                });

                await tcs.Task;
            }

            var ableToConnect = false;

            try
            {
                try
                {
                    var dbContext = new ShopsDbContext(connectionString);

                    if (await dbContext.Database.CanConnectAsync())
                    {
                        ableToConnect = true;
                    }
                }
                catch (InvalidOperationException ex) when (ex.InnerException?.GetType() == typeof(MySqlException))
                {
                    Debug.WriteLine(ex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                if (!ableToConnect)
                {
                    await ShowUnableToConnectMessageAsync();
                    Environment.Exit(-1);
                }
            }
        }
    }
}
