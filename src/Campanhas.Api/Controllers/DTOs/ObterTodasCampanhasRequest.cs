namespace Campanhas.Api.Controllers.DTOs
{
    public record ObterTodasCampanhasRequest(
        int Pagina = 1,
        int TamanhoPagina = 9999
   );
}
