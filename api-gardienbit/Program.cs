using api_gardienbit.DAL;
using api_gardienbit.Repositories;
using api_gardienbit.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

namespace api_gardienbit
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Repositories
            builder.Services.AddScoped<ClientRepository, ClientRepository>();
            builder.Services.AddScoped<CategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<VaultRepository, VaultRepository>();
            builder.Services.AddScoped<PwdPackageRepository, PwdPackageRepository>();
            builder.Services.AddScoped<LogActionRepository, LogActionRepository>();
            builder.Services.AddScoped<LogRepository, LogRepository>();
            builder.Services.AddScoped<VaultSessionRepository, VaultSessionRepository>();
            builder.Services.AddScoped<VaultUserLinkRepository, VaultUserLinkRepository>();

            // Custom services
            builder.Services.AddScoped<UserIdentifierService, UserIdentifierService>();
            builder.Services.AddScoped<UserIdentifierMiddleware, UserIdentifierMiddleware>();
            builder.Services.AddScoped<UserCacheService, UserCacheService>();
            builder.Services.AddScoped<LoggingService, LoggingService>();
            builder.Services.AddScoped<TotpService, TotpService>();
            builder.Services.AddScoped<ExceptionMiddleware, ExceptionMiddleware>();

            // Custom services
            builder.Services.AddScoped<UserIdentifierService, UserIdentifierService>();
            builder.Services.AddScoped<UserIdentifierMiddleware, UserIdentifierMiddleware>();
            builder.Services.AddScoped<UserCacheService, UserCacheService>();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure the DbContext with the connection string from appsettings.json
            builder.Services.AddDbContext<EFContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<FixturesServices, FixturesServices>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || true)
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var createFixtures = services.GetRequiredService<FixturesServices>();
                        createFixtures.ResetDatabase();
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while resetting the database.");
                    }
                }

            }

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<UserIdentifierMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
