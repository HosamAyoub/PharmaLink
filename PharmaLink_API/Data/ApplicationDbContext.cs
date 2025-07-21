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
                .IncludeProperties(d => new { d.CommonName, d.ActiveIngredient });



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


            // Configure Pharmacy entity

            modelBuilder.Entity<Pharmacy>()
                .HasIndex(p => p.Name)
                .IsUnique();
            // Rate validation (enforces [Range(0,5)] at database level)
            modelBuilder.Entity<Pharmacy>()
                .Property(p => p.Rate)
                .HasColumnType("decimal(3,1)") // Stores values like 4.5
                .HasPrecision(3, 1);

            modelBuilder.Entity<Pharmacy>()
                .Property(p => p.StartHour)
                .HasConversion(
                    timeOnly => timeOnly.HasValue ? timeOnly.Value.ToTimeSpan() : (TimeSpan?)null,
                    timeSpan => timeSpan.HasValue ? TimeOnly.FromTimeSpan(timeSpan.Value) : (TimeOnly?)null);
            modelBuilder.Entity<Pharmacy>()
                .Property(p => p.EndHour)
                .HasConversion(
                    timeOnly => timeOnly.HasValue ? timeOnly.Value.ToTimeSpan() : (TimeSpan?)null,
                    timeSpan => timeSpan.HasValue ? TimeOnly.FromTimeSpan(timeSpan.Value) : (TimeOnly?)null);
                

            

            base.OnModelCreating(modelBuilder);

            SeedPharmacies(modelBuilder);

        }
        //// *********Seed tables********* //
        //    const string guid1 = "d5b5b5e5-5e5e-5e5e-5e5e-5e5e5e5e5e5e";
        //    const string guid2 = "d5b5b5e5-5e5e-5e5e-5e5e-5e5e5e5e5e5d";

        //    modelBuilder.Entity<Account>().HasData(
        //     new Account
        //     {
        //         Id = guid1,
        //         UserName = "patient1",
        //         NormalizedUserName = "PATIENT1",
        //         Email = "patient1@example.com",
        //         NormalizedEmail = "PATIENT1@EXAMPLE.COM",
        //         EmailConfirmed = true,
        //         PasswordHash = "AQAAAAIAAYagAAAAEHYx1JwHk7QbX1YpJxTwWk+YqGE=", // Hash for "123"
        //         PhoneNumber = "1234567890",
        //         PhoneNumberConfirmed = true,
        //         SecurityStamp = Guid.NewGuid().ToString(),
        //         ConcurrencyStamp = Guid.NewGuid().ToString()
        //     }
        //     );

        //     modelBuilder.Entity<Patient>().HasData(
        //        new Patient
        //        {
        //            PatientId = 1,
        //            Name = "Patient1",
        //            Gender = Gender.Female,
        //            DateOfBirth = new DateOnly(2000, 1, 1),
        //            Country = "Egypt",
        //            Address = "Cairo",
        //            PatientDiseases = "Clear",
        //            PatientDrugs = "Paracetamol",
        //            AccountId = guid1
        //        }
        //      );

        //    modelBuilder.Entity<Drug>().HasData(
        //        new Drug
        //        {
        //            DrugID = 1,
        //            CommonName = "Panadol",
        //            Category = "Painkiller",
        //            ActiveIngredient = "Paracetamol",
        //            Alternatives_names = "Tylenol",
        //            AlternativesGpID = 100,
        //            Indications_and_usage = "Headache, fever",
        //            Dosage_and_administration = "500mg twice daily",
        //            Dosage_forms_and_strengths = "Tablet 500mg",
        //            Contraindications = "Liver disease",
        //            Warnings_and_cautions = "Don't exceed 4g/day",
        //            Drug_interactions = "Warfarin",
        //            Description = "Pain reliever",
        //            Storage_and_handling = "Keep cool and dry",
        //            Adverse_reactions = "Nausea",
        //            Drug_UrlImg = "/images/panadol.png"
        //        }
        //     );

        //    modelBuilder.Entity<Pharmacy>().HasData(
        //        new Pharmacy { PharmacyID = 1, Name = "Pharmacy1", Country = "Egypt", Address = "Nasr City", AccountId = guid2 }
        //    );

        //    modelBuilder.Entity<PharmacyStock>().HasData(
        //        new PharmacyStock { DrugId = 1, PharmacyId = 1, Price = 15.00m, QuantityAvailable = 50 }
        //    );

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

        private void SeedPharmacies(ModelBuilder modelBuilder)
        {
            // Pharmacy Accounts
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = "11111111-1111-1111-1111-111111111111",
                    UserName = "pharmacy1",
                    NormalizedUserName = "PHARMACY1",
                    Email = "pharmacy1@example.com",
                    NormalizedEmail = "PHARMACY1@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEHYx1JwHk7QbX1YpJxTwWk+YqGE=", // Hash for "123"
                    PhoneNumber = "01012345678",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "7D8C9A2B4E6F1C3A5B9D8E7F6A5C4B3A", // Static value
                    ConcurrencyStamp = "1A2B3C4D5E6F7G8H9I0J1K2L3M4N5O6P" // Static value
                },
                new Account
                {
                    Id = "22222222-2222-2222-2222-222222222222",
                    UserName = "pharmacy2",
                    NormalizedUserName = "PHARMACY2",
                    Email = "pharmacy2@example.com",
                    NormalizedEmail = "PHARMACY2@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEHYx1JwHk7QbX1YpJxTwWk+YqGE=",
                    PhoneNumber = "01023456789",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "8E9F0A1B2C3D4E5F6G7H8I9J0K1L2M3N", // Static value
                    ConcurrencyStamp = "2B3C4D5E6F7G8H9I0J1K2L3M4N5O6P7Q" // Static value
                },
                new Account
                {
                    Id = "33333333-3333-3333-3333-333333333333",
                    UserName = "pharmacy3",
                    NormalizedUserName = "PHARMACY3",
                    Email = "pharmacy3@example.com",
                    NormalizedEmail = "PHARMACY3@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEHYx1JwHk7QbX1YpJxTwWk+YqGE=",
                    PhoneNumber = "01034567890",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "9F0A1B2C3D4E5F6G7H8I9J0K1L2M3N4O", // Static value
                    ConcurrencyStamp = "3C4D5E6F7G8H9I0J1K2L3M4N5O6P7Q8R" // Static value
                }
            );

            // Pharmacies - Using DateTime for TimeOnly conversion
            modelBuilder.Entity<Pharmacy>().HasData(
                new Pharmacy
                {
                    PharmacyID = 1,
                    Name = "City Pharmacy",
                    Country = "Egypt",
                    Address = "123 Main Street, Downtown Cairo",
                    PhoneNumber = "01012345678", // Matches account phone
                    Rate = 4.5,
                    StartHour = TimeOnly.Parse("09:00"),
                    EndHour = TimeOnly.Parse("21:00"),
                    AccountId = "11111111-1111-1111-1111-111111111111"
                },
                new Pharmacy
                {
                    PharmacyID = 2,
                    Name = "Health Plus",
                    Country = "Egypt",
                    Address = "456 Alexandria Corniche, Alexandria",
                    PhoneNumber = "01023456789", // Matches account phone
                    Rate = 4.2,
                    StartHour = TimeOnly.Parse("08:00"),
                    EndHour = TimeOnly.Parse("20:00"),
                    AccountId = "22222222-2222-2222-2222-222222222222"
                },
                new Pharmacy
                {
                    PharmacyID = 3,
                    Name = "MediCare",
                    Country = "Egypt",
                    Address = "789 Nasr City, Cairo",
                    PhoneNumber = "01034567890", // Matches account phone
                    Rate = 4.7,
                    StartHour = TimeOnly.Parse("10:00"),
                    EndHour = TimeOnly.Parse("22:00"),
                    AccountId = "33333333-3333-3333-3333-333333333333"
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
