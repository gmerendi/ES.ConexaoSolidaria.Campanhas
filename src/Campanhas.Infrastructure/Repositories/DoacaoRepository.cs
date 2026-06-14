using Campanhas.Domain.Entities.Doacoes;
using Campanhas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Campanhas.Infrastructure.Repositories;

public sealed class DoacaoRepository : EFRepository<Doacao>, IDoacaoRepository
{
    private readonly string _connectionString;

    public DoacaoRepository(ApplicationDbContext context, IConfiguration configuration) : base(context)
    {
        _connectionString = configuration.GetConnectionString("ConnectionString") ?? "";
    }
}
