using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Campanhas.Infrastructure.Data;

public sealed class DesignTimeCampanhaDbContextFactory : IDesignTimeDbContextFactory<CampanhaDbContext>
{
    public CampanhaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CampanhaDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=ConexaoSolidaria_Campanhas;Username=postgres;Password=postgres");
        return new CampanhaDbContext(optionsBuilder.Options);
    }
}
