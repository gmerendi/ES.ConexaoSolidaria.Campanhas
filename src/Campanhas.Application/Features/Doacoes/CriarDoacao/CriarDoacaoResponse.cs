namespace Campanhas.Application.Features.Campanhas
{
    public record CriarDoacaoResponse(
        Guid GuidCampanha,
        string TituloCampanha,
        string NomeUsuario,
        string EmailUsuario,
        string CpfUsuario,
        decimal Valor
    );
}
