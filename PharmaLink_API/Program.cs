
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository;
using PharmaLink_API.Repository.IRepository;
using System.Text;

namespace PharmaLink_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //........................
            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("My Policiy", builderOptions =>
                {
                    builderOptions.AllowAnyOrigin() // Allow any origin
                        .AllowAnyMethod()           // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
                        .AllowAnyHeader();          // Allow any header
                });
            });

            builder.Services.AddAutoMapper(typeof(Program));

            builder.Services.AddIdentity<Account, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            // IMPORTANT: Add Identity FIRST, then configure Authentication
            //builder.Services.AddIdentity<Account, IdentityRole>(options =>
            //{
            //    // Configure Identity options if needed
            //    options.Password.RequireDigit = true;
            //    options.Password.RequiredLength = 6;
            //    options.User.RequireUniqueEmail = true;
            //})
            //.AddEntityFrameworkStores<ApplicationDbContext>()
            //.AddDefaultTokenProviders();

            // THEN configure JWT Authentication - this will override Identity's default schemes
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;   // Make the authentication schema based on Bearer methods (based on token no cookie) -> Where & How?
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;      // Check if user invalid redirect to login page as he is unauthorize -> What
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;               // Make JWT Bearer as the default scheme (When no specific scheme is mentioned)

            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;                                   // Save the token in the request header
                options.RequireHttpsMetadata = false;                       // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,                                  // Check if the token is issued by a valid issuer
                    ValidIssuer = builder.Configuration["JWT:Issuer"],      // Issuer of the token
                    ValidateAudience = true,                                // Check if the token is issued for a valid audience
                    ValidAudience = builder.Configuration["JWT:Audience"],  // Audience of the token
                    ValidateLifetime = true,                                // Check if the token is not expired
                    ValidateIssuerSigningKey = true,                        // Check if the token is signed by a valid key
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),   // Key used to sign the token
                    ClockSkew = TimeSpan.Zero               // No clock skew, meaning the token is valid immediately after issuance
                };

                //// Add event handlers for debugging
                //options.Events = new JwtBearerEvents
                //{
                //    OnAuthenticationFailed = context =>
                //    {
                //        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                //        return Task.CompletedTask;
                //    },
                //    OnChallenge = context =>
                //    {
                //        Console.WriteLine($"Authentication challenge: {context.Error}, {context.ErrorDescription}");
                //        return Task.CompletedTask;
                //    },
                //    OnTokenValidated = context =>
                //    {
                //        Console.WriteLine("Token validated successfully");
                //        return Task.CompletedTask;
                //    }
                //};
            });

            // Register repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IDrugRepository, DrugRepoServices>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("My Policiy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
