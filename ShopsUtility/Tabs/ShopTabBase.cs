using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.EntityFrameworkCore;
using ShopsUtility.Assets;
using ShopsUtility.Database;
using ShopsUtility.Database.Models;
using ShopsUtility.Shops;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ShopsUtility.Tabs
{
    public abstract class ShopTabBase<TShop, TShopModel> : INotifyPropertyChanged
        where TShop : IShop
        where TShopModel : class, IShopModel, new()
    {
        public MainWindow Window { get; }

        public List<AssetInfo> Assets { get; }

        public CollectionViewSource AssetsCollectionView { get; }

        public string AssetFilter { get; set; } = "";

        public ObservableCollection<TShop> Shops { get; }

        public CollectionViewSource ShopsCollectionView { get; }

        private bool _isUsingDatabase;

        public ShopsDbContext DbContext { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _databaseControlsEnabled;

        public bool DatabaseControlsEnabled
        {
            get => _databaseControlsEnabled;
            set
            {
                if (_databaseControlsEnabled != value)
                {
                    _databaseControlsEnabled = value;
                    OnPropertyChanged(nameof(DatabaseControlsEnabled));
                }
            }
        }

        public abstract Uri AssetInfoUri { get; }

        public abstract TextBox AssetFilterTextBox { get; }

        public abstract DataGrid AssetDataGrid { get; }

        public abstract NumericUpDown ShopIdNumericBox { get; }

        public abstract Button ShopAddButton { get; }

        public abstract Button DatabaseRefreshButton { get; }

        public abstract ProgressRing DatabaseProgressRing { get; }

        protected ShopTabBase(MainWindow window, string connectionString)
        {
            Window = window;
            
            Assets = new List<AssetInfo>();

            AssetsCollectionView = new CollectionViewSource
            {
                SortDescriptions =
                {
                    new SortDescription("Id", ListSortDirection.Ascending),
                    new SortDescription("Name", ListSortDirection.Descending)
                }
            };

            Shops = new ObservableCollection<TShop>();

            ShopsCollectionView = new CollectionViewSource();

            foreach (var property in typeof(TShop).GetProperties())
            {
                ShopsCollectionView.SortDescriptions.Add(
                    new SortDescription(property.Name, ListSortDirection.Ascending));
            }

            DbContext = new ShopsDbContext(connectionString);

            AssetsCollectionView.Filter += OnAssetFilter;
        }

        public void Load()
        {
            AssetFilterTextBox.TextChanged += OnAssetFilterTextChanged;
            DatabaseRefreshButton.Click += OnDatabaseRefreshClicked;
            ShopAddButton.Click += OnShopAddClicked;

            AssetDataGrid.CellStyle = new Style(typeof(DataGridCell), AssetDataGrid.CellStyle)
            {
                Setters =
                {
                    new EventSetter(Control.MouseDoubleClickEvent, new MouseButtonEventHandler(OnAssetDoubleClick))
                }
            };

            AsyncHelper.Run(async () =>
            {
                await DisableControls();

                await RefreshAssets();

                await RefreshDatabase();

                await EnableControls();
            });

            OnLoad();
        }

        protected virtual void OnLoad() { }

        private void OnDatabaseRefreshClicked(object sender, RoutedEventArgs e)
        {
            AsyncHelper.Run(RefreshDatabase);
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Asset Filter Events

        private void OnAssetFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            AssetFilter = textBox.Text;

            AssetsCollectionView.View.Refresh();
        }

        private void OnAssetFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is not AssetInfo asset)
            {
                return;
            }

            e.Accepted = asset.Name.Contains(AssetFilter, StringComparison.OrdinalIgnoreCase) ||
                         asset.Id.ToString().Contains(AssetFilter);
        }

        #endregion

        #region Assets

        private void OnAssetDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cell = e.Source as DataGridCell;

            if (cell?.DataContext is not AssetInfo itemAsset)
            {
                return;
            }

            ShopIdNumericBox.Value = itemAsset.Id;

            ShopIdNumericBox.Focus();
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

        protected async Task<bool> RefreshAssets()
        {
            using var webClient = new WebClient();

            var address = AssetInfoUri;

            try
            {
                var result = await webClient.DownloadStringTaskAsync(address);

                if (result == null)
                {
                    return false;
                }

                var assets = GetAssets(result);


                return await Window.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        Assets.Clear();

                        foreach (var asset in assets)
                        {
                            Assets.Add(asset);
                        }

                        AssetsCollectionView.Source = Assets.ToList();

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

        #endregion

        #region Database

        public abstract TShop CreateShopFromModel(TShopModel shopModel, AssetInfo asset);

        public abstract void UpdateShopModel(TShopModel shopModel);

        protected async Task DisableControls()
        {
            await Window.Dispatcher.InvokeAsync(() =>
            {
                DatabaseControlsEnabled = false;
            });
        }

        protected async Task EnableControls()
        {
            await Window.Dispatcher.InvokeAsync(() =>
            {
                DatabaseControlsEnabled = true;
            });
        }

        protected async Task<bool> UseDatabase(Func<Task> task, bool manageControls = true)
        {
            if (_isUsingDatabase)
            {
                return false;
            }

            _isUsingDatabase = true;

            try
            {
                if (manageControls)
                {
                    await DisableControls();
                }

                await task();
                
                return true;
            }
            finally
            {
                _isUsingDatabase = false;
                if (manageControls)
                {
                    await EnableControls();
                }
            }
        }

        protected async Task RefreshDatabase()
        {
            if (DbContext == null)
            {
                return;
            }

            await UseDatabase(async () =>
            {
                await Window.Dispatcher.InvokeAsync(() =>
                {
                    DatabaseControlsEnabled = false;

                    Shops.Clear();
                    ShopsCollectionView.Source = Shops;

                    DatabaseProgressRing.IsActive = true;
                    DatabaseProgressRing.Visibility = Visibility.Visible;
                });

                var itemShopModels = await DbContext.Set<TShopModel>().ToListAsync();

                var shops = new List<TShop>();

                foreach (var shopModel in itemShopModels)
                {
                    var asset = Assets.FirstOrDefault(x => x.Id == shopModel.GetId());

                    shops.Add(CreateShopFromModel(shopModel, asset));
                }

                await Window.Dispatcher.InvokeAsync(() =>
                {
                    DatabaseControlsEnabled = true;

                    DatabaseProgressRing.IsActive = false;
                    DatabaseProgressRing.Visibility = Visibility.Hidden;

                    Shops.Clear();
                    shops.ForEach(x => Shops.Add(x));
                    ShopsCollectionView.Source = Shops;
                });
            }, false);
        }

        protected async Task AddShop()
        {
            var success = false;

            if (await UseDatabase(async () =>
            {
                ushort id = 0;

                if (!await Window.Dispatcher.InvokeAsync(() =>
                {
                    id = (ushort?) ShopIdNumericBox.Value ?? 0;

                    if (id == 0)
                    {
                        // No await needed if not waiting for response
                        Window.ShowMessageAsync("Invalid Shop ID", "The shop ID cannot be zero.");
                        return false;
                    }

                    return true;
                }))
                {
                    return;
                }

                var set = DbContext.Set<TShopModel>();

                var shop = await set.FindAsync(id);

                var shouldAdd = shop == null;

                shop ??= new();

                UpdateShopModel(shop);

                if (shouldAdd)
                {
                    await set.AddAsync(shop);
                }
                else
                {
                    set.Update(shop);
                }

                await DbContext.SaveChangesAsync();

                success = true;
            }) && success)
            {
                await RefreshDatabase();
            }
        }

        private void OnShopAddClicked(object sender, RoutedEventArgs e)
        {
            AsyncHelper.Run(AddShop);
        }

        #endregion
    }
}
