using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopsUtility.Database.Models
{
    [Serializable]
    public class VehicleShopModel : IShopModel
    {
        [Key]
        public ushort VehicleId { get; set; }
        
        public ushort GetId() => VehicleId;

        [Column(TypeName = "decimal(24,2)")]
        public decimal BuyPrice { get; set; }

        public int Order { get; set; }

        public virtual ICollection<VehicleGroupModel> AuthGroups { get; set; } = new List<VehicleGroupModel>();

        public VehicleShopModel()
        {
            VehicleId = 0;
            BuyPrice = 0;
            Order = 0;
        }
    }
}
