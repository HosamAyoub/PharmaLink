using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Models;

namespace PharmaLink_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email)
                .IsUnique();

            //modelBuilder.Entity<Pharmacy>()
            //    .HasIndex(p => p.Name)
            //    .IsUnique();

            //modelBuilder.Entity<Drug>()
            //    .HasIndex(d => d.UNII)
            //    .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.MobileNumber)
                .IsUnique();

            // *********RELATIONSHIPS********* //

            //Account-User (1,1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.User)
                .WithOne(u => u.Account)
                .HasForeignKey<User>(u => u.AccountId);

            //Account-Pharmacy (1,1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Pharmacy)
                .WithOne(p => p.Account)
                .HasForeignKey<Pharmacy>(p => p.AccountId);

            //User-Order (1, many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //Pharmacy-Order (1, many)
            modelBuilder.Entity<Pharmacy>()
                .HasMany(p => p.Orders)
                .WithOne(o => o.Pharmacy)
                .HasForeignKey(o => o.PharmacyId)
                .OnDelete(DeleteBehavior.Restrict);

            //Order-CartItem relationship (1, many)
            //modelBuilder.Entity<Order>()
            //    .HasMany(o => o.CartItems)
            //    .WithOne(ci => ci.Order)
            //    .HasForeignKey(ci => ci.OrderID)
            //    .OnDelete(DeleteBehavior.Cascade);

            //OrderDetail-PharmacyStock relationship (many to one)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.PharmacyStock)
                .WithMany(ps => ps.OrderDetails)
                .HasForeignKey(od => new { od.DrugId, od.PharmacyId })
                .OnDelete(DeleteBehavior.Restrict);

            //OrderDetail-Order relationship (many to one)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            //PharmacyStock (many, many)
            modelBuilder.Entity<PharmacyStock>(entity =>
            {
                entity.HasKey(pd => new { pd.PharmacyId, pd.DrugId });

                entity.HasOne(pd => pd.Pharmacy)
                      .WithMany(p => p.PharmacyStocks)
                      .HasForeignKey(pd => pd.PharmacyId);

                entity.HasOne(pd => pd.Drug)
                      .WithMany(d => d.PharmacyStocks)
                      .HasForeignKey(pd => pd.DrugId);
            });

            //User-PharmacyStocks(Cart) relationship (many to many)
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(uc => new { uc.UserId, uc.DrugId, uc.PharmacyId });

                entity.HasOne(uc => uc.User)
                      .WithMany(u => u.CartItems)
                      .HasForeignKey(uc => uc.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(uc => uc.PharmacyStocks)
                      .WithMany(pd => pd.CartItems)
                      .HasForeignKey(uc => new { uc.DrugId, uc.PharmacyId })
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //UserFavoriteDrug (many, many)
            modelBuilder.Entity<UserFavoriteDrug>(entity =>
            {
                entity.HasKey(uf => new { uf.UserId, uf.DrugId });

                entity.HasOne(uf => uf.User)
                      .WithMany(u => u.UserFavorites)
                      .HasForeignKey(uf => uf.UserId);

                entity.HasOne(uf => uf.Drug)
                      .WithMany(d => d.UserFavorites)
                      .HasForeignKey(uf => uf.DrugId);
            });

            // *********Seed tables********* //
            // Seed Account
            var userAccountId = new Guid("11111111-1111-1111-1111-111111111111");
            var pharmacyAccountId = new Guid("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    AccountID = userAccountId,
                    Role = "User",
                    Email = "user@example.com",
                    Password = "UserPass123"
                },
                new Account
                {
                    AccountID = pharmacyAccountId,
                    Role = "Pharmacy",
                    Email = "pharmacy@example.com",
                    Password = "PharmacyPass123"
                }
            );

            // Seed User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Name = "John Doe",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1995, 1, 1),
                    MobileNumber = "0123456789",
                    Country = "Egypt",
                    Address = "123 Main St",
                    UserDisease = "Diabetes",
                    UserDrugs = "Metformin",
                    AccountId = userAccountId
                }
            );

            // Seed Pharmacy
            modelBuilder.Entity<Pharmacy>().HasData(
                new Pharmacy
                {
                    PharmacyID = 1,
                    Name = "GoodHealth Pharmacy",
                    Country = "Egypt",
                    Address = "456 Health Ave",
                    Rate = 4.5,
                    StartHour = new TimeOnly(8, 0),
                    EndHour = new TimeOnly(22, 0),
                    AccountId = pharmacyAccountId
                }
            );

            // Seed Drug
            modelBuilder.Entity<Drug>().HasData(
                new Drug
                {
                    DrugID = 1,
                    CommonName = "Paracetamol",
                    Category = "Pain Reliever",
                    ActiveIngredient = "Acetaminophen",
                    Alternatives_names = "Panadol, Tylenol",
                    AlternativesGpID = 101,
                    Indications_and_usage = "For fever and pain",
                    Dosage_and_administration = "500mg every 6 hours",
                    Dosage_forms_and_strengths = "Tablet 500mg",
                    Contraindications = "Liver disease",
                    Warnings_and_cautions = "Do not exceed 4g/day",
                    Drug_interactions = "Alcohol",
                    Description = "Pain relief drug",
                    Storage_and_handling = "Store below 30°C",
                    Adverse_reactions = "Nausea, Rash",
                    Drug_UrlImg = "https://example.com/images/paracetamol.png"
                }
            );

            // Seed PharmacyStock (Pharmacy ↔ Drug)
            modelBuilder.Entity<PharmacyStock>().HasData(
                new PharmacyStock
                {
                    DrugId = 1,
                    PharmacyId = 1,
                    Price = 10.5m,
                    QuantityAvailable = 100
                }
            );
        }

        // *********DB SETS********* //
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<PharmacyStock> PharmacyStocks { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<UserFavoriteDrug> UserFavoriteDrugs { get; set; }
        
    }
}
