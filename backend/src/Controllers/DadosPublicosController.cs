using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Services.Interfaces;

namespace Controllers
{
    [ApiController]
    [Route("api/dados-publicos")]
    [AllowAnonymous]
    public class DadosPublicosController : ControllerBase
    {
        private readonly IDadosPublicosService _dadosPublicosService;
        private readonly ILogger<DadosPublicosController> _logger;

        public DadosPublicosController(
            IDadosPublicosService dadosPublicosService,
            ILogger<DadosPublicosController> logger
        )
        {
            _dadosPublicosService = dadosPublicosService;
            _logger = logger;
        }

        /// <summary>
        /// Consulta pública de solicitações com filtros e permite retorno em JSON ou exportação em CSV com dados mascarados.
        /// </summary>
        /// <returns>
        /// Resultado paginado em JSON quando formatoArquivo = json (padrão) ou arquivo CSV quando formatoArquivo = csv.
        /// </returns>
        [HttpGet("solicitacoes")]
        [ProducesResponseType(typeof(PublicoSolicitacaoConsultaResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ConsultarSolicitacoes(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] int? statusId,
            [FromQuery] string? statusNome,
            [FromQuery] string? siglaDepartamento,
            [FromQuery] string? categoriaNome,
            [FromQuery] string? itemNome,
            [FromQuery] string? catMat,
            [FromQuery] string? itemsType,
            [FromQuery] decimal? valorMinimo,
            [FromQuery] decimal? valorMaximo,
            [FromQuery] bool? somenteSolicitacoesAtivas,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string formatoArquivo = "json"
        )
        {
            try
            {
                if (formatoArquivo.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    var csvBytes = await _dadosPublicosService.ExportarSolicitacoesCsvAsync(
                        dataInicio,
                        dataFim,
                        statusId,
                        statusNome,
                        siglaDepartamento,
                        categoriaNome,
                        itemNome,
                        catMat,
                        itemsType,
                        valorMinimo,
                        valorMaximo,
                        somenteSolicitacoesAtivas,
                        pageNumber,
                        pageSize
                    );

                    return File(
                        csvBytes,
                        "text/csv; charset=utf-8",
                        $"dados-publicos-solicitacoes-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv"
                    );
                }

                if (!formatoArquivo.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(
                        new
                        {
                            message = "O formatoArquivo deve ser 'json' ou 'csv'.",
                        }
                    );
                }

                var resultado = await _dadosPublicosService.ConsultarSolicitacoesAsync(
                    dataInicio,
                    dataFim,
                    statusId,
                    statusNome,
                    siglaDepartamento,
                    categoriaNome,
                    itemNome,
                    catMat,
                    itemsType,
                    valorMinimo,
                    valorMaximo,
                    somenteSolicitacoesAtivas,
                    pageNumber,
                    pageSize
                );

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar dados públicos de solicitações.");
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }
    }
}
