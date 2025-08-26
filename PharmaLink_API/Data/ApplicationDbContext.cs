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

            //modelBuilder.Entity<Pharmacy>()
            //    .HasIndex(p => p.Name)
            //    .IsUnique();
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

            ////1. Drugs
            //SeedDrugs(modelBuilder);

            // 2. Roles
            SeedRoles(modelBuilder);

            // 3. Pharmacies
            SeedPharmacies(modelBuilder);

            // 4. Patients
            SeedPatients(modelBuilder);

            //5. PharmacyStock
            SeedPharmacyStocks(modelBuilder);
            //6. OrderDetails
            SeedOrderDetails(modelBuilder);
            // 7. Orders
            SeedOrders(modelBuilder);
            //8. Admin
            SeedAdmin(modelBuilder);
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

                },
                new IdentityRole
                {
                    Id = "4",
                    Name = "pending",
                    NormalizedName = "PENDING"
                },
                new IdentityRole
                {
                    Id = "5",
                    Name = "suspended",
                    NormalizedName = "SUSPENDED"
                }
            );
        }
        private void SeedPharmacies(ModelBuilder modelBuilder)
        {
            var password = "Ads6*6";
            var hasher = new PasswordHasher<Account>();
            var hashedPassword = hasher.HashPassword(new Account(), password);

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
                    PhoneNumber = "01096906912",
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
                    PhoneNumber = "0842112689",
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
                    PhoneNumber = "01200169999",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "9F0A1B2C3D4E5F6G7H8I9J0K1L2M3N4O", // Static value
                    ConcurrencyStamp = "3C4D5E6F7G8H9I0J1K2L3M4N5O6P7Q8R" // Static value
                },
                new Account
                {
                    Id = "77777777-7777-7777-7777-777777777777",
                    UserName = "pharmacy4",
                    NormalizedUserName = "PHARMACY4",
                    Email = "pharmacy4@example.com",
                    NormalizedEmail = "PHARMACY4@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==",
                    PhoneNumber = "01068309213",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6", // Static value
                    ConcurrencyStamp = "4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T" // Static value
                },
                new Account
                {
                    Id = "88888888-8888-8888-8888-888888888888",
                    UserName = "pharmacy5",
                    NormalizedUserName = "PHARMACY5",
                    Email = "pharmacy5@example.com",
                    NormalizedEmail = "PHARMACY5@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==",
                    PhoneNumber = "0846343938",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7", // Static value
                    ConcurrencyStamp = "5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U" // Static value
                },
                new Account
                {
                    Id = "99999999-9999-9999-9999-999999999999",
                    UserName = "pharmacy6",
                    NormalizedUserName = "PHARMACY6",
                    Email = "pharmacy6@example.com",
                    NormalizedEmail = "PHARMACY6@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==",
                    PhoneNumber = "01064206162",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8", // Static value
                    ConcurrencyStamp = "6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V" // Static value
                },
                new Account
                {
                    Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                    UserName = "pharmacy7",
                    NormalizedUserName = "PHARMACY7",
                    Email = "pharmacy7@example.com",
                    NormalizedEmail = "PHARMACY7@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==",
                    PhoneNumber = "0842030536",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9", // Static value
                    ConcurrencyStamp = "7H8I9J0K1L2M3N4O5P6Q7R8S9T0U1V2W" // Static value
                }
            );

            // Pharmacies - Using DateTime for TimeOnly conversion
            modelBuilder.Entity<Pharmacy>().HasData(
                new Pharmacy
                {
                    PharmacyID = 1,
                    Name = "صيدلية كيور - Cure pharmacy",
                    OwnerName = "Dr. Ahmed El-Sayed",
                    Country = "Egypt",
                    Address = "بعد متجر رنين، بعد فيلا المحافظ، طريق، منشأة عبد الله، محافظة الفيوم",
                    PhoneNumber = "01096906912", // Matches account phone
                    Rate = 4.5,
                    StartHour = TimeOnly.Parse("09:00"),
                    EndHour = TimeOnly.Parse("21:00"),
                    Status = Pharmacy_Status.Active,
                    JoinedDate = DateTime.Parse("2025-06-18"),
                    Latitude = 29.324462925569318,
                    Longitude = 30.841825241427312,
                    AccountId = "11111111-1111-1111-1111-111111111111"
                },
                new Pharmacy
                {
                    PharmacyID = 2,
                    Name = "صيدلية الدكتوره رشا",
                    OwnerName = "Dr. Fatma Nour",
                    Country = "Egypt",
                    Address = "أمام فيلا المحافظ بجوار محطة البنزين، أحمد شوقي، أول الفيوم، محافظة الفيوم ",
                    PhoneNumber = "0842112689", // Matches account phone
                    Rate = 4.2,
                    StartHour = TimeOnly.Parse("08:00"),
                    EndHour = TimeOnly.Parse("20:00"),
                    Status = Pharmacy_Status.Active,
                    JoinedDate = DateTime.Parse("2025-06-18"),
                    Latitude = 29.3227417666807,
                    Longitude = 30.84105276526131,
                    AccountId = "22222222-2222-2222-2222-222222222222"
                },
                new Pharmacy
                {
                    PharmacyID = 3,
                    Name = "صيدليات عناية",
                    OwnerName = "Dr. Mohamed Ali",
                    Country = "Egypt",
                    Address = "أمام فيلا المحافظ بجوار مطعم كوك دور، أحمد شوقي، قسم الفيوم، أول الفيوم، محافظة الفيوم ",
                    PhoneNumber = "01200169999", // Matches account phone
                    Rate = 4.7,
                    StartHour = TimeOnly.Parse("10:00"),
                    EndHour = TimeOnly.Parse("22:00"),
                    Status = Pharmacy_Status.Active,
                    JoinedDate = DateTime.Parse("2023-05-15"),
                    Latitude = 29.321918593467597,
                    Longitude = 30.840966934576198,
                    AccountId = "33333333-3333-3333-3333-333333333333"
                },
                new Pharmacy
                {
                    PharmacyID = 4,
                    Name = "صيدليه الجبيلي",
                    Country = "Egypt",
                    OwnerName = "Dr. ahmed Ali",
                    Address = "منشاة عبد الله، قبل معهد الصفوه الازهري، أول الفيوم، محافظة الفيوم",
                    PhoneNumber = "01068309213", // Matches account phone
                    Rate = 4.7,
                    StartHour = TimeOnly.Parse("10:00"),
                    EndHour = TimeOnly.Parse("22:00"),
                    Status = Pharmacy_Status.Active,
                    JoinedDate = DateTime.Parse("2023-05-15"),
                    Latitude = 29.330898305818675,
                    Longitude = 30.841868156769863,
                    AccountId = "77777777-7777-7777-7777-777777777777"
                },
                new Pharmacy
                {
                    PharmacyID = 5,
                    Name = "صيدلية الجبيلى فرع التعاونيات",
                    Country = "Egypt",
                    OwnerName = "Dr. Mohamed medhat",
                    Address = "امام سكن الطالبات، مساكن التعاونيات، قسم الفيوم، أول الفيوم، محافظة الفيوم ",
                    PhoneNumber = "0846343938", // Matches account phone
                    Rate = 4.7,
                    StartHour = TimeOnly.Parse("10:00"),
                    EndHour = TimeOnly.Parse("22:00"),
                    Status = Pharmacy_Status.Active,
                    JoinedDate = DateTime.Parse("2023-05-15"),
                    Latitude = 29.327418761107833,
                    Longitude = 30.830924744418205,
                    AccountId = "88888888-8888-8888-8888-888888888888"
                },
                new Pharmacy
                {
                    PharmacyID = 6,
                    Name = "صيدلية الجهاد",
                    OwnerName = "Dr. Mohamed wali",
                    Country = "Egypt",
                    Address = "مركز, الجامعة، قسم الفيوم، أول الفيوم، محافظة الفيوم  ",
                    PhoneNumber = "01064206162", // Matches account phone
                    Rate = 4.7,
                    StartHour = TimeOnly.Parse("10:00"),
                    EndHour = TimeOnly.Parse("22:00"),
                    Status = Pharmacy_Status.Active,
                    JoinedDate = DateTime.Parse("2023-05-15"),
                    Latitude = 29.327418761107833,
                    Longitude = 30.830924744418205,
                    AccountId = "99999999-9999-9999-9999-999999999999"
                },
                new Pharmacy
                {
                    PharmacyID = 7,
                    Name = "صيدليات سامح عطا ...sameh Atta Pharmacy",
                    Country = "Egypt",
                    OwnerName = "Dr. Sameh Atta",
                    Address = "Sameh Atta Pharmacy، alsayfiat aljadidat in front of Al-Shorouk Private School، محافظة الفيوم",
                    PhoneNumber = "0842030536", // Matches account phone
                    Rate = 4.7,
                    StartHour = TimeOnly.Parse("10:00"),
                    EndHour = TimeOnly.Parse("22:00"),
                    Status = Pharmacy_Status.Active,
                    JoinedDate = DateTime.Parse("2023-05-15"),
                    Latitude = 29.321245083170584,
                    Longitude = 30.82742686952981,
                    AccountId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
                }
            );

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = "11111111-1111-1111-1111-111111111111",
                    RoleId = "2"
                },
                new IdentityUserRole<string>
                {
                    UserId = "22222222-2222-2222-2222-222222222222",
                    RoleId = "2"
                },
                new IdentityUserRole<string>
                {
                    UserId = "33333333-3333-3333-3333-333333333333",
                    RoleId = "2"
                },
                new IdentityUserRole<string>
                {
                    UserId = "77777777-7777-7777-7777-777777777777",
                    RoleId = "2"
                },
                new IdentityUserRole<string>
                {
                    UserId = "88888888-8888-8888-8888-888888888888",
                    RoleId = "2"
                },
                new IdentityUserRole<string>
                {
                    UserId = "99999999-9999-9999-9999-999999999999",
                    RoleId = "2"
                },
                new IdentityUserRole<string>
                {
                    UserId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
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
                Drug_UrlImg = "https://www.bloompharmacy.com/cdn/shop/products/paracetamol-500-mg-20-tablets-606862.jpg?v=1707749818",
                DrugStatus = Status.Approved,
                CreatedByPharmacy = 0,
                IsRead = false,
                CreatedAt = new DateTime(2025, 8, 16, 4, 17, 23, 329, DateTimeKind.Local).AddTicks(1600)

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
                Drug_UrlImg = "https://lh6.googleusercontent.com/proxy/uQRcEtpKh0CN_X9m4XoC-XIsU50ITQYNcXEn6YiF4wxFvvctThypADpbL0xskSrs1hM3d6mJlUmnIJ010DF1YihIVBXZ0lnDUq1jWrS_v0wQ5IZDfOQLkQ7ZrzJaTC0KwA",
                DrugStatus = Status.Approved,
                CreatedByPharmacy = 0,
                IsRead = false,
                CreatedAt = new DateTime(2025, 8, 16, 4, 17, 23, 332, DateTimeKind.Local).AddTicks(1030)
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
                Drug_UrlImg = "https://www.canonbury.com/media/catalog/product/cache/ac001188e3511e11921f4c9c9c586cfc/a/m/amoxicillin_capsules_500mg_15_pl_la_.png",
                DrugStatus = Status.Approved,
                CreatedByPharmacy = 0,
                IsRead = false,
                CreatedAt = new DateTime(2025, 8, 16, 4, 17, 23, 332, DateTimeKind.Local).AddTicks(1058)
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
                    Status = User_Status.Active,
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
                    Status = User_Status.Active,
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
                    Status = User_Status.Active,
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
                // Pharmacy 1 - صيدلية كيور (7 products)
                new PharmacyProduct { DrugId = 1, PharmacyId = 1, Price = 12.50m, QuantityAvailable = 150, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 2, PharmacyId = 1, Price = 18.00m, QuantityAvailable = 85, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 3, PharmacyId = 1, Price = 28.50m, QuantityAvailable = 65, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 4, PharmacyId = 1, Price = 5.75m, QuantityAvailable = 120, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 5, PharmacyId = 1, Price = 45.00m, QuantityAvailable = 40, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 6, PharmacyId = 1, Price = 22.00m, QuantityAvailable = 75, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 7, PharmacyId = 1, Price = 15.50m, QuantityAvailable = 90, Status = Product_Status.Available },

                // Pharmacy 2 - صيدلية الدكتوره رشا (7 products)
                new PharmacyProduct { DrugId = 1, PharmacyId = 2, Price = 10.50m, QuantityAvailable = 200, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 2, PharmacyId = 2, Price = 15.00m, QuantityAvailable = 120, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 3, PharmacyId = 2, Price = 25.00m, QuantityAvailable = 90, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 8, PharmacyId = 2, Price = 35.25m, QuantityAvailable = 60, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 9, PharmacyId = 2, Price = 42.00m, QuantityAvailable = 45, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 10, PharmacyId = 2, Price = 28.75m, QuantityAvailable = 80, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 11, PharmacyId = 2, Price = 18.50m, QuantityAvailable = 55, Status = Product_Status.Available },

                // Pharmacy 3 - صيدليات عناية (7 products)
                new PharmacyProduct { DrugId = 1, PharmacyId = 3, Price = 11.25m, QuantityAvailable = 100, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 2, PharmacyId = 3, Price = 16.50m, QuantityAvailable = 75, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 3, PharmacyId = 3, Price = 26.75m, QuantityAvailable = 55, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 12, PharmacyId = 3, Price = 125.00m, QuantityAvailable = 30, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 13, PharmacyId = 3, Price = 8.25m, QuantityAvailable = 95, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 14, PharmacyId = 3, Price = 65.50m, QuantityAvailable = 25, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 15, PharmacyId = 3, Price = 180.00m, QuantityAvailable = 15, Status = Product_Status.Available },

                // Pharmacy 4 - صيدليه الجبيلي (7 products)
                new PharmacyProduct { DrugId = 4, PharmacyId = 4, Price = 6.00m, QuantityAvailable = 140, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 5, PharmacyId = 4, Price = 43.50m, QuantityAvailable = 50, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 6, PharmacyId = 4, Price = 20.75m, QuantityAvailable = 85, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 16, PharmacyId = 4, Price = 95.00m, QuantityAvailable = 35, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 17, PharmacyId = 4, Price = 85.25m, QuantityAvailable = 40, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 18, PharmacyId = 4, Price = 24.50m, QuantityAvailable = 70, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 19, PharmacyId = 4, Price = 52.00m, QuantityAvailable = 45, Status = Product_Status.Available },

                // Pharmacy 5 - صيدلية الجبيلى فرع التعاونيات (7 products)
                new PharmacyProduct { DrugId = 7, PharmacyId = 5, Price = 14.75m, QuantityAvailable = 110, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 8, PharmacyId = 5, Price = 33.00m, QuantityAvailable = 65, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 9, PharmacyId = 5, Price = 40.50m, QuantityAvailable = 50, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 20, PharmacyId = 5, Price = 32.25m, QuantityAvailable = 60, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 1, PharmacyId = 5, Price = 11.00m, QuantityAvailable = 180, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 3, PharmacyId = 5, Price = 27.50m, QuantityAvailable = 70, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 12, PharmacyId = 5, Price = 120.00m, QuantityAvailable = 28, Status = Product_Status.Available },

                // Pharmacy 6 - صيدلية الجهاد (7 products)
                new PharmacyProduct { DrugId = 10, PharmacyId = 6, Price = 30.00m, QuantityAvailable = 75, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 11, PharmacyId = 6, Price = 19.25m, QuantityAvailable = 60, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 13, PharmacyId = 6, Price = 7.75m, QuantityAvailable = 100, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 14, PharmacyId = 6, Price = 68.00m, QuantityAvailable = 22, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 15, PharmacyId = 6, Price = 175.50m, QuantityAvailable = 18, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 2, PharmacyId = 6, Price = 17.25m, QuantityAvailable = 95, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 4, PharmacyId = 6, Price = 5.50m, QuantityAvailable = 130, Status = Product_Status.Available },

                // Pharmacy 7 - صيدليات سامح عطا (8 products)
                new PharmacyProduct { DrugId = 16, PharmacyId = 7, Price = 92.50m, QuantityAvailable = 40, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 17, PharmacyId = 7, Price = 82.00m, QuantityAvailable = 45, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 18, PharmacyId = 7, Price = 23.75m, QuantityAvailable = 80, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 19, PharmacyId = 7, Price = 50.50m, QuantityAvailable = 50, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 20, PharmacyId = 7, Price = 31.00m, QuantityAvailable = 65, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 5, PharmacyId = 7, Price = 46.25m, QuantityAvailable = 38, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 6, PharmacyId = 7, Price = 21.50m, QuantityAvailable = 88, Status = Product_Status.Available },
                new PharmacyProduct { DrugId = 8, PharmacyId = 7, Price = 34.75m, QuantityAvailable = 55, Status = Product_Status.Available }
            );
        }
        private void SeedOrders(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    OrderID = 1,
                    TotalPrice = 45.50m,
                    Status = "Delivered",
                    PaymentMethod = "Credit Card",
                    PaymentStatus = "Approved",
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
                    Status = "Delivered",
                    PaymentMethod = "PayPal",
                    PaymentStatus = "Approved",
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
                    Status = "Delivered",
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

        //8. Admin
        private void SeedAdmin(ModelBuilder modelBuilder)
        {
            // Patient Accounts
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = "00000000-0000-0000-0000-000000000000",
                    UserName = "hosam",
                    NormalizedUserName = "HOSAM",
                    Email = "hosam@admin.com",
                    NormalizedEmail = "HOSAM@ADMIN.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
                    PhoneNumber = "01045678910",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6", // Static value
                    ConcurrencyStamp = "4D5E6F7G8H9I0J1K2L3M4N5O6P7Q8R9S" // Static value
                },
                new Account
                {
                    Id = "99999999-9999-9999-9999-999999999998",
                    UserName = "abdo",
                    NormalizedUserName = "ABDO",
                    Email = "abdo@admin.com",
                    NormalizedEmail = "ABDO@ADMIN.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
                    PhoneNumber = "01045678911",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6", // Static value
                    ConcurrencyStamp = "4D5E6F7G8H9I0J1K2L3M4N5O6P7Q8R9S" // Static value
                },
                new Account
                {
                    Id = "99999999-9999-9999-9999-999999999997",
                    UserName = "Zakaria",
                    NormalizedUserName = "ZAKARIA",
                    Email = "zakaria@admin.com",
                    NormalizedEmail = "ZAKARIA@ADMIN.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
                    PhoneNumber = "01045678912",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6", // Static value
                    ConcurrencyStamp = "4D5E6F7G8H9I0J1K2L3M4N5O6P7Q8R9S" // Static value
                },
                new Account
                {
                    Id = "99999999-9999-9999-9999-999999999996",
                    UserName = "Mariem",
                    NormalizedUserName = "MARIEM",
                    Email = "mariem@admin.com",
                    NormalizedEmail = "MARIEM@ADMIN.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
                    PhoneNumber = "01045678913",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6", // Static value
                    ConcurrencyStamp = "4D5E6F7G8H9I0J1K2L3M4N5O6P7Q8R9S" // Static value
                },
                new Account
                {
                    Id = "99999999-9999-9999-9999-999999999995",
                    UserName = "Ayman",
                    NormalizedUserName = "ayman",
                    Email = "ayman@admin.com",
                    NormalizedEmail = "AYMAN@ADMIN.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEFTj4mquLwOc1c1NB1+xuRpdZLOn3yWriL5bvub4bhK+BU1+xQbPOTHbYNoyg/OL9A==", // Hash for "Ads6*6"
                    PhoneNumber = "01045678914",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6", // Static value
                    ConcurrencyStamp = "4D5E6F7G8H9I0J1K2L3M4N5O6P7Q8R9S" // Static value
                }
            );

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = "99999999-9999-9999-9999-999999999995",
                    RoleId = "1"
                },
                new IdentityUserRole<string>
                {
                    UserId = "99999999-9999-9999-9999-999999999996",
                    RoleId = "1"
                },
                new IdentityUserRole<string>
                {
                    UserId = "99999999-9999-9999-9999-999999999997",
                    RoleId = "1"
                },
                new IdentityUserRole<string>
                {
                    UserId = "99999999-9999-9999-9999-999999999998",
                    RoleId = "1"
                },
                new IdentityUserRole<string>
                {
                    UserId = "00000000-0000-0000-0000-000000000000",
                    RoleId = "1"
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
