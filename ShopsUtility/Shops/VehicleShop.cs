namespace ShopsUtility.Shops
{
    public class VehicleShop : IShop
    {
        public ushort VehicleId { get; set; }

        public string VehicleName { get; set; }

        public decimal BuyPrice { get; set; }

        public int Order { get; set; }

        public ushort GetId() => VehicleId;
    }
}
