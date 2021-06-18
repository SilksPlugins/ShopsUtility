using MahApps.Metro.Controls;
using ShopsUtility.Assets;
using ShopsUtility.Database.Models;
using ShopsUtility.Shops;
using System;
using System.Windows.Controls;

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

        public override void UpdateShopModel(VehicleShopModel shopModel)
        {
            shopModel.VehicleId = VehicleId;
            shopModel.BuyPrice = VehicleBuyPrice;
            shopModel.Order = VehicleOrder;
        }
    }
}
