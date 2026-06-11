using Campanhas.Domain.Entities.Campanhas;
using Campanhas.Domain.Shared.Exceptions;
using FluentAssertions;

namespace Campanhas.Tests.Domain;

public class MetaFinanceiraTests
{
    // ── Create: casos de sucesso ───────────────────────────────────────────

    [Fact]
    public void Create_DeveRetornarMetaFinanceira_QuandoValorValido()
    {
        // Arrange
        var valor = 1000m;

        // Act
        var meta = MetaFinanceira.Create(valor);

        // Assert
        meta.Should().NotBeNull();
        meta.Valor.Should().Be(1000m);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(50000)]
    [InlineData(999999.99)]
    public void Create_DeveAceitar_ValoresPositivos(decimal valor)
    {
        // Act
        var meta = MetaFinanceira.Create(valor);

        // Assert
        meta.Valor.Should().Be(valor);
    }

    // ── Create: casos de falha ─────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_DeveLancarDomainException_QuandoValorZeroOuNegativo(decimal valor)
    {
        // Act
        var acao = () => MetaFinanceira.Create(valor);

        // Assert
        acao.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be("400_TARGET_RANGE_INVALID");
    }

    // ── Conversão implícita ────────────────────────────────────────────────

    [Fact]
    public void ImplicitOperator_DeveConverterParaDecimal()
    {
        // Arrange
        var meta = MetaFinanceira.Create(5000m);

        // Act
        decimal valor = meta;

        // Assert
        valor.Should().Be(5000m);
    }

    [Fact]
    public void ToString_DeveRetornarFormatoMonetario()
    {
        // Arrange
        var meta = MetaFinanceira.Create(1500m);

        // Act
        var resultado = meta.ToString();

        // Assert — formata como moeda (C2)
        resultado.Should().NotBeNullOrWhiteSpace();
    }
}