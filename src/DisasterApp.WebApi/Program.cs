using CloudinaryDotNet;
using DisasterApp.Application.Services;
using DisasterApp.Application.Services.Implementations;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Application.Settings;
using DisasterApp.Hubs;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Infrastructure.Repositories;
using DisasterApp.Infrastructure.Repositories.Implementations;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using DisasterApp.WebApi.Authorization;
using DisasterApp.WebApi.Hubs;
using DisasterApp.WebApi.Middleware;
using DisasterApp.WebApi.Services;
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
using System.Text.Json.Serialization;

namespace DisasterApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Entity Framework
            builder.Services.AddDbContext<DisasterDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        sqlOptions.CommandTimeout(60); // Increase timeout to 60 seconds for large audit queries
                    }));

            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

            builder.Services.AddSingleton(x =>
            {
                var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
                return new Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret));
            });

            // Add repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            builder.Services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
            builder.Services.AddScoped<IBackupCodeRepository, BackupCodeRepository>();
            builder.Services.AddScoped<IOtpAttemptRepository, OtpAttemptRepository>();
            builder.Services.AddScoped<IDisasterTypeRepository, DisasterTypeRepository>();
            builder.Services.AddScoped<IDisasterEventRepository, DisasterEventRepository>();
            builder.Services.AddScoped<IDisasterReportRepository, DisasterReportRepository>();
            builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
            builder.Services.AddScoped<IImpactTypeRepository, ImpactTypeRepository>();
            builder.Services.AddScoped<IImpactDetailRepository, ImpactDetailRepository>();
            builder.Services.AddScoped<ISupportRequestRepository, SupportRequestRepository>();
            builder.Services.AddScoped<IUserBlacklistRepository, UserBlacklistRepository>();

            // Add services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IPasswordValidationService, PasswordValidationService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IDisasterTypeService, DisasterTypeService>();
            builder.Services.AddScoped<IDisasterEventService, DisasterEventService>();
            builder.Services.AddScoped<IDisasterReportService, DisasterReportService>();
            builder.Services.AddScoped<IPhotoService, PhotoService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
            builder.Services.AddScoped<IImpactTypeService, ImpactTypeService>();
            builder.Services.AddScoped<IImpactDetailService, ImpactDetailService>();
            builder.Services.AddScoped<ISupportRequestService, SupportRequestService>();
            builder.Services.AddScoped<IBlacklistService, BlacklistService>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            // Add Two-Factor Authentication services
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IBackupCodeService, BackupCodeService>();
            builder.Services.AddScoped<IRateLimitingService, RateLimitingService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<INotificationHubService, NotificationHubService>();

            // Add Email OTP services
            builder.Services.AddScoped<IEmailOtpService, EmailOtpService>();

            // Add Enhanced Audit System services
            builder.Services.AddScoped<IAuditTargetValidator, AuditTargetValidator>();
            builder.Services.AddScoped<IAuditDataSanitizer, AuditDataSanitizer>();
            builder.Services.AddScoped<IExportService, ExportService>();
            builder.Services.AddScoped<IDonationAuditService, DonationAuditService>();
            builder.Services.AddScoped<IOrganizationAuditService, OrganizationAuditService>();
            builder.Services.AddScoped<IAuditRetentionService, AuditRetentionService>();

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
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/userStatsHub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["GoogleAuth:ClientId"] ?? throw new InvalidOperationException("Google Client ID not configured");
                options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"] ?? throw new InvalidOperationException("Google Client Secret not configured");
            });

            // Add services to the container.
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // Add SignalR
            builder.Services.AddSignalR();

            builder.Services.AddScoped<IUserStatsHubService, UserStatsHubService>();
            builder.Services.AddHttpClient("Nominatim", client =>
            {
                client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DisasterApp/1.0 (kaungthantlinn78@gmail.com)");
            });

            // Add Cookie Policy for secure refresh token storage
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false; // Disable consent for API
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.SameAsRequest; // Use HTTPS in production
            });

            // Add CORS (optimized for Google OAuth)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true) // Allow any origin for development
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

            // Initialize and seed the database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DisasterDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Ensuring database is created and migrated...");

                    // Ensure database is created (this will create the database if it doesn't exist)
                    logger.LogInformation("Ensuring database is created and migrated...");
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Database creation completed successfully.");

                    logger.LogInformation("Seeding database...");
                    await DataSeeder.SeedAsync(services);
                    logger.LogInformation("Database seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while initializing or seeding the database");
                    throw; // Re-throw to prevent application startup with broken database
                }
            }

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DisasterApp API v1");
                c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
            });

            // Add security headers (optimized for Google OAuth)
            app.Use(async (context, next) =>
            {
                // Allow same-origin-allow-popups for Google OAuth popups
                if (!context.Response.Headers.ContainsKey("Cross-Origin-Opener-Policy"))
                    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
                if (!context.Response.Headers.ContainsKey("Cross-Origin-Embedder-Policy"))
                    context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "unsafe-none");
                if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
                    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
                    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
                if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
                    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                await next();
            });


            app.UseCors("AllowAll");
            app.UseCookiePolicy();

            // Only use HTTPS redirection in production or when HTTPS is configured
            if (app.Environment.IsProduction() || builder.Configuration.GetValue<string>("ASPNETCORE_URLS")?.Contains("https") == true)
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseMiddleware<AuditLogMiddleware>();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<UserStatsHub>("/userStatsHub");
            app.MapHub<NotificationHub>("/notificationHub");



            app.Run();
        }
    }
}