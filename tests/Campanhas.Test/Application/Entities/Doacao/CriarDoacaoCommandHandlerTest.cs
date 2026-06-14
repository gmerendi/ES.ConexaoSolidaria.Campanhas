using Campanhas.Application.Features.Campanhas;
using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Entities.ValueObjects;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Exceptions;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;
using FluentAssertions;
using Moq;

namespace Campanhas.Tests.Application;

public class CriarDoacaoCommandHandlerTests
{
    private readonly Mock<ICampanhaRepository> _repositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IBaseLogger<AlterarCampanhaCommandHandler>> _loggerMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IMetricsService> _metricsMock;
    private readonly Mock<IElasticSearchService> _elasticSearchMock;

    private readonly CriarDoacaoCommandHandler _handler;

    public CriarDoacaoCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICampanhaRepository>();
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<IBaseLogger<AlterarCampanhaCommandHandler>>();
        _messageServiceMock = new Mock<IMessageService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _metricsMock = new Mock<IMetricsService>();
        _elasticSearchMock = new Mock<IElasticSearchService>();

        _handler = new CriarDoacaoCommandHandler(
            _repositoryMock.Object,
            _userContextMock.Object,
            _loggerMock.Object,
            _messageServiceMock.Object,
            _cacheServiceMock.Object,
            _metricsMock.Object,
            _elasticSearchMock.Object);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Campanha CriarCampanhaAtiva()
        => new(
            TituloCampanha.Create("Campanha Aberta para Doações"),
            "Descrição da campanha que aceita doações",
            MetaFinanceira.Create(7000m),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            "gestor@ong.com");

    private void ConfigurarSolicitanteDoador(
        string email = "doador@email.com",
        string nome = "João Doador",
        string cpf = "111.222.333-44")
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), nome, cpf, email,
            Perfil.DOADOR.ToString(), "ACTIVE");
        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    private void ConfigurarSolicitanteGestor()
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Gestor ONG", "123.456.789-00",
            "gestor@ong.com", Perfil.GESTOR_ONG.ToString(), "ACTIVE");
        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    // ── Casos de sucesso ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DeveRetornarSucesso_QuandoCampanhaAtivaESolicitanteValido()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new CriarDoacaoCommand(campanha.Guid, 250m);
        ConfigurarSolicitanteDoador();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        _messageServiceMock
            .Setup(m => m.SendDonationCreatedEventMessage(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.GuidCampanha.Should().Be(campanha.Guid);
        result.Value.Valor.Should().Be(250m);
    }

    [Fact]
    public async Task HandleAsync_DeveEnviarEventoDeDoacao_QuandoCampanhaAtivaESolicitanteValido()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new CriarDoacaoCommand(campanha.Guid, 100m);
        ConfigurarSolicitanteDoador();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        _messageServiceMock
            .Setup(m => m.SendDonationCreatedEventMessage(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _messageServiceMock.Verify(m => m.SendDonationCreatedEventMessage(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
            campanha.Guid, campanha.Titulo.Valor, It.IsAny<string>(), It.IsAny<decimal>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DeveIncrementarMetrica_QuandoDoacaoRealizada()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new CriarDoacaoCommand(campanha.Guid, 50m);
        ConfigurarSolicitanteDoador();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        _messageServiceMock
            .Setup(m => m.SendDonationCreatedEventMessage(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _metricsMock.Verify(m => m.IncrementarIntencaoDoacao(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DeveRetornarDadosDoSolicitante_NaResponse()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new CriarDoacaoCommand(campanha.Guid, 300m);
        ConfigurarSolicitanteDoador("ana@email.com", "Ana Silva", "999.888.777-66");

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        _messageServiceMock
            .Setup(m => m.SendDonationCreatedEventMessage(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Value!.NomeUsuario.Should().Be("Ana Silva");
        result.Value.EmailUsuario.Should().Be("ana@email.com");
        result.Value.CpfUsuario.Should().Be("999.888.777-66");
    }

    // ── Casos de falha ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCommandNulo()
    {
        // Act
        var acao = async () => await _handler.HandleAsync(null!);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "400_COMMAND_INVALID");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoSolicitanteNulo()
    {
        // Arrange
        _userContextMock.Setup(u => u.GetUser()).Returns((SystemUser)null!);
        var command = new CriarDoacaoCommand(Guid.NewGuid(), 100m);

        // Act
        var acao = async () => await _handler.HandleAsync(command);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "400_REQUESTER_REQUIRED");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaNaoEncontrada()
    {
        // Arrange
        ConfigurarSolicitanteDoador();
        var command = new CriarDoacaoCommand(Guid.NewGuid(), 100m);

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campanha?)null);

        // Act
        var acao = async () => await _handler.HandleAsync(command);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "400_CAMPAIGN_DOES_NOT_EXIST");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaCancelada()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        campanha.CancelarCampanha("gestor@ong.com");
        var command = new CriarDoacaoCommand(campanha.Guid, 100m);
        ConfigurarSolicitanteDoador();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        var acao = async () => await _handler.HandleAsync(command);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "403_CAMPAIGN_DOES_NOT_ACCEPT_DONATION");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaConcluida()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        campanha.ConcluirCampanha("gestor@ong.com");
        var command = new CriarDoacaoCommand(campanha.Guid, 100m);
        ConfigurarSolicitanteDoador();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        var acao = async () => await _handler.HandleAsync(command);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "403_CAMPAIGN_DOES_NOT_ACCEPT_DONATION");
    }
}