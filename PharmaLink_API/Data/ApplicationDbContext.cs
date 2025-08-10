using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Models;
using PharmaLink_API.Core.Enums;

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
                .IncludeProperties(d => new { d.CommonName, d.ActiveIngredient });


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
                .HasOne(od => od.PharmacyProduct)
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
            modelBuilder.Entity<PharmacyProduct>(entity =>
            {
                entity.HasKey(pd => new { pd.PharmacyId, pd.DrugId });

                entity.HasOne(pd => pd.Pharmacy)
                      .WithMany(p => p.PharmacyStock)
                      .HasForeignKey(pd => pd.PharmacyId);

                entity.HasOne(pd => pd.Drug)
                      .WithMany(d => d.PharmacyStock)
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

                entity.HasOne(uc => uc.PharmacyProduct)
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

            // *********Seed tables********* //

            // 1. Drugs
            SeedDrugs(modelBuilder);

            // 2. Roles
            SeedRoles(modelBuilder);

            // 3. Pharmacies
            SeedPharmacies(modelBuilder);

            // 4. Patients
            SeedPatients(modelBuilder);

            // 5. PharmacyStock
            SeedPharmacyStocks(modelBuilder);
            // 6. OrderDetails
            SeedOrderDetails(modelBuilder);
            // 7. Orders
            SeedOrders(modelBuilder);
        }


        private void SeedRoles(ModelBuilder modelBuilder)
        {
            // Seed roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "Pharmacy",
                    NormalizedName = "PHARMACY"

                },
                new IdentityRole
                {
                    Id = "3",
                    Name = "Patient",
                    NormalizedName = "PATIENT"

                }
            );


            // 4. PharmacyStock
        //    modelBuilder.Entity<PharmacyProduct>().HasData(
        //        new PharmacyProduct
        //        {
        //            DrugId = 1,
        //            PharmacyId = 2,
        //            Price = 10.50m,
        //            QuantityAvailable = 100
        //        },
        //        new PharmacyProduct
        //        {
        //            DrugId = 2,
        //            PharmacyId = 2,
        //            Price = 15.00m,
        //            QuantityAvailable = 50
        //        },
        //        new PharmacyProduct
        //        {
        //            DrugId = 3,
        //            PharmacyId = 2,
        //            Price = 25.00m,
        //            QuantityAvailable = 70
        //        }
        //    );
        }
        private void SeedPharmacies(ModelBuilder modelBuilder)
        {
            var password = "Ads6*6";
            var hasher = new PasswordHasher<object>();
            var hashedPassword = hasher.HashPassword(null, password);
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
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
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
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==",
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
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==",
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

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                { UserId = "11111111-1111-1111-1111-111111111111",
                    RoleId = "2" },
                new IdentityUserRole<string>
                {
                    UserId = "22222222-2222-2222-2222-222222222222",
                    RoleId = "2"
                },
                new IdentityUserRole<string>
                {
                    UserId = "33333333-3333-3333-3333-333333333333",
                    RoleId = "2"
                }
                );
        }
        private void SeedDrugs(ModelBuilder modelBuilder)
        {
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
                Drug_UrlImg = "https://www.bloompharmacy.com/cdn/shop/products/paracetamol-500-mg-20-tablets-606862.jpg?v=1707749818"
            },
            new Drug
            {
                DrugID = 2,
                CommonName = "Ibuprofen",
                Category = "Painkiller",
                ActiveIngredient = "Ibuprofen",
                Alternatives_names = "Advil, Motrin",
                AlternativesGpID = 2,
                Indications_and_usage = "Pain, inflammation, fever",
                Dosage_and_administration = "200mg every 6–8h",
                Dosage_forms_and_strengths = "Tablet, 200mg",
                Contraindications = "Ulcers, kidney disease",
                Warnings_and_cautions = "Avoid long-term use",
                Drug_interactions = "Aspirin, Warfarin",
                Description = "NSAID used for pain and inflammation",
                Storage_and_handling = "Store below 25°C",
                Adverse_reactions = "GI bleeding, headache",
                Drug_UrlImg = "https://lh6.googleusercontent.com/proxy/uQRcEtpKh0CN_X9m4XoC-XIsU50ITQYNcXEn6YiF4wxFvvctThypADpbL0xskSrs1hM3d6mJlUmnIJ010DF1YihIVBXZ0lnDUq1jWrS_v0wQ5IZDfOQLkQ7ZrzJaTC0KwA"
            },
            new Drug
            {
                DrugID = 3,
                CommonName = "Amoxicillin",
                Category = "Antibiotic",
                ActiveIngredient = "Amoxicillin",
                Alternatives_names = "Moxatag, Trimox",
                AlternativesGpID = 3,
                Indications_and_usage = "Bacterial infections",
                Dosage_and_administration = "500mg every 8h",
                Dosage_forms_and_strengths = "Capsule, 500mg",
                Contraindications = "Penicillin allergy",
                Warnings_and_cautions = "Complete full course",
                Drug_interactions = "Methotrexate, Warfarin",
                Description = "Broad-spectrum antibiotic",
                Storage_and_handling = "Store in a cool, dry place",
                Adverse_reactions = "Diarrhea, rash",
                Drug_UrlImg = "https://www.canonbury.com/media/catalog/product/cache/ac001188e3511e11921f4c9c9c586cfc/a/m/amoxicillin_capsules_500mg_15_pl_la_.png"
            }
        );
        }
        private void SeedPatients(ModelBuilder modelBuilder)
        {
            // Patient Accounts
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = "44444444-4444-4444-4444-444444444444",
                    UserName = "patient1",
                    NormalizedUserName = "PATIENT1",
                    Email = "patient1@example.com",
                    NormalizedEmail = "PATIENT1@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
                    PhoneNumber = "01045678901",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6", // Static value
                    ConcurrencyStamp = "4D5E6F7G8H9I0J1K2L3M4N5O6P7Q8R9S" // Static value
                },
                new Account
                {
                    Id = "55555555-5555-5555-5555-555555555555",
                    UserName = "patient2",
                    NormalizedUserName = "PATIENT2",
                    Email = "patient2@example.com",
                    NormalizedEmail = "PATIENT2@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
                    PhoneNumber = "01056789012",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7", // Static value
                    ConcurrencyStamp = "5E6F7G8H9I0J1K2L3M4N5O6P7Q8R9S0T" // Static value
                },
                new Account
                {
                    Id = "66666666-6666-6666-6666-666666666666",
                    UserName = "patient3",
                    NormalizedUserName = "PATIENT3",
                    Email = "patient3@example.com",
                    NormalizedEmail = "PATIENT3@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
                    PhoneNumber = "01067890123",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8", // Static value
                    ConcurrencyStamp = "6F7G8H9I0J1K2L3M4N5O6P7Q8R9S0T1U" // Static value
                }
            );

            // Patients
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    PatientId = 1,
                    Name = "Ahmed Hassan",
                    Gender = Gender.Male,
                    DateOfBirth = new DateOnly(1995, 3, 15),
                    Country = "Egypt",
                    Address = "15 Tahrir Square, Downtown Cairo",
                    MedicalHistory = "Hypertension",
                    Medications = "Amlodipine 5mg daily",
                    Allergies = "Latex",
                    AccountId = "44444444-4444-4444-4444-444444444444"
                },
                new Patient
                {
                    PatientId = 2,
                    Name = "Fatima El-Zahra",
                    Gender = Gender.Female,
                    DateOfBirth = new DateOnly(1988, 7, 22),
                    Country = "Egypt",
                    Address = "27 Nile Corniche, Alexandria",
                    MedicalHistory = "Diabetes Type 2",
                    Medications = "Metformin 500mg twice daily",
                    Allergies = "Peanuts",
                    AccountId = "55555555-5555-5555-5555-555555555555"
                },
                new Patient
                {
                    PatientId = 3,
                    Name = "Omar Khaled",
                    Gender = Gender.Male,
                    DateOfBirth = new DateOnly(2000, 11, 8),
                    Country = "Egypt",
                    Address = "42 University Street, Giza",
                    MedicalHistory = "Asthma",
                    Medications = "Salbutamol inhaler as needed",
                    Allergies = "Shellfish",
                    AccountId = "66666666-6666-6666-6666-666666666666"
                }
            );


            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = "44444444-4444-4444-4444-444444444444",
                    RoleId = "3"
                },
                new IdentityUserRole<string>
                {
                    UserId = "55555555-5555-5555-5555-555555555555",
                    RoleId = "3"
                },
                new IdentityUserRole<string>
                {
                    UserId = "66666666-6666-6666-6666-666666666666",
                    RoleId = "3"
                }
                );
        }
        private void SeedPharmacyStocks(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PharmacyProduct>().HasData(
                // City Pharmacy (PharmacyID = 1) - Premium pricing, good stock
                new PharmacyProduct
                {
                    DrugId = 1, // Paracetamol
                    PharmacyId = 1,
                    Price = 12.50m,
                    QuantityAvailable = 150
                },
                new PharmacyProduct
                {
                    DrugId = 2, // Ibuprofen
                    PharmacyId = 1,
                    Price = 18.00m,
                    QuantityAvailable = 85
                },
                new PharmacyProduct
                {
                    DrugId = 3, // Amoxicillin
                    PharmacyId = 1,
                    Price = 28.50m,
                    QuantityAvailable = 65
                },

                 //Health Plus(PharmacyID = 2) - Competitive pricing, high stock
                new PharmacyProduct
                {
                    DrugId = 1, // Paracetamol
                    PharmacyId = 2,
                    Price = 10.50m,
                    QuantityAvailable = 200
                },
                new PharmacyProduct
                {
                    DrugId = 2, // Ibuprofen
                    PharmacyId = 2,
                    Price = 15.00m,
                    QuantityAvailable = 120
                },
                new PharmacyProduct
                {
                    DrugId = 3, // Amoxicillin
                    PharmacyId = 2,
                    Price = 25.00m,
                    QuantityAvailable = 90
                },

                // MediCare (PharmacyID = 3) - Mid-range pricing, moderate stock
                new PharmacyProduct
                {
                    DrugId = 1, // Paracetamol
                    PharmacyId = 3,
                    Price = 11.25m,
                    QuantityAvailable = 100
                },
                new PharmacyProduct
                {
                    DrugId = 2, // Ibuprofen
                    PharmacyId = 3,
                    Price = 16.50m,
                    QuantityAvailable = 75
                },
                new PharmacyProduct
                {
                    DrugId = 3, // Amoxicillin
                    PharmacyId = 3,
                    Price = 26.75m,
                    QuantityAvailable = 55
                }
            );
        }
        private void SeedOrders(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    OrderID = 1,
                    TotalPrice = 45.50m,
                    Status = "Completed",
                    PaymentMethod = "Credit Card",
                    PaymentStatus = "Paid",
                    OrderDate = DateTime.Parse("2023-05-15"),
                    PharmacyId = 1, // City Pharmacy
                    PatientId = 1, // Ahmed Hassan
                    Name = "Ahmed Hassan",
                    PhoneNumber = "01045678901", // Matches patient1 account
                    Email = "patient1@example.com", // Matches patient1 account
                    Address = "15 Tahrir Square, Downtown Cairo", // Matches patient1 address
                    Country = "Egypt"
                },
                new Order
                {
                    OrderID = 2,
                    TotalPrice = 62.50m,
                    Status = "Processing",
                    PaymentMethod = "PayPal",
                    PaymentStatus = "Paid",
                    OrderDate = DateTime.Parse("2023-06-20"),
                    PharmacyId = 2, // Health Plus
                    PatientId = 2, // Fatima El-Zahra
                    Name = "Fatima El-Zahra",
                    PhoneNumber = "01056789012", // Matches patient2 account
                    Email = "patient2@example.com", // Matches patient2 account
                    Address = "27 Nile Corniche, Alexandria", // Matches patient2 address
                    Country = "Egypt"
                },
                new Order
                {
                    OrderID = 3,
                    TotalPrice = 33.75m,
                    Status = "Pending",
                    PaymentMethod = "Cash on Delivery",
                    PaymentStatus = "Pending",
                    OrderDate = DateTime.Parse("2023-07-10"),
                    PharmacyId = 3, // MediCare
                    PatientId = 3, // Omar Khaled
                    Name = "Omar Khaled",
                    PhoneNumber = "01067890123", // Matches patient3 account
                    Email = "patient3@example.com", // Matches patient3 account
                    Address = "42 University Street, Giza", // Matches patient3 address
                    Country = "Egypt"
                }
            );
        }

        private void SeedOrderDetails(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetail>().HasData(
                // Order 1 - Ahmed Hassan at City Pharmacy
                new OrderDetail
                {
                    OrderDetailId = 1,
                    OrderId = 1,
                    DrugId = 1, // Paracetamol
                    PharmacyId = 1, // City Pharmacy
                    Quantity = 2,
                    Price = 12.50m // Matches City Pharmacy's Paracetamol price
                },
                new OrderDetail
                {
                    OrderDetailId = 2,
                    OrderId = 1,
                    DrugId = 2, // Ibuprofen
                    PharmacyId = 1, // City Pharmacy
                    Quantity = 1,
                    Price = 18.00m // Matches City Pharmacy's Ibuprofen price
                },

                // Order 2 - Fatima El-Zahra at Health Plus
                new OrderDetail
                {
                    OrderDetailId = 3,
                    OrderId = 2,
                    DrugId = 1, // Paracetamol
                    PharmacyId = 2, // Health Plus
                    Quantity = 3,
                    Price = 10.50m // Matches Health Plus's Paracetamol price
                },
                new OrderDetail
                {
                    OrderDetailId = 4,
                    OrderId = 2,
                    DrugId = 3, // Amoxicillin
                    PharmacyId = 2, // Health Plus
                    Quantity = 1,
                    Price = 25.00m // Matches Health Plus's Amoxicillin price
                },
                new OrderDetail
                {
                    OrderDetailId = 5,
                    OrderId = 2,
                    DrugId = 2, // Ibuprofen
                    PharmacyId = 2, // Health Plus
                    Quantity = 1,
                    Price = 15.00m // Matches Health Plus's Ibuprofen price
                },

                // Order 3 - Omar Khaled at MediCare
                new OrderDetail
                {
                    OrderDetailId = 6,
                    OrderId = 3,
                    DrugId = 1, // Paracetamol
                    PharmacyId = 3, // MediCare
                    Quantity = 3,
                    Price = 11.25m // Matches MediCare's Paracetamol price
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
        public DbSet<PharmacyProduct> PharmacyStock { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<PatientFavoriteDrug> PatientFavoriteDrugs { get; set; }

    }
}
