using CloudinaryDotNet;
using DisasterApp.Application.Services;
using DisasterApp.Application.Services.Implementations;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Application.Settings;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories;
using DisasterApp.Infrastructure.Repositories.Implementations;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using DisasterApp.WebApi.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Text;


namespace DisasterApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Entity Framework
            builder.Services.AddDbContext<DisasterDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

            builder.Services.AddSingleton(x =>
            {
                var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
                return new Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret));
            });

            // Add repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            builder.Services.AddScoped<IDisasterTypeRepository, DisasterTypeRepository>();
            builder.Services.AddScoped<IDisasterEventRepository, DisasterEventRepository>();
            builder.Services.AddScoped<IDisasterReportRepository, DisasterReportRepository>();
            builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
            
            ;
            // Add services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IPasswordValidationService, PasswordValidationService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IDisasterTypeService, DisasterTypeService>();
            builder.Services.AddScoped<IDisasterEventService, DisasterEventService>();
            builder.Services.AddScoped<IDisasterReportService, DisasterReportService>();
            builder.Services.AddScoped<IPhotoService,PhotoService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();



            // Add authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new RoleRequirement("admin")));
                options.AddPolicy("CjOnly", policy => policy.Requirements.Add(new RoleRequirement("cj")));
                options.AddPolicy("UserOnly", policy => policy.Requirements.Add(new RoleRequirement("user")));
                options.AddPolicy("AdminOrCj", policy => policy.Requirements.Add(new RoleRequirement("admin", "cj")));
            });

            // Add authorization handlers
            builder.Services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();

            // Add JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var key = Encoding.ASCII.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    //NameClaimType = "sub" // Google unique ID

                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["GoogleAuth:ClientId"] ?? throw new InvalidOperationException("Google Client ID not configured");
                options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"] ?? throw new InvalidOperationException("Google Client Secret not configured");
            });

            // Add services to the container.
            builder.Services.AddControllers();

            // ✅ Controller + Newtonsoft.Json Configure
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    // EF Core Navigation Properties တွေရဲ့ Loop Error ကို Prevent
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            //for location
            builder.Services.AddHttpClient("Nominatim", client =>
            {
                client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DisasterApp/1.0 (your-email@example.com)");
            });

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DisasterApp API", Version = "v1" });

                c.SupportNonNullableReferenceTypes();
                // Add JWT authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            var app = builder.Build();

            // Seed the database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await DataSeeder.SeedAsync(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database");
                }
            }

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DisasterApp API v1");
                c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
            });

            // Add security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Cross-Origin-Opener-Policy", "unsafe-none");
                context.Response.Headers.Add("Cross-Origin-Embedder-Policy", "unsafe-none");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                await next();
            });

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}