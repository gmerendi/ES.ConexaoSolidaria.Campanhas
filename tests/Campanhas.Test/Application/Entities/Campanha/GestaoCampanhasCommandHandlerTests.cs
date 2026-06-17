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

public class CriarCampanhaCommandHandlerTests
{
    private readonly Mock<ICampanhaRepository> _repositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IBaseLogger<CriarCampanhaCommandHandler>> _loggerMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IMetricsService> _metricsMock;
    private readonly Mock<IElasticSearchService> _elasticSearchMock;

    private readonly CriarCampanhaCommandHandler _handler;

    public CriarCampanhaCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICampanhaRepository>();
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<IBaseLogger<CriarCampanhaCommandHandler>>();
        _messageServiceMock = new Mock<IMessageService>();
        _metricsMock = new Mock<IMetricsService>();
        _elasticSearchMock = new Mock<IElasticSearchService>();

        _handler = new CriarCampanhaCommandHandler(
            _repositoryMock.Object,
            _userContextMock.Object,
            _loggerMock.Object,
            _messageServiceMock.Object,
            _metricsMock.Object,
            _elasticSearchMock.Object);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static CriarCampanhaCommand ComandoValido(string titulo = "Campanha de Doação Valida")
        => new(
            Titulo: titulo,
            Descricao: "Descrição válida para testes de criação de campanha",
            MetaFinanceira: 5000m,
            DataInicio: DateTime.UtcNow.AddDays(1),
            DataFim: DateTime.UtcNow.AddDays(30));

    private void ConfigurarSolicitanteGestor(string email = "gestor@ong.com")
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Gestor ONG", "123.456.789-00",
            email, Perfil.GESTOR_ONG.ToString(), "ACTIVE");

        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    private void ConfigurarSolicitanteDoador(string email = "doador@email.com")
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Doador", "111.222.333-44",
            email, Perfil.DOADOR.ToString(), "ACTIVE");

        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    // ── Casos de sucesso ─────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DeveRetornarSucesso_QuandoCommandValido()
    {
        // Arrange
        var command = ComandoValido();
        ConfigurarSolicitanteGestor();

        _repositoryMock
            .Setup(r => r.ObterPorTituloAsync(command.Titulo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campanha?)null);

        _repositoryMock
            .Setup(r => r.CadastrarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _elasticSearchMock
            .Setup(e => e.IndexAsync(It.IsAny<CampanhaDTO>()))
            .Returns(Task.CompletedTask);


        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Titulo.Should().Be(command.Titulo);
        result.Value.StatusCampanha.Should().Be(CampanhaStatus.ATIVA.ToString());
    }

    [Fact]
    public async Task HandleAsync_DeveChamarCadastrar_QuandoCommandValido()
    {
        // Arrange
        var command = ComandoValido();
        ConfigurarSolicitanteGestor();

        _repositoryMock
            .Setup(r => r.ObterPorTituloAsync(command.Titulo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campanha?)null);
        _repositoryMock
            .Setup(r => r.CadastrarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _elasticSearchMock.Setup(e => e.IndexAsync(It.IsAny<CampanhaDTO>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.CadastrarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DeveIncrementarMetrica_QuandoCampanhaCriada()
    {
        // Arrange
        var command = ComandoValido();
        ConfigurarSolicitanteGestor();

        _repositoryMock.Setup(r => r.ObterPorTituloAsync(command.Titulo, It.IsAny<CancellationToken>())).ReturnsAsync((Campanha?)null);
        _repositoryMock.Setup(r => r.CadastrarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _elasticSearchMock.Setup(e => e.IndexAsync(It.IsAny<CampanhaDTO>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _metricsMock.Verify(m => m.IncrementarCampanhaCriada(), Times.Once);
    }

    // ── Casos de falha ───────────────────────────────────────────────────────

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
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaJaExiste()
    {
        // Arrange
        var command = ComandoValido();
        ConfigurarSolicitanteGestor();

        var campanhaExistente = new Campanha(
            TituloCampanha.Create(command.Titulo),
            "descrição existente",
            MetaFinanceira.Create(1000m),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            "outro@ong.com");

        _repositoryMock
            .Setup(r => r.ObterPorTituloAsync(command.Titulo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanhaExistente);

        // Act
        var acao = async () => await _handler.HandleAsync(command);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "422_CAMPAIGN_DUPLICATED");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoSolicitanteNaoEGestor()
    {
        // Arrange
        var command = ComandoValido();
        ConfigurarSolicitanteDoador();

        _repositoryMock
            .Setup(r => r.ObterPorTituloAsync(command.Titulo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campanha?)null);

        // Act
        var acao = async () => await _handler.HandleAsync(command);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "403_USER_NOT_ALLOWED");
    }
}




public class AlterarCampanhaCommandHandlerTests
{
    private readonly Mock<ICampanhaRepository> _repositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IBaseLogger<AlterarCampanhaCommandHandler>> _loggerMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IMetricsService> _metricsMock;
    private readonly Mock<IElasticSearchService> _elasticSearchMock;

    private readonly AlterarCampanhaCommandHandler _handler;

    public AlterarCampanhaCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICampanhaRepository>();
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<IBaseLogger<AlterarCampanhaCommandHandler>>();
        _cacheServiceMock = new Mock<ICacheService>();
        _elasticSearchMock = new Mock<IElasticSearchService>();

        _handler = new AlterarCampanhaCommandHandler(
            _repositoryMock.Object,
            _userContextMock.Object,
            _loggerMock.Object,
            _cacheServiceMock.Object,
            _elasticSearchMock.Object);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Campanha CriarCampanhaAtiva()
        => new(
            TituloCampanha.Create("Campanha Original para Testes"),
            "Descrição original da campanha",
            MetaFinanceira.Create(8000m),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            "gestor@ong.com");

    private static AlterarCampanhaCommand ComandoValido(Guid? guid = null)
        => new(
            Guid: guid ?? Guid.NewGuid(),
            Titulo: "Titulo Alterado para Testes",
            Descricao: "Descrição alterada para a campanha",
            MetaFinanceira: 15000m,
            DataInicio: DateTime.UtcNow.AddDays(2),
            DataFim: DateTime.UtcNow.AddDays(60));

    private void ConfigurarSolicitanteGestor(string email = "gestor@ong.com")
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Gestor ONG", "123.456.789-00",
            email, Perfil.GESTOR_ONG.ToString(), "ACTIVE");
        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    private void ConfigurarSolicitanteDoador()
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Doador", "111.222.333-44",
            "doador@email.com", Perfil.DOADOR.ToString(), "ACTIVE");
        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    // ── Casos de sucesso ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DeveRetornarSucesso_QuandoCommandValido()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = ComandoValido(campanha.Guid);
        ConfigurarSolicitanteGestor();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);
        _repositoryMock
            .Setup(r => r.AlterarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _elasticSearchMock.Setup(e => e.UpdateAsync(It.IsAny<CampanhaDTO>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Titulo.Should().Be(command.Titulo);
    }

    [Fact]
    public async Task HandleAsync_DeveChamarAlterarAsync_QuandoCommandValido()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = ComandoValido(campanha.Guid);
        ConfigurarSolicitanteGestor();

        _repositoryMock.Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>())).ReturnsAsync(campanha);
        _repositoryMock.Setup(r => r.AlterarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _elasticSearchMock.Setup(e => e.UpdateAsync(It.IsAny<CampanhaDTO>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _repositoryMock.Verify(r => r.AlterarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task HandleAsync_DeveLancarDomainException_QuandoSolicitanteNaoEGestor()
    {
        // Arrange
        ConfigurarSolicitanteDoador();
        var command = ComandoValido();

        // Act
        var acao = async () => await _handler.HandleAsync(command);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "403_CAMPAIGN_CAN_BE_CHANGED_BY_MANAGER");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaNaoEncontrada()
    {
        // Arrange
        ConfigurarSolicitanteGestor();
        var command = ComandoValido();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campanha?)null);

        // Act
        var acao = async () => await _handler.HandleAsync(command);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "400_CAMPAIGN_DOES_NOT_EXIST");
    }
}




public class CancelarCampanhaCommandHandlerTests
{
    private readonly Mock<ICampanhaRepository> _repositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IBaseLogger<CancelarCampanhaCommandHandler>> _loggerMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IMetricsService> _metricsMock;
    private readonly Mock<IElasticSearchService> _elasticSearchMock;

    private readonly CancelarCampanhaCommandHandler _handler;

    public CancelarCampanhaCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICampanhaRepository>();
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<IBaseLogger<CancelarCampanhaCommandHandler>>();
        _cacheServiceMock = new Mock<ICacheService>();
        _metricsMock = new Mock<IMetricsService>();
        _elasticSearchMock = new Mock<IElasticSearchService>();

        _handler = new CancelarCampanhaCommandHandler(
            _repositoryMock.Object,
            _userContextMock.Object,
            _loggerMock.Object,
            _cacheServiceMock.Object,
            _metricsMock.Object,
            _elasticSearchMock.Object);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Campanha CriarCampanhaAtiva()
        => new(
            TituloCampanha.Create("Campanha para Cancelamento"),
            "Descrição da campanha que será cancelada",
            MetaFinanceira.Create(3000m),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            "gestor@ong.com");

    private void ConfigurarSolicitanteGestor(string email = "gestor@ong.com")
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Gestor ONG", "123.456.789-00",
            email, Perfil.GESTOR_ONG.ToString(), "ACTIVE");
        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    private void ConfigurarSolicitanteDoador()
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Doador", "111.222.333-44",
            "doador@email.com", Perfil.DOADOR.ToString(), "ACTIVE");
        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    private void ConfigurarMocksSecundarios()
    {
        _repositoryMock.Setup(r => r.AlterarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _elasticSearchMock.Setup(e => e.UpdateAsync(It.IsAny<CampanhaDTO>())).Returns(Task.CompletedTask);
    }

    // ── Casos de sucesso ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DeveRetornarSucesso_QuandoCampanhaAtiva()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new CancelarCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();
        ConfigurarMocksSecundarios();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_DeveAlterarStatusParaCancelada_QuandoCampanhaAtiva()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new CancelarCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();
        ConfigurarMocksSecundarios();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        campanha.StatusCampanha.Should().Be(CampanhaStatus.CANCELADA);
    }

    [Fact]
    public async Task HandleAsync_DeveIncrementarMetrica_QuandoCampanhaCancelada()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new CancelarCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();
        ConfigurarMocksSecundarios();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _metricsMock.Verify(m => m.IncrementarCampanhaCancelada(), Times.Once);
    }

    // ── Casos de falha ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCommandNulo()
    {
        // Act
        var acao = async () => await _handler.HandleAsync(null!, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "400_COMMAND_INVALID");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoSolicitanteNaoEGestor()
    {
        // Arrange
        ConfigurarSolicitanteDoador();
        var command = new CancelarCampanhaCommand(Guid.NewGuid());

        // Act
        var acao = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "403_USER_NOT_ALLOWED");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaNaoEncontrada()
    {
        // Arrange
        ConfigurarSolicitanteGestor();
        var command = new CancelarCampanhaCommand(Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campanha?)null);

        // Act
        var acao = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "400_CAMPAIGN_NOT_FOUND");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaJaCancelada()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        campanha.CancelarCampanha("gestor@ong.com"); // já cancelada
        var command = new CancelarCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        var acao = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert — o handler verifica StatusCampanha != ATIVA antes de chamar CancelarCampanha
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "422_CAMPAIGN_NOT_ACTIVE");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaConcluida()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        campanha.ConcluirCampanha("gestor@ong.com"); // concluída
        var command = new CancelarCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        var acao = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "422_CAMPAIGN_NOT_ACTIVE");
    }
}




public class ConcluirCampanhaCommandHandlerTests
{
    private readonly Mock<ICampanhaRepository> _repositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IBaseLogger<ConcluirCampanhaCommandHandler>> _loggerMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IMetricsService> _metricsMock;
    private readonly Mock<IElasticSearchService> _elasticSearchMock;

    private readonly ConcluirCampanhaCommandHandler _handler;

    public ConcluirCampanhaCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICampanhaRepository>();
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<IBaseLogger<ConcluirCampanhaCommandHandler>>();
        _cacheServiceMock = new Mock<ICacheService>();
        _metricsMock = new Mock<IMetricsService>();
        _elasticSearchMock = new Mock<IElasticSearchService>();

        _handler = new ConcluirCampanhaCommandHandler(
            _repositoryMock.Object,
            _userContextMock.Object,
            _loggerMock.Object,
            _cacheServiceMock.Object,
            _metricsMock.Object,
            _elasticSearchMock.Object);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Campanha CriarCampanhaAtiva()
        => new(
            TituloCampanha.Create("Campanha para Conclusão"),
            "Descrição da campanha que será concluída",
            MetaFinanceira.Create(12000m),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            "gestor@ong.com");

    private void ConfigurarSolicitanteGestor(string email = "gestor@ong.com")
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Gestor ONG", "123.456.789-00",
            email, Perfil.GESTOR_ONG.ToString(), "ACTIVE");
        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    private void ConfigurarSolicitanteDoador()
    {
        var usuario = new SystemUser(
            Guid.NewGuid(), "Doador", "111.222.333-44",
            "doador@email.com", Perfil.DOADOR.ToString(), "ACTIVE");
        _userContextMock.Setup(u => u.GetUser()).Returns(usuario);
    }

    private void ConfigurarMocksSecundarios()
    {
        _repositoryMock.Setup(r => r.AlterarAsync(It.IsAny<Campanha>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _elasticSearchMock.Setup(e => e.UpdateAsync(It.IsAny<CampanhaDTO>())).Returns(Task.CompletedTask);
    }

    // ── Casos de sucesso ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DeveRetornarSucesso_QuandoCampanhaAtiva()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new ConcluirCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();
        ConfigurarMocksSecundarios();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_DeveAlterarStatusParaConcluida_QuandoCampanhaAtiva()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new ConcluirCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();
        ConfigurarMocksSecundarios();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        campanha.StatusCampanha.Should().Be(CampanhaStatus.CONCLUIDA);
    }

    [Fact]
    public async Task HandleAsync_DeveIncrementarMetrica_QuandoCampanhaConcluida()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        var command = new ConcluirCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();
        ConfigurarMocksSecundarios();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _metricsMock.Verify(m => m.IncrementarCampanhaConcluida(), Times.Once);
    }

    // ── Casos de falha ────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCommandNulo()
    {
        // Act
        var acao = async () => await _handler.HandleAsync(null!, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "400_COMMAND_INVALID");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoSolicitanteNaoEGestor()
    {
        // Arrange
        ConfigurarSolicitanteDoador();
        var command = new ConcluirCampanhaCommand(Guid.NewGuid());

        // Act
        var acao = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "403_USER_NOT_ALLOWED");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaNaoEncontrada()
    {
        // Arrange
        ConfigurarSolicitanteGestor();
        var command = new ConcluirCampanhaCommand(Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campanha?)null);

        // Act
        var acao = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "400_CAMPAIGN_NOT_FOUND");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaCancelada()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        campanha.CancelarCampanha("gestor@ong.com"); // já cancelada
        var command = new ConcluirCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        var acao = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "422_CAMPAIGN_NOT_ACTIVE");
    }

    [Fact]
    public async Task HandleAsync_DeveLancarDomainException_QuandoCampanhaJaConcluida()
    {
        // Arrange
        var campanha = CriarCampanhaAtiva();
        campanha.ConcluirCampanha("gestor@ong.com"); // já concluída
        var command = new ConcluirCampanhaCommand(campanha.Guid);
        ConfigurarSolicitanteGestor();

        _repositoryMock
            .Setup(r => r.ObterPorGuidAsync(command.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campanha);

        // Act
        var acao = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "422_CAMPAIGN_NOT_ACTIVE");
    }
}