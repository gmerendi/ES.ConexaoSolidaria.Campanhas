namespace Campanhas.Application.Features.Campanhas;

public record ObterTodasCampanhasQuery(
        int Pagina = 1,
        int TamanhoPagina = 9999
    );
