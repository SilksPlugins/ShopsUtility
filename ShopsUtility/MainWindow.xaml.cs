using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using ShopsUtility.Assets;
using ShopsUtility.Database;
using ShopsUtility.Database.Models;
using ShopsUtility.Shops;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ShopsUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        #region Items Properties

        public const string ItemAssetsInfoUri = "https://pastebin.com/raw/Ai8gRFT7";

        public List<AssetInfo> ItemAssets { get; }

        public string ItemAssetFilter { get; set; } = "";

        public CollectionViewSource ItemAssetsCollectionView { get; }

        public List<ItemShop> ItemShops { get; }

        public CollectionViewSource ItemShopsCollectionView { get; }

        // Binded Properties:
        private ushort _itemId;
        public ushort ItemId
        {
            get => _itemId;
            set
            {
                if (_itemId != value)
                {
                    _itemId = value;
                    OnPropertyChanged(nameof(ItemId));
                }
            }
        }

        private int _itemOrder;
        public int ItemOrder
        {
            get => _itemOrder;
            set
            {
                if (_itemOrder != value)
                {
                    _itemOrder = value;
                    OnPropertyChanged(nameof(ItemOrder));
                }
            }
        }

        private decimal? _itemBuyPrice;
        public decimal? ItemBuyPrice
        {
            get => _itemBuyPrice;
            set
            {
                if (_itemBuyPrice != value)
                {
                    _itemBuyPrice = value;
                    OnPropertyChanged(nameof(ItemBuyPrice));
                }
            }
        }

        private decimal? _itemSellPrice;
        public decimal? ItemSellPrice
        {
            get => _itemSellPrice;
            set
            {
                if (_itemSellPrice != value)
                {
                    _itemSellPrice = value;
                    OnPropertyChanged(nameof(ItemSellPrice));
                }
            }
        }

        #endregion

        #region Vehicle Properties

        public const string VehicleAssetsInfoUri = "";

        public List<AssetInfo> VehicleAssets { get; }

        public string VehicleAssetFilter { get; set; } = "";

        public CollectionViewSource VehicleAssetsCollectionView { get; }

        public List<VehicleShop> VehicleShops { get; }

        public CollectionViewSource VehicleShopsCollectionView { get; }

        #endregion

        public ShopsDbContext DbContext { get; private set; }

        private bool _isUsingDatabase;

        public MainWindow()
        {
            InitializeComponent();

            ItemAssets = new List<AssetInfo>();
            VehicleAssets = new List<AssetInfo>();
            ItemShops = new List<ItemShop>();
            VehicleShops = new List<VehicleShop>();

            ItemAssetsCollectionView = new CollectionViewSource
            {
                Source = ItemAssets,
                SortDescriptions =
                {
                    new SortDescription("Id", ListSortDirection.Ascending),
                    new SortDescription("Name", ListSortDirection.Descending)
                }
            };

            ItemAssetsCollectionView.Filter += OnItemAssetsFilter;

            VehicleAssetsCollectionView = new CollectionViewSource
            {
                Source = VehicleAssets,
                SortDescriptions =
                {
                    new SortDescription("Id", ListSortDirection.Ascending),
                    new SortDescription("Name", ListSortDirection.Descending)
                }
            };

            VehicleAssetsCollectionView.Filter += OnVehicleAssetsFilter;

            ItemShopsCollectionView = new CollectionViewSource
            {
                Source = ItemShops,
                SortDescriptions =
                {
                    new SortDescription("ItemId", ListSortDirection.Ascending),
                    new SortDescription("ItemName", ListSortDirection.Descending),
                    new SortDescription("BuyPrice", ListSortDirection.Ascending),
                    new SortDescription("SellPrice", ListSortDirection.Ascending),
                    new SortDescription("Order", ListSortDirection.Ascending)
                }
            };

            VehicleShopsCollectionView = new CollectionViewSource
            {
                Source = ItemShops,
                SortDescriptions =
                {
                    new SortDescription("VehicleId", ListSortDirection.Ascending),
                    new SortDescription("VehicleName", ListSortDirection.Descending),
                    new SortDescription("BuyPrice", ListSortDirection.Ascending),
                    new SortDescription("Order", ListSortDirection.Ascending)
                }
            };

            Loaded += OnLoaded;

            DataContext = this;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var connectionString = new DatabaseWindow().GetConnectionString();

            AsyncHelper.Run(async () =>
            {
                await RefreshItemAssets();

                //await RefreshVehicleAssets();

                await SetupDatabase(connectionString);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static IEnumerable<AssetInfo> GetAssets(string unparsed)
        {
            var assets = new List<AssetInfo>();

            foreach (var line in unparsed.Split("\n").Select(x => x.Trim()).ToArray())
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var index = line.IndexOf(',');

                if (index <= 0 || index == line.Length - 1) continue;

                var strAssetId = line.Substring(0, index).Trim();
                var assetName = line.Substring(index + 1).Trim();

                if (string.IsNullOrWhiteSpace(assetName)) continue;

                if (!ushort.TryParse(strAssetId, out var assetId)) continue;

                assets.Add(new AssetInfo
                {
                    Id = assetId,
                    Name = assetName
                });
            }

            return assets;
        }

        private async Task<bool> UseDatabase(Func<Task> task)
        {
            if (_isUsingDatabase) return false;

            _isUsingDatabase = true;

            try
            {
                await task();

                _isUsingDatabase = false;
                return true;
            }
            catch
            {
                _isUsingDatabase = false;
                throw;
            }
        }

        private async Task<bool> RefreshAssets(string uri, ICollection<AssetInfo> assetCollection, Action completedCallback)
        {
            using var webClient = new WebClient();

            var address = new Uri(uri);

            try
            {
                var result = await webClient.DownloadStringTaskAsync(address);

                if (result == null)
                {
                    return false;
                }

                var assets = GetAssets(result);

                
                return await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        assetCollection.Clear();

                        foreach (var asset in assets)
                        {
                            assetCollection.Add(asset);
                        }

                        completedCallback();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error occurred when updating UI with assets:");
                        Debug.WriteLine(ex);

                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error when refreshing assets:");
                Debug.WriteLine(ex);

                return false;
            }
        }

        private Task<bool> RefreshItemAssets()
        {
            return RefreshAssets(ItemAssetsInfoUri, ItemAssets,
                () => ItemAssetsCollectionView.Source = ItemAssets.ToList());
        }

        private Task<bool> RefreshVehicleAssets()
        {
            return RefreshAssets(VehicleAssetsInfoUri, VehicleAssets,
                () => VehicleAssetsCollectionView.Source = VehicleAssets.ToList());
        }

        private async Task RefreshDatabase()
        {
            if (DbContext == null)
            {
                return;
            }

            await UseDatabase(async () =>
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ItemShops.Clear();
                    ItemShopsCollectionView.Source = ItemShops.ToList();

                    VehicleShops.Clear();
                    VehicleShopsCollectionView.Source = VehicleShops.ToList();

                    ItemShopsProgressRing.IsActive = true;
                    ItemShopsProgressRing.Visibility = Visibility.Visible;
                });

                var itemShopModels = await DbContext.ItemShops.ToListAsync();

                var vehicleShopModels = await DbContext.VehicleShops.ToListAsync();

                var itemShops = new List<ItemShop>();

                foreach (var itemShopModel in itemShopModels)
                {
                    var asset = ItemAssets.FirstOrDefault(x => x.Id == itemShopModel.ItemId);

                    itemShops.Add(new ItemShop
                    {
                        ItemId = itemShopModel.ItemId,
                        ItemName = asset?.Name ?? "",
                        BuyPrice = itemShopModel.BuyPrice,
                        SellPrice = itemShopModel.SellPrice,
                        Order = itemShopModel.Order
                    });
                }

                var vehicleShops = new List<VehicleShop>();

                foreach (var vehicleShopModel in vehicleShopModels)
                {
                    var asset = VehicleAssets.FirstOrDefault(x => x.Id == vehicleShopModel.VehicleId);

                    vehicleShops.Add(new VehicleShop
                    {
                        VehicleId = vehicleShopModel.VehicleId,
                        VehicleName = asset?.Name ?? "",
                        BuyPrice = vehicleShopModel.BuyPrice,
                        Order = vehicleShopModel.Order
                    });
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    ItemShopsProgressRing.IsActive = false;
                    ItemShopsProgressRing.Visibility = Visibility.Hidden;

                    ItemShops.Clear();
                    ItemShops.AddRange(itemShops);
                    ItemShopsCollectionView.Source = ItemShops.ToList();

                    VehicleShops.Clear();
                    VehicleShops.AddRange(vehicleShops);
                    VehicleShopsCollectionView.Source = VehicleShops.ToList();
                });
            });
        }

        private async Task SetupDatabase(string connectionString)
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
                if (await UseDatabase(async () =>
                {
                    try
                    {
                        DbContext = new ShopsDbContext(connectionString);

                        if (await DbContext.Database.CanConnectAsync())
                        {
                            ableToConnect = true;
                        }
                    }
                    catch (InvalidOperationException ex) when (ex.InnerException?.GetType() == typeof(MySqlException))
                    {
                        Debug.WriteLine(ex);
                    }
                }) && ableToConnect)
                {
                    await RefreshDatabase();
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

        private async Task AddItemShop(ushort itemId, decimal? buyPrice, decimal? sellPrice, int order)
        {
            if (await UseDatabase(async () =>
            {
                var itemShop = await DbContext.ItemShops.FindAsync(itemId);

                if (itemShop == null)
                {
                    itemShop = new ItemShopModel
                    {
                        ItemId = itemId,
                        BuyPrice = buyPrice,
                        SellPrice = sellPrice,
                        Order = order
                    };
                }
                else
                {
                    itemShop.BuyPrice = buyPrice;
                    itemShop.SellPrice = sellPrice;
                    itemShop.Order = order;
                }

                DbContext.ItemShops.Update(itemShop);

                await DbContext.SaveChangesAsync();
            }))
            {
                await RefreshDatabase();
            }
        }

        #region Filter Logic

        private void OnItemAssetFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            ItemAssetsCollectionView.View.Refresh();
        }

        private void OnItemAssetsFilter(object sender, FilterEventArgs e)
        {
            if (!(e.Item is AssetInfo itemAsset)) return;

            e.Accepted = itemAsset.Name.Contains(ItemAssetFilter, StringComparison.OrdinalIgnoreCase) ||
                         itemAsset.Id.ToString().Contains(ItemAssetFilter);
        }

        private void OnVehicleAssetFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            VehicleAssetsCollectionView.View.Refresh();
        }

        private void OnVehicleAssetsFilter(object sender, FilterEventArgs e)
        {
            if (!(e.Item is AssetInfo vehicleAsset)) return;

            e.Accepted = vehicleAsset.Name.Contains(VehicleAssetFilter, StringComparison.OrdinalIgnoreCase) ||
                         vehicleAsset.Id.ToString().Contains(VehicleAssetFilter);
        }

        #endregion

        private void OnItemAssetDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cell = e.Source as DataGridCell;

            if (cell?.DataContext is not AssetInfo itemAsset)
            {
                return;
            }

            ItemId = itemAsset.Id;

            ItemIdNumericBox.Focus();
        }

        private void OnItemShopsRefreshClick(object sender, RoutedEventArgs e)
        {
            AsyncHelper.Run(RefreshDatabase);
        }

        private void OnItemShopAddClick(object sender, RoutedEventArgs e)
        {
            AsyncHelper.Run(() => AddItemShop(ItemId, ItemBuyPrice, ItemSellPrice, ItemOrder));
        }
    }
}
