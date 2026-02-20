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
        /// Consulta pública de solicitações com filtros de período, status, departamento e itens, retornando dados mascarados.
        /// </summary>
        /// <returns>Resultado paginado com dados de solicitações e itens sem campos sensíveis em claro.</returns>
        [HttpGet("solicitacoes")]
        [ProducesResponseType(typeof(PublicoSolicitacaoConsultaResultDto), 200)]
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
            [FromQuery] int pageSize = 25
        )
        {
            try
            {
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
