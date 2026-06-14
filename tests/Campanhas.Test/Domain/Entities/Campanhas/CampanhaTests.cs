using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Entities.Campanhas.Enums;
using Campanhas.Domain.Entities.ValueObjects;
using Campanhas.Domain.Shared.Exceptions;
using FluentAssertions;

namespace Campanhas.Tests.Domain;

public class CampanhaTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static TituloCampanha CriarTitulo(string valor = "Campanha de Testes Válida")
        => TituloCampanha.Create(valor);

    private static MetaFinanceira CriarMeta(decimal valor = 10000m)
        => MetaFinanceira.Create(valor);

    private static Campanha CriarCampanha(
        string titulo = "Campanha de Testes Válida",
        string descricao = "Descrição válida para a campanha de testes",
        decimal meta = 10000m,
        DateTime? inicio = null,
        DateTime? fim = null,
        string criadoPor = "gestor@ong.com")
    {
        var dataInicio = inicio ?? DateTime.UtcNow.AddDays(1);
        var dataFim = fim ?? DateTime.UtcNow.AddDays(30);

        return new Campanha(
            CriarTitulo(titulo),
            descricao,
            CriarMeta(meta),
            dataInicio,
            dataFim,
            criadoPor);
    }

    // ── Construtor: casos de sucesso ───────────────────────────────────────

    [Fact]
    public void Construtor_DeveCriarCampanha_ComStatusAtiva()
    {
        // Act
        var campanha = CriarCampanha();

        // Assert
        campanha.StatusCampanha.Should().Be(CampanhaStatus.ATIVA);
    }

    [Fact]
    public void Construtor_DeveCriarCampanha_ComValorArrecadadoZero()
    {
        // Act
        var campanha = CriarCampanha();

        // Assert
        campanha.ValorArrecadado.Should().Be(0);
    }

    [Fact]
    public void Construtor_DeveCriarCampanha_ComDadosCorretos()
    {
        // Arrange
        var inicio = DateTime.UtcNow.AddDays(1);
        var fim = DateTime.UtcNow.AddDays(30);

        // Act
        var campanha = CriarCampanha(
            titulo: "Campanha de Alimentos",
            descricao: "Arrecadação de alimentos não perecíveis",
            meta: 5000m,
            inicio: inicio,
            fim: fim,
            criadoPor: "gestor@ong.org");

        // Assert
        campanha.Titulo.Valor.Should().Be("Campanha de Alimentos");
        campanha.Descricao.Should().Be("Arrecadação de alimentos não perecíveis");
        campanha.MetaFinanceira.Valor.Should().Be(5000m);
        campanha.DataInicio.Should().BeCloseTo(inicio, TimeSpan.FromSeconds(1));
        campanha.DataFim.Should().BeCloseTo(fim, TimeSpan.FromSeconds(1));
        campanha.CriadoPor.Should().Be("gestor@ong.org");
    }

    // ── Construtor: casos de falha ─────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarDomainException_QuandoDescricaoVazia(string descricao)
    {
        // Act
        var acao = () => CriarCampanha(descricao: descricao);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("400_DESCRIPTION_REQUIRED");
    }

    [Fact]
    public void Construtor_DeveLancarDomainException_QuandoDescricaoMuitoLonga()
    {
        // Arrange — 2001 caracteres, acima do limite de 2000
        var descricao = new string('D', 2001);

        // Act
        var acao = () => CriarCampanha(descricao: descricao);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("400_DESCRIPTION_LENGTH_INVALID");
    }

    [Fact]
    public void Construtor_DeveLancarDomainException_QuandoDataFimAnteriorADataInicio()
    {
        // Arrange
        var inicio = DateTime.UtcNow.AddDays(10);
        var fim = DateTime.UtcNow.AddDays(5); // fim < início

        // Act
        var acao = () => CriarCampanha(inicio: inicio, fim: fim);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("422_DATES_MISMATCHING");
    }

    [Fact]
    public void Construtor_DeveLancarDomainException_QuandoDataFimIgualDataInicio()
    {
        // Arrange
        var data = DateTime.UtcNow.AddDays(5);

        // Act
        var acao = () => CriarCampanha(inicio: data, fim: data);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("422_DATES_MISMATCHING");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarDomainException_QuandoCriadoPorVazio(string criadoPor)
    {
        // Act
        var acao = () => CriarCampanha(criadoPor: criadoPor);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("400_REQUESTER_REQUIRED");
    }

    // ── AlterarCampanha ────────────────────────────────────────────────────

    [Fact]
    public void AlterarCampanha_DeveAtualizarDados_QuandoCampanhaAtiva()
    {
        // Arrange
        var campanha = CriarCampanha();
        var novoTitulo = TituloCampanha.Create("Titulo Alterado Valido");
        var novoInicio = DateTime.UtcNow.AddDays(2);
        var novoFim = DateTime.UtcNow.AddDays(60);
        var novaMeta = MetaFinanceira.Create(20000m);

        // Act
        campanha.AlterarCampanha(novoTitulo, "Nova descrição válida", novaMeta, novoInicio, novoFim, "editor@ong.com");

        // Assert
        campanha.Titulo.Valor.Should().Be("Titulo Alterado Valido");
        campanha.Descricao.Should().Be("Nova descrição válida");
        campanha.MetaFinanceira.Valor.Should().Be(20000m);
        campanha.ModificadoPor.Should().Be("editor@ong.com");
    }

    [Fact]
    public void AlterarCampanha_DeveLancarDomainException_QuandoCampanhaCancelada()
    {
        // Arrange
        var campanha = CriarCampanha();
        campanha.CancelarCampanha("gestor@ong.com");

        // Act
        var acao = () => campanha.AlterarCampanha(
            CriarTitulo("Novo Titulo"),
            "Nova descrição",
            CriarMeta(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            "gestor@ong.com");

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("422_CAMPAIGN_ACTIVE_CAN_BE_EDITED");
    }

    [Fact]
    public void AlterarCampanha_DeveLancarDomainException_QuandoCampanhaConcluida()
    {
        // Arrange
        var campanha = CriarCampanha();
        campanha.ConcluirCampanha("gestor@ong.com");

        // Act
        var acao = () => campanha.AlterarCampanha(
            CriarTitulo("Novo Titulo"),
            "Nova descrição",
            CriarMeta(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            "gestor@ong.com");

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("422_CAMPAIGN_ACTIVE_CAN_BE_EDITED");
    }

    // ── CancelarCampanha ───────────────────────────────────────────────────

    [Fact]
    public void CancelarCampanha_DeveAlterarStatus_QuandoCampanhaAtiva()
    {
        // Arrange
        var campanha = CriarCampanha();

        // Act
        campanha.CancelarCampanha("gestor@ong.com");

        // Assert
        campanha.StatusCampanha.Should().Be(CampanhaStatus.CANCELADA);
        campanha.ModificadoPor.Should().Be("gestor@ong.com");
    }

    [Fact]
    public void CancelarCampanha_DeveLancarDomainException_QuandoJaCancelada()
    {
        // Arrange
        var campanha = CriarCampanha();
        campanha.CancelarCampanha("gestor@ong.com");

        // Act
        var acao = () => campanha.CancelarCampanha("gestor@ong.com");

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("422_CAMPAIGN_ALREADY_CANCELLED");
    }

    [Fact]
    public void CancelarCampanha_DeveLancarDomainException_QuandoCampanhaConcluida()
    {
        // Arrange
        var campanha = CriarCampanha();
        campanha.ConcluirCampanha("gestor@ong.com");

        // Act
        var acao = () => campanha.CancelarCampanha("gestor@ong.com");

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("422_CAMPAIGN_FINISHED_CANNOT_CANCEL");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CancelarCampanha_DeveLancarDomainException_QuandoModificadoPorVazio(string modificadoPor)
    {
        // Arrange
        var campanha = CriarCampanha();

        // Act
        var acao = () => campanha.CancelarCampanha(modificadoPor);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("400_REQUESTER_REQUIRED");
    }

    // ── ConcluirCampanha ───────────────────────────────────────────────────

    [Fact]
    public void ConcluirCampanha_DeveAlterarStatus_QuandoCampanhaAtiva()
    {
        // Arrange
        var campanha = CriarCampanha();

        // Act
        campanha.ConcluirCampanha("gestor@ong.com");

        // Assert
        campanha.StatusCampanha.Should().Be(CampanhaStatus.CONCLUIDA);
        campanha.ModificadoPor.Should().Be("gestor@ong.com");
    }

    [Fact]
    public void ConcluirCampanha_DeveLancarDomainException_QuandoCampanhaCancelada()
    {
        // Arrange
        var campanha = CriarCampanha();
        campanha.CancelarCampanha("gestor@ong.com");

        // Act
        var acao = () => campanha.ConcluirCampanha("gestor@ong.com");

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("422_CAMPAIGN_ACTIVE_CAN_BE_FINISHED");
    }

    [Fact]
    public void ConcluirCampanha_DeveLancarDomainException_QuandoJaConcluida()
    {
        // Arrange
        var campanha = CriarCampanha();
        campanha.ConcluirCampanha("gestor@ong.com");

        // Act
        var acao = () => campanha.ConcluirCampanha("gestor@ong.com");

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("422_CAMPAIGN_ACTIVE_CAN_BE_FINISHED");
    }
}