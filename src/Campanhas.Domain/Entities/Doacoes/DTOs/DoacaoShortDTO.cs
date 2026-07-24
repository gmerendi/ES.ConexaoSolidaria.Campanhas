using System.Diagnostics.CodeAnalysis;

namespace Campanhas.Domain.Entities.Doacoes;

public sealed class DoacaoShortDTO
{
    public Guid GuidCampanha { get; init; }
    public string TituloCampanha { get; init; } = String.Empty;
    public decimal ValorDoacao { get; init; } 
    public string DataDoacao { get; init; } = String.Empty;
    public string StatusDoacao { get; init; } = String.Empty;

    private DoacaoShortDTO() { }

    [SetsRequiredMembers]
    public DoacaoShortDTO(Guid guidCampanha, string tituloCampanha, decimal valorDoacao, string dataDoacao, 
        string statusDoacao)
    {
        GuidCampanha = guidCampanha;
        TituloCampanha = tituloCampanha;
        ValorDoacao = valorDoacao;
        DataDoacao = dataDoacao;
        StatusDoacao = statusDoacao;
    }
}