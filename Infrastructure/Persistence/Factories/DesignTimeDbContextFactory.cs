using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Factories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Default environment
            var environment = args != null && args.Length > 0
                ? args[0] // Use the first argument as the environment
                : Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Assume the configuration is in the Api project
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "MyLambdaApi", "src", "MyLambdaApi");

            Console.WriteLine($"Using environment: {environment}");
            Console.WriteLine($"Using basePath: {basePath}");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var connectionString = configurationBuilder.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("Template"))
            {
                throw new InvalidOperationException(
                    $"Connection string 'DefaultConnection' is missing or invalid. Ensure the correct environment is set.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }

    }
}
