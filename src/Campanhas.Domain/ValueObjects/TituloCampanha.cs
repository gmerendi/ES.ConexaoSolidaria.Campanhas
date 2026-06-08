using Campanhas.Domain.Exceptions;

namespace Campanhas.Domain.ValueObjects;

public sealed class TituloCampanha
{
    public const int TamanhoMinimo = 5;
    public const int TamanhoMaximo = 200;

    public string Valor { get; private set; } = null!;

    private TituloCampanha() { }

    private TituloCampanha(string valor) => Valor = valor;

    public static TituloCampanha Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("Título é obrigatório.");

        var valorTrimado = valor.Trim();

        if (valorTrimado.Length < TamanhoMinimo)
            throw new DomainException($"Título deve ter no mínimo {TamanhoMinimo} caracteres.");

        if (valorTrimado.Length > TamanhoMaximo)
            throw new DomainException($"Título deve ter no máximo {TamanhoMaximo} caracteres.");

        return new TituloCampanha(valorTrimado);
    }

    public static implicit operator string(TituloCampanha titulo) => titulo.Valor;
    public override string ToString() => Valor;
}
