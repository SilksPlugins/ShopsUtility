namespace ShopsUtility.Shops
{
    public class ItemShop
    {
        public ushort ItemId { get; set; }

        public string ItemName { get; set; }

        public decimal? BuyPrice { get; set; }

        public decimal? SellPrice { get; set; }

        public int Order { get; set; }
    }
}
