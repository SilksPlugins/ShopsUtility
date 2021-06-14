using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopsUtility.Database.Models
{
    [Serializable]
    public class VehicleGroupModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Permission { get; set; }

        public bool IsWhitelist { get; set; }

        public ushort VehicleShopVehicleId { get; set; }

        public virtual VehicleShopModel VehicleShop { get; set; } = null!;

        public VehicleGroupModel()
        {
            Id = 0;
            Permission = "";
            IsWhitelist = true;
        }
    }
}
