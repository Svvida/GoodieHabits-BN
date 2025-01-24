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
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Parse command-line arguments
            if (args != null && args.Length > 0)
            {
                var configuration = new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build();

                environment = configuration["environment"] ?? environment; // Override if --environment is provided
            }

            Console.WriteLine($"Using environment: {environment}");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var connectionString = configurationBuilder.GetConnectionString("DefaultConnection");

            if (connectionString is not null && connectionString.Contains("Template"))
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
