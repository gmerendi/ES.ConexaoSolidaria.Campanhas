using Campanhas.Domain.Entities.ValueObjects;
using Campanhas.Domain.Shared.Exceptions;
using FluentAssertions;

namespace Campanhas.Tests.Domain;

public class TituloCampanhaTests
{
    // ── Create: casos de sucesso ───────────────────────────────────────────

    [Fact]
    public void Create_DeveRetornarTitulo_QuandoValorValido()
    {
        // Arrange
        var valor = "Campanha de Doação de Roupas";

        // Act
        var titulo = TituloCampanha.Create(valor);

        // Assert
        titulo.Should().NotBeNull();
        titulo.Valor.Should().Be(valor);
    }

    [Fact]
    public void Create_DeveAparar_EspacosNasBordas()
    {
        // Arrange
        var valor = "   Campanha de Alimentos   ";

        // Act
        var titulo = TituloCampanha.Create(valor);

        // Assert
        titulo.Valor.Should().Be("Campanha de Alimentos");
    }

    [Fact]
    public void Create_DeveAceitar_TituloComTamanhoMinimo()
    {
        // Arrange — exatamente 5 caracteres
        var valor = "Cinco";

        // Act
        var titulo = TituloCampanha.Create(valor);

        // Assert
        titulo.Valor.Should().Be("Cinco");
    }

    [Fact]
    public void Create_DeveAceitar_TituloComTamanhoMaximo()
    {
        // Arrange — exatamente 200 caracteres
        var valor = new string('A', 200);

        // Act
        var titulo = TituloCampanha.Create(valor);

        // Assert
        titulo.Valor.Length.Should().Be(200);
    }

    // ── Create: casos de falha ─────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_DeveLancarDomainException_QuandoValorVazioOuNulo(string? valor)
    {
        // Act
        var acao = () => TituloCampanha.Create(valor!);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("400_TITLE_REQUIRED");
    }

    [Fact]
    public void Create_DeveLancarDomainException_QuandoTituloMuitoCurto()
    {
        // Arrange — 4 caracteres, abaixo do mínimo de 5
        var valor = "Abcd";

        // Act
        var acao = () => TituloCampanha.Create(valor);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("400_TITLE_LENGTH_INVALID");
    }

    [Fact]
    public void Create_DeveLancarDomainException_QuandoTituloMuitoLongo()
    {
        // Arrange — 201 caracteres, acima do máximo de 200
        var valor = new string('A', 201);

        // Act
        var acao = () => TituloCampanha.Create(valor);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("400_TITLE_LENGTH_INVALID");
    }

    // ── Conversão implícita ────────────────────────────────────────────────

    [Fact]
    public void ImplicitOperator_DeveConverterParaString()
    {
        // Arrange
        var titulo = TituloCampanha.Create("Campanha de Natal");

        // Act
        string valor = titulo;

        // Assert
        valor.Should().Be("Campanha de Natal");
    }

    [Fact]
    public void ToString_DeveRetornarValor()
    {
        // Arrange
        var titulo = TituloCampanha.Create("Campanha de Natal");

        // Act & Assert
        titulo.ToString().Should().Be("Campanha de Natal");
    }
}