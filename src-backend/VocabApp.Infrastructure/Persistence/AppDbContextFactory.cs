using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VocabApp.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var apiSettings = Path.Combine(basePath, "appsettings.json");
        var apiDevSettings = Path.Combine(basePath, "appsettings.Development.json");

        if (!File.Exists(apiSettings))
        {
            apiSettings = Path.Combine(basePath, "src", "VocabApp.API", "appsettings.json");
            apiDevSettings = Path.Combine(basePath, "src", "VocabApp.API", "appsettings.Development.json");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(apiSettings, optional: false)
            .AddJsonFile(apiDevSettings, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' is missing.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
