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
    public class VehicleShopTab : ShopTabBase<VehicleShop, VehicleShopModel>
    {
        public VehicleShopTab(MainWindow window, string connectionString) : base(window, connectionString)
        {
        }

        public override Uri AssetInfoUri => new("https://pastebin.com/raw/A62rLnWC");

        public override TextBox AssetFilterTextBox => Window.VehicleAssetSearchTextBox;

        public override DataGrid AssetDataGrid => Window.VehicleAssetDataGrid;

        public override NumericUpDown ShopIdNumericBox => Window.VehicleIdNumericBox;

        public override Button ShopAddButton => Window.VehicleShopAddButton;

        public override Button DatabaseRefreshButton => Window.VehicleShopDatabaseRefreshButton;

        public override ProgressRing DatabaseProgressRing => Window.VehicleShopDatabaseProgressRing;

        // Binded Properties:
        private ushort _vehicleId;
        public ushort VehicleId
        {
            get => _vehicleId;
            set
            {
                if (_vehicleId != value)
                {
                    _vehicleId = value;
                    OnPropertyChanged(nameof(VehicleId));
                }
            }
        }

        private int _vehicleOrder;
        public int VehicleOrder
        {
            get => _vehicleOrder;
            set
            {
                if (_vehicleOrder != value)
                {
                    _vehicleOrder = value;
                    OnPropertyChanged(nameof(VehicleOrder));
                }
            }
        }

        private decimal _vehicleBuyPrice;
        public decimal VehicleBuyPrice
        {
            get => _vehicleBuyPrice;
            set
            {
                if (_vehicleBuyPrice != value)
                {
                    _vehicleBuyPrice = value;
                    OnPropertyChanged(nameof(VehicleBuyPrice));
                }
            }
        }

        protected override void OnLoad()
        {
            Window.VehicleShopDataGrid.CellStyle = new Style(typeof(DataGridCell), Window.VehicleShopDataGrid.CellStyle)
            {
                Setters =
                {
                    new EventSetter(Control.MouseDoubleClickEvent, new MouseButtonEventHandler(OnShopDoubleClick))
                }
            };
        }

        public override VehicleShop CreateShopFromModel(VehicleShopModel shopModel, AssetInfo asset)
        {
            return new()
            {
                VehicleId = shopModel.VehicleId,
                BuyPrice = shopModel.BuyPrice,
                VehicleName = asset?.Name ?? "",
                Order = shopModel.Order
            };
        }

        public override void ResetInputs()
        {
            VehicleId = 0;
            VehicleBuyPrice = 0;
            VehicleOrder = 0;
        }

        public override void UpdateShopModel(VehicleShopModel shopModel)
        {
            shopModel.VehicleId = VehicleId;
            shopModel.BuyPrice = VehicleBuyPrice;
            shopModel.Order = VehicleOrder;
        }

        private void OnShopDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cell = e.Source as DataGridCell;

            if (cell?.DataContext is not VehicleShop shop)
            {
                return;
            }

            VehicleId = shop.VehicleId;
            VehicleBuyPrice = shop.BuyPrice;
            VehicleOrder = shop.Order;

            var focus = cell.Column.Header switch
            {
                "Buy Price" => Window.VehicleBuyPriceNumericBox,
                "Order" => Window.VehicleOrderNumericBox,
                _ => Window.VehicleBuyPriceNumericBox
            };

            focus.Focus();
            focus.SelectAll();
        }
    }
}
