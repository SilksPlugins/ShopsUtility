using Microsoft.EntityFrameworkCore;
using ShopsUtility.Database.Models;

namespace ShopsUtility.Database
{
    public class ShopsDbContext : DbContext
    {
        private readonly string _connectionString;

        public ShopsDbContext() :
            this("Host=127.0.0.1;Port=3306;Database=database;Username=username;Password=password")
        {
        }

        public ShopsDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public virtual string TablePrefix => "ShopsUI_";

        public virtual string MigrationsTableName => $"__{TablePrefix}_MigrationsHistory";

        public DbSet<ItemShopModel> ItemShops => Set<ItemShopModel>();

        public DbSet<VehicleShopModel> VehicleShops => Set<VehicleShopModel>();

        public DbSet<ItemGroupModel> ItemGroups => Set<ItemGroupModel>();

        public DbSet<VehicleGroupModel> VehicleGroups => Set<VehicleGroupModel>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            optionsBuilder.UseMySql(_connectionString,
                options => options.MigrationsHistoryTable(MigrationsTableName));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var name = TablePrefix + entityType.GetTableName();
                entityType.SetTableName(name);
            }

            modelBuilder.Entity<ItemShopModel>()
                .HasKey(x => x.ItemId);

            modelBuilder.Entity<ItemShopModel>()
                .Property(x => x.ItemId)
                .ValueGeneratedNever();

            modelBuilder.Entity<ItemShopModel>()
                .HasMany(x => x.AuthGroups)
                .WithOne(x => x.ItemShop)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VehicleShopModel>()
                .HasKey(x => x.VehicleId);

            modelBuilder.Entity<VehicleShopModel>()
                .Property(x => x.VehicleId)
                .ValueGeneratedNever();

            modelBuilder.Entity<VehicleShopModel>()
                .HasMany(x => x.AuthGroups)
                .WithOne(x => x.VehicleShop)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemGroupModel>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<VehicleGroupModel>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
