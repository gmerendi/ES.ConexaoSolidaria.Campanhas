using Campanhas.Domain.Exceptions;

namespace Campanhas.Domain.ValueObjects;

public sealed class MetaFinanceira
{
    public decimal Valor { get; private set; }

    private MetaFinanceira() { }

    private MetaFinanceira(decimal valor) => Valor = valor;

    public static MetaFinanceira Criar(decimal valor)
    {
        if (valor <= 0)
            throw new DomainException("Meta financeira deve ser maior que zero.");

        return new MetaFinanceira(valor);
    }

    public static implicit operator decimal(MetaFinanceira meta) => meta.Valor;
    public override string ToString() => Valor.ToString("C2");
}
