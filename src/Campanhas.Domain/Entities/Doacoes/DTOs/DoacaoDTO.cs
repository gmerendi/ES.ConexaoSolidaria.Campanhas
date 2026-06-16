using System.Diagnostics.CodeAnalysis;

namespace Campanhas.Domain.Entities.Doacoes;

public sealed class DoacaoDTO
{
    public Guid GuidUsuario { get; init; }
    public string NomeUsuario { get; init; } = String.Empty;
    public string EmailUsuario { get; init; } = String.Empty;
    public string CpfUsuario { get; init; } = String.Empty;
    public Guid GuidCampanha { get; init; }
    public string TituloCampanha { get; init; } = String.Empty;
    public decimal ValorDoacao { get; init; } 
    public string DataDoacao { get; init; } = String.Empty;

    private DoacaoDTO() { }

    [SetsRequiredMembers]
    public DoacaoDTO(Guid guidUsuario, string nomeUsuario, string emailUsuario, string cpfUsuario,
        Guid guidCampanha, string tituloCampanha, decimal valorDoacao, string dataDoacao)
    {
        GuidUsuario = guidUsuario;
        NomeUsuario = nomeUsuario;
        EmailUsuario = emailUsuario;
        CpfUsuario = cpfUsuario;
        GuidCampanha = guidCampanha;
        TituloCampanha = tituloCampanha;
        ValorDoacao = valorDoacao;
        DataDoacao = dataDoacao;
    }
}