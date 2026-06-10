using Campanhas.Api.Configuration;
using Campanhas.Api.Extensions;
using Campanhas.Api.Middlewares;
using Campanhas.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
var logCounter = 0;
var logTotal = 10;

// ──────────────────────────────────────────────────────────────────────────────
// ── Configuration
// ──────────────────────────────────────────────────────────────────────────────
ConfigureAppSettings(builder.Configuration);


// ──────────────────────────────────────────────────────────────────────────────
// ── Logs
// ──────────────────────────────────────────────────────────────────────────────
using var loggerFactory = LoggerFactory.Create(logging => {
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddSimpleConsole();
});
var logger = loggerFactory.CreateLogger("Program");
logger.LogInformation(" ***** ({0}/{1}) - Inicializando Campanhas API ", logCounter++, logTotal);


// ──────────────────────────────────────────────────────────────────────────────
// ── Api
// ──────────────────────────────────────────────────────────────────────────────
logger.LogInformation(" ***** ({0}/{1}) - Inicio inicialização de Endpoints ", logCounter++, logTotal);
builder.Services.AddControllers().ConfigurarMensagensDeValidacaoCustomizadas().ConfigurarErrosDeValidacaoCustomizados();
builder.Services.AddEndpointsApiExplorer();
logger.LogInformation(" ***** ({0}/{1}) - Termino inicialização de Endpoints ", logCounter, logTotal);


// ──────────────────────────────────────────────────────────────────────────────
// ── Api Extensions
// ──────────────────────────────────────────────────────────────────────────────
logger.LogInformation(" ***** ({0}/{1}) - Inicio inicialização de Api Extensions ", logCounter++, logTotal);
builder.Services.AddSwaggerConfiguration(logger);
logger.LogInformation(" ***** ({0}/{1}) - Termino inicialização de Api Extensions ", logCounter, logTotal);


// ──────────────────────────────────────────────────────────────────────────────
// ── Application Extensions
// ──────────────────────────────────────────────────────────────────────────────
logger.LogInformation(" ***** ({0}/{1}) - Inicio inicialização de Application Extensions ", logCounter++, logTotal);
builder.Services.AddUseCaseServices();
logger.LogInformation(" ***** ({0}/{1}) - Termino inicialização de Application Extensions ", logCounter, logTotal);


// ──────────────────────────────────────────────────────────────────────────────
// ── Domain Extensions
// ──────────────────────────────────────────────────────────────────────────────
logger.LogInformation(" ***** ({0}/{1}) - Inicio inicialização de Domain Extensions ", logCounter++, logTotal);
//builder.Services.AddDomainServices(logger);
logger.LogInformation(" ***** ({0}/{1}) - Termino inicialização de Domain Extensions ", logCounter, logTotal);


// ──────────────────────────────────────────────────────────────────────────────
// ── Infrastructure Extensions
// ──────────────────────────────────────────────────────────────────────────────
logger.LogInformation(" ***** ({0}/{1}) - Inicio inicialização de Infrastructure Extensions ", logCounter++, logTotal);
builder.Services.AddDbContext(builder.Configuration, logger);
builder.Services.AddCustomLogging(logger);
builder.Services.AddRepositories(logger);
builder.Services.AddAuditLog(builder.Configuration, logger);
builder.Services.AddMessaging(builder.Configuration, logger);
builder.Services.AddAuthenticationServices(builder.Configuration, logger);
builder.Services.AddCacheService(builder.Configuration, logger);
builder.Services.AddHealthCheckServices();
logger.LogInformation(" ***** ({0}/{1}) - Termino inicialização de Infrastructure Extensions ", logCounter, logTotal);


var app = builder.Build();


// ──────────────────────────────────────────────────────────────────────────────
// ── Middlewares  (ANTES do MapControllers — ordem correta)
// ──────────────────────────────────────────────────────────────────────────────
logger.LogInformation(" ***** ({0}/{1}) - Inicio inicialização de Middlewares ", logCounter++, logTotal);
app.UseSwaggerMiddleware(logger);
app.UseExceptionMiddleware();
app.UseCorrelationMiddleware();
app.UseTokenBlacklistMiddleware();
logger.LogInformation(" ***** ({0}/{1}) - Termino inicialização de Middlewares ", logCounter, logTotal);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// ──────────────────────────────────────────────────────────────────────────────
// ── Migrations
// ──────────────────────────────────────────────────────────────────────────────
logger.LogInformation(" ***** ({0}/{1}) - Inicio inicialização de Migrations ", logCounter++, logTotal);
app.ApplyMigrations(logger);
logger.LogInformation(" ***** ({0}/{1}) - Termino inicialização de Migrations ", logCounter, logTotal);


// ──────────────────────────────────────────────────────────────────────────────
// ── Health Check Mappings
// ──────────────────────────────────────────────────────────────────────────────
app.MapHealthCheckEndpoints();


app.Run();




#region Helper Methods
void ConfigureAppSettings(ConfigurationManager config)
{
    string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                       ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                       ?? "Production";

    config.SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables();
}
#endregion
