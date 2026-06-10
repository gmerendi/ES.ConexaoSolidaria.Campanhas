using Campanhas.Api.Models;
using Campanhas.Application.Common;
using Campanhas.Application.DTOs;
using Campanhas.Application.UseCases.CancelarCampanha;
using Campanhas.Application.UseCases.ConcluirCampanha;
using Campanhas.Application.UseCases.CriarCampanha;
using Campanhas.Application.UseCases.EditarCampanha;
using Campanhas.Application.UseCases.ObterCampanha;
using Campanhas.Application.UseCases.ObterCampanhasPorFiltro;
using Campanhas.Application.UseCases.ObterTodasCampanhas;
using Campanhas.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campanhas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class CampanhasController : ControllerBase
{
    private readonly IUseCaseHandler<CriarCampanhaCommand, CampanhaDto> _criarHandler;
    private readonly IUseCaseHandler<EditarCampanhaCommand, CampanhaDto> _editarHandler;
    private readonly IUseCaseHandler<CancelarCampanhaCommand, CampanhaDto> _cancelarHandler;
    private readonly IUseCaseHandler<ConcluirCampanhaCommand, CampanhaDto> _concluirHandler;
    private readonly IUseCaseHandler<ObterCampanhaQuery, CampanhaDto> _obterHandler;
    private readonly IUseCaseHandler<ObterTodasCampanhasQuery, IReadOnlyList<CampanhaDto>> _obterTodasHandler;
    private readonly IUseCaseHandler<ObterCampanhasPorFiltroQuery, IReadOnlyList<CampanhaDto>> _obterPorFiltroHandler;

    public CampanhasController(
        IUseCaseHandler<CriarCampanhaCommand, CampanhaDto> criarHandler,
        IUseCaseHandler<EditarCampanhaCommand, CampanhaDto> editarHandler,
        IUseCaseHandler<CancelarCampanhaCommand, CampanhaDto> cancelarHandler,
        IUseCaseHandler<ConcluirCampanhaCommand, CampanhaDto> concluirHandler,
        IUseCaseHandler<ObterCampanhaQuery, CampanhaDto> obterHandler,
        IUseCaseHandler<ObterTodasCampanhasQuery, IReadOnlyList<CampanhaDto>> obterTodasHandler,
        IUseCaseHandler<ObterCampanhasPorFiltroQuery, IReadOnlyList<CampanhaDto>> obterPorFiltroHandler)
    {
        _criarHandler = criarHandler;
        _editarHandler = editarHandler;
        _cancelarHandler = cancelarHandler;
        _concluirHandler = concluirHandler;
        _obterHandler = obterHandler;
        _obterTodasHandler = obterTodasHandler;
        _obterPorFiltroHandler = obterPorFiltroHandler;
    }

    /// <summary>Retorna todas as campanhas.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CampanhaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodas(CancellationToken ct)
    {
        var result = await _obterTodasHandler.HandleAsync(new ObterTodasCampanhasQuery(), ct);
        return HandleResult(result);
    }

    /// <summary>Retorna uma campanha pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CampanhaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var result = await _obterHandler.HandleAsync(new ObterCampanhaQuery(id), ct);
        return HandleResult(result);
    }

    /// <summary>Retorna campanhas filtradas por critérios opcionais.</summary>
    [HttpGet("filtro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CampanhaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorFiltro(
        [FromQuery] string? titulo,
        [FromQuery] CampanhaStatus? status,
        [FromQuery] DateTime? dataInicioMin,
        [FromQuery] DateTime? dataInicioMax,
        CancellationToken ct)
    {
        var query = new ObterCampanhasPorFiltroQuery(titulo, status, dataInicioMin, dataInicioMax);
        var result = await _obterPorFiltroHandler.HandleAsync(query, ct);
        return HandleResult(result);
    }

    /// <summary>Cria uma nova campanha.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CampanhaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar([FromBody] CriarCampanhaRequest request, CancellationToken ct)
    {
        var command = new CriarCampanhaCommand(
            request.Titulo,
            request.Descricao,
            request.MetaFinanceira,
            request.DataInicio,
            request.DataFim);

        var result = await _criarHandler.HandleAsync(command, ct);
        return HandleResult(result, StatusCodes.Status201Created);
    }

    /// <summary>Edita uma campanha existente (somente se ativa).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CampanhaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarCampanhaRequest request, CancellationToken ct)
    {
        var command = new EditarCampanhaCommand(
            id,
            request.Titulo,
            request.Descricao,
            request.MetaFinanceira,
            request.DataInicio,
            request.DataFim);

        var result = await _editarHandler.HandleAsync(command, ct);
        return HandleResult(result);
    }

    /// <summary>Cancela uma campanha ativa.</summary>
    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(CampanhaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
    {
        var result = await _cancelarHandler.HandleAsync(new CancelarCampanhaCommand(id), ct);
        return HandleResult(result);
    }

    /// <summary>Conclui uma campanha ativa.</summary>
    [HttpPost("{id:guid}/concluir")]
    [ProducesResponseType(typeof(CampanhaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Concluir(Guid id, CancellationToken ct)
    {
        var result = await _concluirHandler.HandleAsync(new ConcluirCampanhaCommand(id), ct);
        return HandleResult(result);
    }

    private IActionResult HandleResult<T>(Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
            return StatusCode(successStatusCode, result.Data);

        var correlationId = HttpContext.Items["x-correlation-id"]?.ToString() ?? Guid.NewGuid().ToString();

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(new
            {
                codigo = "404_CAMPAIGN_NOT_FOUND",
                mensagem = result.ErrorMessage,
                correlationId
            }),
            ResultErrorType.DomainError => UnprocessableEntity(new
            {
                codigo = "422_CAMPAIGN_DOMAIN_ERROR",
                mensagem = result.ErrorMessage,
                correlationId
            }),
            ResultErrorType.ValidationError => BadRequest(new
            {
                codigo = "400_CAMPAIGN_VALIDATION_ERROR",
                mensagem = result.ErrorMessage,
                correlationId
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new
            {
                codigo = "500_INTERNAL_ERROR",
                mensagem = "Erro interno do servidor.",
                correlationId
            })
        };
    }
}
