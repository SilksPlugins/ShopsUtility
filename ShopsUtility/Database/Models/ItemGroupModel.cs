using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopsUtility.Database.Models
{
    [Serializable]
    public class ItemGroupModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Permission { get; set; }

        public bool IsWhitelist { get; set; }

        public ushort ItemShopItemId { get; set; }

        public virtual ItemShopModel ItemShop { get; set; } = null!;

        public ItemGroupModel()
        {
            Id = 0;
            Permission = "";
            IsWhitelist = true;
        }
    }
}