namespace ShopsUtility.Shops
{
    public class ItemShop : IShop
    {
        public ushort ItemId { get; set; }

        public string ItemName { get; set; }

        public decimal? BuyPrice { get; set; }

        public decimal? SellPrice { get; set; }

        public int Order { get; set; }

        public ushort GetId() => ItemId;
    }
}
