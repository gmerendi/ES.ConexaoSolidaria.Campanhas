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
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class DoacoesController : ControllerBase
{
    private readonly IBaseLogger<DoacoesController> _logger;
    private readonly IUseCaseHandler<CriarDoacaoCommand, Result<CriarDoacaoResponse>> _criarDoacaoCommandHandler;


    public DoacoesController(IBaseLogger<DoacoesController> logger,
        IUseCaseHandler<CriarDoacaoCommand, Result<CriarDoacaoResponse>> criarDoacaoCommandHandler)
    {
        _logger = logger;
        _criarDoacaoCommandHandler = criarDoacaoCommandHandler;
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
    /// * **Guid:**
    ///   - `O campo Titulo é obrigatório.`
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
        _logger.LogInformation("Iniciando criação de intencao de doacao para a campanha: " + request.Guid, BaseLogType.LOG, request);
        var command = new CriarDoacaoCommand(request.Guid, request.Valor);

        var result = await _criarDoacaoCommandHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError(result.Error, BaseLogType.LOG, result);
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Intencao de doacao criada com sucesso: " + result.Value, BaseLogType.LOG, result.Value);
        return Created("Intencao de doacao criada com sucesso", result);
    }


    /*

    /// <summary>
    /// UC-13 - Visualizar os dados de uma campanha
    /// </summary>
    /// <remarks>  
    /// 
    /// Visualiza os dados de uma campanha no sistema pelo Guid.
    /// 
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    /// **Regras de Validação:**
    /// 
    /// * **Guid:**
    ///   - `O campo Guid é obrigatório.`
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>CampanhaDTO</returns>
    /// <response code="201">Usuário cadastrado com sucesso.</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize(Roles = "GESTOR_ONG")]
    [HttpGet]
    [ProducesResponseType(typeof(ObterCampanhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObterCampanha([FromQuery] ObterCampanhaRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Iniciando busca de campanha: " + request.Guid, BaseLogType.LOG, request);

        var query = new ObterCampanhaQuery(request.Guid);

        var result = await _obterCampanhaQueryHandler.HandleAsync(query, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError(result.Error, BaseLogType.LOG, result);
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Campanha obtida com sucesso: " + result.Value?.Guid, BaseLogType.LOG, result);
        return Ok(result.Value);
    }





    /// <summary>
    /// UC-14 - Listar todas Campanhas Ativas
    /// </summary>
    /// <remarks>   
    /// 
    /// Lista campanhas com status ATIVA no sistema.
    /// Esse endpoint possui paginacao. 
    /// Caso a pagina e tamanho nao sejam especificados, o padrão é pagina 1, tamanho 9999
    /// 
    /// **Esse endpoint não requer autenticacao**
    ///
    ///   
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>List<CampanhaDTO></CampanhaDTO></returns>
    /// <response code="201">Campanhas obtidas com sucesso.</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("todas")]
    [ProducesResponseType(typeof(ObterTodasCampanhasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObterTodasCampanhas([FromQuery] ObterTodasCampanhasRequest request,CancellationToken ct)
    {
        _logger.LogInformation("Iniciando obtencao de campanhas ativas.", BaseLogType.LOG, null);

        var query = new ObterTodasCampanhasQuery(request.Pagina, request.TamanhoPagina);
        var result = await _obterTodasCampanhasQueryHandler.HandleAsync(query ,ct);

        if (!result.IsSuccess)
        {
            _logger.LogError(result.Error, BaseLogType.LOG, result);
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Campanhas ativas obtidas sucesso.", BaseLogType.LOG, result);
        return Ok(result.Value);
    }





    /// <summary>
    /// UC-15 - Cancelar Campanha
    /// </summary>
    /// <remarks>   
    /// 
    /// Cancela campanha com status ATIVA no sistema.
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    /// * **Guid:**
    ///   - `O campo Guid é obrigatório.`
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>true</returns>
    /// <response code="201">Campanha cancelada com sucesso.</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("cancel")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelarCampanha([FromQuery] CancelarCampanhaRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Iniciando cancelamento de campanha ativa.", BaseLogType.LOG, null);

        var command = new CancelarCampanhaCommand(request.Guid);
        var result = await _cancelarCampanhaCommandHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError(result.Error, BaseLogType.LOG, result);
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Campanha cancelada com sucesso.", BaseLogType.LOG, result);
        return Ok("Campanha cancelada com sucesso: " + request.Guid);
    }




    /// <summary>
    /// UC-16 - Concluir Campanha
    /// </summary>
    /// <remarks>   
    /// 
    /// Conclui campanha com status ATIVA no sistema.
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    /// * **Guid:**
    ///   - `O campo Guid é obrigatório.`
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>true</returns>
    /// <response code="201">Campanha concluida com sucesso.</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("concluir")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConcluirCampanha([FromQuery] ConcluirCampanhaRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Iniciando conclusao de campanha ativa.", BaseLogType.LOG, null);

        var command = new ConcluirCampanhaCommand(request.Guid);
        var result = await _concluirCampanhaCommandHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError(result.Error, BaseLogType.LOG, result);
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Campanha concluida com sucesso.", BaseLogType.LOG, result);
        return Ok("Campanha concluida com sucesso: " + request.Guid);
    }





    /// <summary>
    /// UC-17 - Altera Campanha
    /// </summary>
    /// <remarks>   
    /// 
    /// Altera uma campanha de doações já criada.  
    /// A campanha deve estar ativa para ser editada.
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    /// **Regras de Validação:**
    /// 
    /// * **Titulo:**
    ///   - `O campo Titulo é obrigatório.`
    ///   - `O Titulo deve ter no minimo 5 e no máximo 200 caracteres.`
    /// * **Descricao:** 
    ///   - `O campo Descricao é obrigatório.`
    ///   - `A descrição deve ter no máximo 2000 caracteres.`
    /// * **Meta Financeira:** 
    ///   - `O campo Meta Financeira é obrigatório.`
    ///   - `A meta financeira deve ser maior que 0 e menor que 9999999999999999.99.`
    /// * **DataInicio:** 
    ///   - `O campo Data de Inicio é obrigatório.`
    ///   - `A data de inicio `
    /// * **DataTermino:** 
    ///   - `O campo Data de Termino é obrigatório.`
    ///   - `A data de termino não pode ser no passado `
    ///  
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>true</returns>
    /// <response code="201">Campanha criada com sucesso.</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize(Roles = "GESTOR_ONG")]
    [HttpPut]
    [ProducesResponseType(typeof(AlterarCampanhaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AlterarCampanha([FromBody] AlterarCampanhaRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Iniciando alteracao de campanha: " + request.Titulo, BaseLogType.LOG, request);
        var command = new AlterarCampanhaCommand(
            request.Guid,
            request.Titulo,
            request.Descricao,
            request.MetaFinanceira,
            request.DataInicio,
            request.DataFim
            );

        var result = await _alterarCampanhaCommandHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError(result.Error, BaseLogType.LOG, result);
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Campanha alterada com sucesso: " + result.Value.Titulo, BaseLogType.LOG, result.Value);
        return Created("Campanha alterada com sucesso", result);
    }

    

    /// <summary>
    /// UC-18 - Obter por busca avançada
    /// </summary>
    /// <remarks>   
    /// 
    /// Este endpoint utiliza o motor de busca Elasticsearch para encontrar 
    /// campanhas por titulo, descrição, status de campanha, data de inicio e fim.
    /// Suporta tolerância a erros de digitação (Fuzziness) e ordena por relevância.
    /// 
    /// **Esse endpoint requer autenticacao**
    /// 
    /// **Regras de Validação:**
    /// 
    /// * **Busca:**
    ///   - `O campo Busca é obrigatório.`
    ///  
    /// 
    /// </remarks>
    /// <param name="request"></param>
    /// <returns>true</returns>
    /// <response code="201">Campanha criada com sucesso.</response>
    /// <response code="400">Dados Inválidos</response>
    /// <response code="422">Entidade não processada</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize(Roles = "GESTOR_ONG")]
    [HttpGet("busca")]
    [ProducesResponseType(typeof(ObterCampanhaAvancadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ObterCampanhaAvancado([FromQuery] ObterCampanhaAvancadoRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Iniciando busca avancada de campanhas: " + request.Termo, BaseLogType.LOG, request);
        var command = new ObterCampanhaAvancadoQuery(request.Termo);

        var result = await _obterCampanhaAvancadoQueryHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            _logger.LogError(result.Error, BaseLogType.LOG, result);
            return BadRequest(result.Error);
        }

        _logger.LogInformation("Campanhas obtidas com sucesso. ", BaseLogType.LOG, result.Value);
        return Ok(result);
    }
    */
    
}
