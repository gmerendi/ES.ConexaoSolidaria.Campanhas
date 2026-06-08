using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Infrastructure.Data;

public sealed class DesignTimeCampanhaDbContextFactory : IDesignTimeDbContextFactory<CampanhaDbContext>
{
    public CampanhaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Campanhas.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<CampanhaDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new CampanhaDbContext(optionsBuilder.Options);
    }
}