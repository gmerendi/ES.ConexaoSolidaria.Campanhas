using Campanhas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Infrastructure.Data;

public class CampanhaDbContext : DbContext
{
    public CampanhaDbContext(DbContextOptions<CampanhaDbContext> options) : base(options) { }

    public DbSet<Campanha> Campanhas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampanhaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
