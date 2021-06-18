using MahApps.Metro.Controls;
using ShopsUtility.Assets;
using ShopsUtility.Database.Models;
using ShopsUtility.Shops;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShopsUtility.Tabs
{
    public class ItemShopTab : ShopTabBase<ItemShop, ItemShopModel>
    {
        public ItemShopTab(MainWindow window, string connectionString) : base(window, connectionString)
        {
        }

        public override Uri AssetInfoUri => new("https://pastebin.com/raw/Ai8gRFT7");

        public override TextBox AssetFilterTextBox => Window.ItemAssetSearchTextBox;

        public override DataGrid AssetDataGrid => Window.ItemAssetDataGrid;

        public override NumericUpDown ShopIdNumericBox => Window.ItemIdNumericBox;

        public override Button ShopAddButton => Window.ItemShopAddButton;

        public override Button DatabaseRefreshButton => Window.ItemShopDatabaseRefreshButton;

        public override ProgressRing DatabaseProgressRing => Window.ItemShopDatabaseProgressRing;

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

        protected override void OnLoad()
        {
            Window.ItemShopDataGrid.CellStyle = new Style(typeof(DataGridCell), Window.ItemShopDataGrid.CellStyle)
            {
                Setters =
                {
                    new EventSetter(Control.MouseDoubleClickEvent, new MouseButtonEventHandler(OnShopDoubleClick))
                }
            };
        }

        public override ItemShop CreateShopFromModel(ItemShopModel shopModel, AssetInfo asset)
        {
            return new()
            {
                ItemId = shopModel.ItemId,
                BuyPrice = shopModel.BuyPrice,
                SellPrice = shopModel.SellPrice,
                ItemName = asset?.Name ?? "",
                Order = shopModel.Order
            };
        }

        public override void UpdateShopModel(ItemShopModel shopModel)
        {
            shopModel.ItemId = ItemId;
            shopModel.BuyPrice = ItemBuyPrice;
            shopModel.SellPrice = ItemSellPrice;
            shopModel.Order = ItemOrder;
        }

        private void OnShopDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cell = e.Source as DataGridCell;

            if (cell?.DataContext is not ItemShop shop)
            {
                return;
            }

            ItemId = shop.ItemId;
            ItemBuyPrice = shop.BuyPrice;
            ItemSellPrice = shop.SellPrice;
            ItemOrder = shop.Order;

            var focus = cell.Column.Header switch
            {
                "Buy Price" => Window.ItemBuyPriceNumericBox,
                "Sell Price" => Window.ItemSellPriceNumericBox,
                "Order" => Window.ItemOrderNumericBox,
                _ => Window.ItemBuyPriceNumericBox
            };

            focus.Focus();
        }
    }
}
