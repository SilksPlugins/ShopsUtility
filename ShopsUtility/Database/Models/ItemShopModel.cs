using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopsUtility.Database.Models
{
    [Serializable]
    public class ItemShopModel
    {
        [Key]
        public ushort ItemId { get; set; }

        [Column(TypeName = "decimal(24,2)")]
        public decimal? BuyPrice { get; set; }

        [Column(TypeName = "decimal(24,2)")]
        public decimal? SellPrice { get; set; }

        public int Order { get; set; }

        public virtual ICollection<ItemGroupModel> AuthGroups { get; set; } = new List<ItemGroupModel>();

        public ItemShopModel()
        {
            ItemId = 0;
            BuyPrice = null;
            SellPrice = null;
            Order = 0;
        }
    }
}
