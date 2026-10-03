using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Data.Seed;
using KeyBasedAuthenticationTemplate_API.ErrorHandling;
using KeyBasedAuthenticationTemplate_API.Extensions;
using KeyBasedAuthenticationTemplate_API.OpenApi;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

namespace KeyBasedAuthenticationTemplate_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // The .env file lives at the solution root (one level above this project),
            // alongside .example.env. Loaded before configuration is read so its values
            // flow in through the standard environment-variables configuration provider.
            var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "env", ".env");
            if (File.Exists(envFilePath))
            {
                DotNetEnv.Env.Load(envFilePath);
            }

            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<SecuritySchemeTransformer>();
            });

            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb")));

            builder.Services.AddCustomAuthentication(builder.Configuration);
            builder.Services.AddApplicationServices();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
                await DbInitializer.SeedAsync(app.Services);
            }

            app.UseExceptionHandler();
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }
    }
}
