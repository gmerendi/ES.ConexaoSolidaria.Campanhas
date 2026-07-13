using Campanhas.Api.Controllers.DTOs;
using Campanhas.Application.Features.Campanhas;
using Campanhas.Application.Shared;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Shared.Interfaces;
using Campanhas.Domain.Shared.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campanhas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class DoacoesController : ControllerBase
{
    private readonly IBaseLogger<DoacoesController> _logger;
    private readonly IUseCaseHandler<CriarDoacaoCommand, Result<CriarDoacaoResponse>> _criarDoacaoCommandHandler;
    private readonly IUseCaseHandler<ObterDoacoesPorCampanhaQuery, Result<ObterDoacoesPorCampanhaResponse>> _obterDoacoesPorCampanhaQueryHandler;
    private readonly IUseCaseHandler<ObterDoacoesPorUsuarioQuery, Result<ObterDoacoesPorUsuarioResponse>> _obterDoacoesPorUsuarioQueryHandler;


    public DoacoesController(IBaseLogger<DoacoesController> logger,
        IUseCaseHandler<CriarDoacaoCommand, Result<CriarDoacaoResponse>> criarDoacaoCommandHandler,
        IUseCaseHandler<ObterDoacoesPorCampanhaQuery, Result<ObterDoacoesPorCampanhaResponse>> obterDoacoesPorCampanhaQueryHandler,
        IUseCaseHandler<ObterDoacoesPorUsuarioQuery, Result<ObterDoacoesPorUsuarioResponse>> obterDoacoesPorUsuarioQueryHandler)
    {
        _logger = logger;
        _criarDoacaoCommandHandler = criarDoacaoCommandHandler;
        _obterDoacoesPorCampanhaQueryHandler = obterDoacoesPorCampanhaQueryHandler;
        _obterDoacoesPorUsuarioQueryHandler = obterDoacoesPorUsuarioQueryHandler;
    }





    /// <summary>
    /// UC-19 - Realizar doacao
    /// </summary>
    /// <remarks>   
    /// 
    /// Cria uma nova intencao de doacao a ser processada pelo worker.
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    /// **Regras de Validação:**
    /// 
    /// * **GuidCampanha:**
    ///   - `O campo GuidCampanha é obrigatório.`
    /// * **Valor:** 
    ///   - `O campo Valor é obrigatório.`
    ///   - `O valor deve ser maior que 0.`
    ///  
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>true</returns>
    /// <response code="201">Intencao de doacao criada com sucesso</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize(Roles = "GESTOR_ONG, DOADOR")]
    [HttpPost]
    [ProducesResponseType(typeof(CriarCampanhaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CriarDoacao([FromBody] CriarDoacaoRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Iniciando criação de intenção de doação para a campanha: {Guid}", BaseLogType.LOG, new { Guid = request.Guid, Valor = request.Valor });

        var command = new CriarDoacaoCommand(request.Guid, request.Valor);

        var result = await _criarDoacaoCommandHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError("Erro ao criar campanha: {ErrorCode}", BaseLogType.LOG, new { ErrorCode = result.Error });
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Intenção de doação criada com sucesso: {Guid}", BaseLogType.LOG, new { Guid = result.Value.GuidCampanha, Valor = result.Value.Valor });
        return Created("Intencao de doacao criada com sucesso", result);
    }



    
    /// <summary>
    /// UC-20 - Obter doacoes por campanha
    /// </summary>
    /// <remarks>   
    /// 
    /// Obtem todas as doacoes de uma campanha. Somente Gestor pode acessar
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    /// **Regras de Validação:**
    /// 
    /// * **Guid:**
    ///   - `O campo GuidCampanha é obrigatório.`
    ///  
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>Lista - DoacaoDTO</returns>
    /// <response code="201">Doacoes</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize(Roles = "GESTOR_ONG")]
    [HttpGet("campanha")]
    [ProducesResponseType(typeof(ObterDoacoesPorCampanhaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ObterDoacoesPorCampanha([FromQuery] ObterDoacoesPorCampanhaRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Obtendo doacoes para a campanha: {Guid}", BaseLogType.LOG, new { Guid = request.GuidCampanha });
        var command = new ObterDoacoesPorCampanhaQuery(request.GuidCampanha);

        var result = await _obterDoacoesPorCampanhaQueryHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError("Erro ao criar campanha: {ErrorCode}", BaseLogType.LOG, new { ErrorCode = result.Error });
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Doacoes obtidas com sucesso: {Guid}", BaseLogType.LOG, new { Guid = request.GuidCampanha, Total = result.Value.Doacoes?.Count() ?? 0 });
        return Ok(result.Value);
    }




    /// <summary>
    /// UC-21 - Obter doacoes por usuario
    /// </summary>
    /// <remarks>   
    /// 
    /// Obtem todas as doacoes de uma usuario. Somente Gestor pode acessar
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    /// **Regras de Validação:**
    /// 
    /// * **Email:**
    ///   - `O campo Email é obrigatório.`
    ///   - `O campo Email é invalido.`
    ///  
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>Lista - DoacaoDTO </returns>
    /// <response code="201">Doacoes</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize(Roles = "GESTOR_ONG")]
    [HttpGet("usuario")]
    [ProducesResponseType(typeof(ObterDoacoesPorUsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ObterDoacoesPorUsuario([FromQuery] ObterDoacoesPorUsuarioRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Obtendo doacoes para o usuario: {Email}", BaseLogType.LOG, new { Email = request.Email });

        var command = new ObterDoacoesPorUsuarioQuery(request.Email);

        var result = await _obterDoacoesPorUsuarioQueryHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError("Erro ao criar campanha: {ErrorCode}", BaseLogType.LOG, new { ErrorCode = result.Error });
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Doacoes obtidas com sucesso: {Email}", BaseLogType.LOG, new { Email = request.Email, Total = result.Value.Doacoes?.Count() ?? 0 });
        return Ok(result.Value);
    }



    
    /// <summary>
    /// UC-22 - Obter proprias doacoes
    /// </summary>
    /// <remarks>   
    /// 
    /// Obtem todas as doacoes do usuario logado
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    ///  
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>Lista DoacaoDTO</returns>
    /// <response code="201">Doacoes</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize(Roles = "GESTOR_ONG, DOADOR")]
    [HttpGet("self")]
    [ProducesResponseType(typeof(ObterDoacoesPorUsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ObterPropriasDoacoes(CancellationToken ct)
    {
        Guid guidLogado = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst("sub")?.Value
                                    ?? throw new UnauthorizedAccessException("Usuário não autenticado ou Guid inválido.")
                                    );

        var emailLogado = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                         ?? User.FindFirst("email")?.Value;

        var request = new ObterDoacoesPorUsuarioRequest(emailLogado);
        _logger.LogInformation("Obtendo doacoes para o usuario: {Email}", BaseLogType.LOG, new { Email = emailLogado });

        var command = new ObterDoacoesPorUsuarioQuery(request.Email);

        var result = await _obterDoacoesPorUsuarioQueryHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError("Erro ao criar campanha: {ErrorCode}", BaseLogType.LOG, new { ErrorCode = result.Error });
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Doacoes obtidas com sucesso: {Email}", BaseLogType.LOG, new { Email = emailLogado, Total = result.Value.Doacoes?.Count() ?? 0 });
        return Ok(result.Value);
    }

    
}
