using Microsoft.AspNetCore.Identity;
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
            modelBuilder.Entity<Drug>()
                .HasIndex(d => d.Category)
                .IncludeProperties(d => new { d.CommonName , d.ActiveIngredient });


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

            //OrderDetail-PharmacyStock relationship (many to one)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.PharmacyStock)
                .WithMany(ps => ps.OrderDetails)
                .HasForeignKey(od => new { od.PharmacyId, od.DrugId })
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
            // 1. Drugs
            modelBuilder.Entity<Drug>().HasData(
                new Drug
                {
                    DrugID = 1,
                    CommonName = "Paracetamol",
                    Category = "Painkiller",
                    ActiveIngredient = "Acetaminophen",
                    Alternatives_names = "Panadol, Tylenol",
                    AlternativesGpID = 1,
                    Indications_and_usage = "Fever, headache, pain",
                    Dosage_and_administration = "500mg every 6h",
                    Dosage_forms_and_strengths = "Tablet, 500mg",
                    Contraindications = "Liver disease",
                    Warnings_and_cautions = "Do not exceed 4g/day",
                    Drug_interactions = "Alcohol, Warfarin",
                    Description = "Common OTC analgesic",
                    Storage_and_handling = "Store below 25°C",
                    Adverse_reactions = "Nausea, rash",
                    Drug_UrlImg = "images/Medicine/Paracetamol.jpg"
                }
            );

            // 4. PharmacyStock
            modelBuilder.Entity<PharmacyStock>().HasData(
                new PharmacyStock
                {
                    DrugId = 1,
                    PharmacyId = 2,
                    Price = 10.50m,
                    QuantityAvailable = 100
                }
            );

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
