using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PharmaLink_API.Core.Constants;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Core.Middleware;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Models.Profiles;
using PharmaLink_API.Repository;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Repository.IRepository;
using PharmaLink_API.Services;
using PharmaLink_API.Services.Interfaces;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Text;

namespace PharmaLink_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    //builder => builder.WithOrigins("http://localhost:4200")
                    //                  .AllowAnyMethod()
                    //                  .AllowAnyHeader());
                    builder =>builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => true) // Allow all origins
                    );

                options.AddPolicy("MyPolicy", builderOptions =>
                {
                    builderOptions.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            //AutoMapper Configuration
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddAutoMapper(typeof(PharmacyProfile));
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();


            
            builder.Services.AddIdentityCore<Account>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // Allowed characters for usernames
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                // PharmacyAdmin policy: Allows Admins and Pharmacy users with valid pharmacy_id claim
                options.AddPolicy("PharmacyAdmin", policy =>
                {
                    policy.RequireAssertion(context =>
                        context.User.IsInRole(UserRole.Admin.ToRoleString()) ||
                        (context.User.IsInRole(UserRole.Pharmacy.ToRoleString()) && 
                         context.User.Claims.Any(c => c.Type == CustomClaimTypes.PharmacyId))
                    );
                });

                // Admin only policy
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole(UserRole.Admin.ToRoleString());
                });

                // Patient only policy
                options.AddPolicy("PatientOnly", policy =>
                {
                    policy.RequireRole(UserRole.Patient.ToRoleString());
                });

                // Pharmacy only policy
                options.AddPolicy("PharmacyOnly", policy =>
                {
                    policy.RequireRole(UserRole.Pharmacy.ToRoleString());
                });
            });

            // Register repositories
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IPharmacyRepository, PharmacyRepository>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();
            builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            builder.Services.AddScoped<IPharmacyStockRepository, PharmacyStockRepository>();
            builder.Services.AddScoped<IDrugRepository, DrugRepoServices>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();

            // Register services
            builder.Services.AddScoped<IPharmacyStockService, PharmacyStockService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IStripeService, StripeService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IRoleService, RoleService>();



            builder.Services.AddControllers();

            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "PharmaLink API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = @"Enter your JWT token like this: Bearer {your JWT token}Example: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                           Reference = new OpenApiReference
                           {
                              Type = ReferenceType.SecurityScheme,
                              Id = "Bearer"
                           }
                        },
                       Array.Empty<string>()
                    }
                });
            });

            builder.Services.Configure<StripeModel>(builder.Configuration.GetSection("Stripe"));

            if (builder.Environment.IsProduction())
            {
                // Configure Serilog with SourceContext column
                builder.Host.UseSerilog((ctx, lc) => {
                    lc.ReadFrom.Configuration(ctx.Configuration)
                      .WriteTo.MSSqlServer(
                          connectionString: ctx.Configuration.GetConnectionString("DefaultConnection"),
                          sinkOptions: new MSSqlServerSinkOptions
                          {
                              TableName = "LogEvents",
                              AutoCreateSqlTable = true
                          },
                          columnOptions: new ColumnOptions()
                          {
                              AdditionalColumns = new SqlColumn[]
                              {
                              new SqlColumn()
                              {
                                  ColumnName = "SourceContext",
                                  PropertyName = "SourceContext",
                                  DataType = System.Data.SqlDbType.NVarChar,
                                  DataLength = 150,
                                  AllowNull = true
                              }
                              }
                          }
                      );
                });

            }


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            
            // Add global exception handling middleware first
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
