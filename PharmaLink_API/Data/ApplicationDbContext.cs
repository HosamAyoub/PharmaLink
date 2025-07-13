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
            //modelBuilder.Entity<Account>().HasData(
            //    new Account { AccountID = 1, Email = "user1@example.com", Password = "hashedpass", Role = "User" },
            //    new Account { AccountID = 2, Email = "pharmacy@example.com", Password = "hashedpass", Role = "Pharmacy" }
            //);

            //modelBuilder.Entity<User>().HasData(
            //    new User
            //    {
            //        UserID = 1,
            //        Name = "Mariem",
            //        Gender = "Female",
            //        DateOfBirth = new DateTime(2000, 1, 1),
            //        MobileNumber = "0100000000",
            //        Country = "Egypt",
            //        Address = "Cairo",
            //        UserDisease = "None",
            //        UserDrugs = "Paracetamol",
            //        AccountId = 1
            //    }
            // );

            //modelBuilder.Entity<Drug>().HasData(
            //    new Drug
            //    {
            //        DrugID = 1,
            //        CommonName = "Panadol",
            //        Category = "Painkiller",
            //        ActiveIngredient = "Paracetamol",
            //        Alternatives_names = "Tylenol",
            //        AlternativesGpID = 100,
            //        Indications_and_usage = "Headache, fever",
            //        Dosage_and_administration = "500mg twice daily",
            //        Dosage_forms_and_strengths = "Tablet 500mg",
            //        Contraindications = "Liver disease",
            //        Warnings_and_cautions = "Don't exceed 4g/day",
            //        Drug_interactions = "Warfarin",
            //        Description = "Pain reliever",
            //        Storage_and_handling = "Keep cool and dry",
            //        Adverse_reactions = "Nausea",
            //        Drug_UrlImg = "/images/panadol.png"
            //    }
            // );

            //modelBuilder.Entity<Pharmacy>().HasData(
            //    new Pharmacy { PharmacyID = 1, Name = "Good Health", Country="Egypt", Address = "Nasr City", AccountId = 2 }
            //);

        //    modelBuilder.Entity<PharmacyStock>().HasData(
        //        new PharmacyStock { DrugId = 1, PharmacyId = 1, Price = 15.00m, QuantityAvailable = 50 }
        //    );

        //    modelBuilder.Entity<UserFavoriteDrug>().HasData(
        //        new UserFavoriteDrug { UserId = 1, DrugId = 1 }
        //    );

        //    modelBuilder.Entity<CartItem>().HasData(
        //        new CartItem
        //        {
        //            UserId = 1,
        //            DrugId = 1,
        //            PharmacyId = 1,
        //            Quantity = 1
        //        }
        //    );
        }

        // *********DB SETS********* //
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<PharmacyStock> PharmacyStocks { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<UserFavoriteDrug> UserFavoriteDrugs { get; set; }
        
    }
}
