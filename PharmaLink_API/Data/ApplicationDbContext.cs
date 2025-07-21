using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Models;

namespace PharmaLink_API.Data
{
    public class ApplicationDbContext : IdentityDbContext<Account>
    {
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Account>()
            //    .HasIndex(a => a.Email)
            //    .IsUnique();

            //modelBuilder.Entity<Pharmacy>()
            //    .HasIndex(p => p.Name)
            //    .IsUnique();

            modelBuilder.Entity<Drug>()
                .HasIndex(d => d.Category)
                .IncludeProperties(d => new { d.CommonName , d.ActiveIngredient });



            //modelBuilder.Entity<Patient>()
            //    .HasIndex(u => u.MobileNumber)
            //    .IsUnique();
            modelBuilder.Entity<Patient>()
                .Property(u => u.Gender)
                .HasConversion<string>();


            // *********RELATIONSHIPS********* //

            //Account-Patient (1,1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Patient)
                .WithOne(u => u.Account)
                .HasForeignKey<Patient>(u => u.AccountId);

            //Account-Pharmacy (1,1)
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Pharmacy)
                .WithOne(p => p.Account)
                .HasForeignKey<Pharmacy>(p => p.AccountId);

            //Account Phone number is unique
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.PhoneNumber)
                .IsUnique();

            //Patient-Order (1, many)
            modelBuilder.Entity<Patient>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.Patient)
                .HasForeignKey(o => o.PatientId)
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

            //Patient-PharmacyStocks(Cart) relationship (many to many)
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(uc => new { uc.PatientId, uc.DrugId, uc.PharmacyId });

                entity.HasOne(uc => uc.Patient)
                      .WithMany(u => u.CartItems)
                      .HasForeignKey(uc => uc.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(uc => uc.PharmacyStocks)
                      .WithMany(pd => pd.CartItems)
                      .HasForeignKey(uc => new { uc.PharmacyId, uc.DrugId })
                      .OnDelete(DeleteBehavior.Restrict);
            });

            //PatientFavoriteDrug (many, many)
            modelBuilder.Entity<PatientFavoriteDrug>(entity =>
            {
                entity.HasKey(uf => new { uf.PatientId, uf.DrugId });

                entity.HasOne(uf => uf.Patient)
                      .WithMany(u => u.PatientFavorites)
                      .HasForeignKey(uf => uf.PatientId);

                entity.HasOne(uf => uf.Drug)
                      .WithMany(d => d.PatientFavorites)
                      .HasForeignKey(uf => uf.DrugId);
            });

            base.OnModelCreating(modelBuilder);



            //// *********Seed tables********* //
            //var guid1 = Guid.NewGuid().ToString();
            //var guid2 = Guid.NewGuid().ToString();
            //modelBuilder.Entity<Account>().HasData(
            //new Account { Id = guid1, UserName="Patient", DisplayName="Patient", PhoneNumber="213123", Email = "Patient1@example.com", PasswordHash = "Ads6*6" },
            //new Account { Id = guid2, UserName = "Pharmacy", DisplayName = "Pharmacy", PhoneNumber = "213123", Email = "pharmacy@example.com", PasswordHash = "Ads6*6" }
            //);

            //modelBuilder.Entity<Patient>().HasData(
            //    new Patient
            //    {
            //        PatientId = 1,
            //        Gender = Gender.Female,
            //        DateOfBirth = new DateOnly(2000, 1, 1),
            //        Country = "Egypt",
            //        Address = "Cairo",
            //        PatientDiseases = "Clear",
            //        PatientDrugs = "Paracetamol",
            //        AccountId = guid1
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
            //    new Pharmacy { PharmacyID = 1, Country = "Egypt", Address = "Nasr City", AccountId = guid2 }
            //);

            //modelBuilder.Entity<PharmacyStock>().HasData(
            //    new PharmacyStock { DrugId = 1, PharmacyId = 1, Price = 15.00m, QuantityAvailable = 50 }
            //);

            //    modelBuilder.Entity<PatientFavoriteDrug>().HasData(
            //        new PatientFavoriteDrug { PatientId = 1, DrugId = 1 }
            //    );

            //    modelBuilder.Entity<CartItem>().HasData(
            //        new CartItem
            //        {
            //            PatientId = 1,
            //            DrugId = 1,
            //            PharmacyId = 1,
            //            Quantity = 1
            //        }
            //    );
        }

        // *********DB SETS********* //
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<PharmacyStock> PharmacyStocks { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<PatientFavoriteDrug> PatientFavoriteDrugs { get; set; }
        
    }
}
