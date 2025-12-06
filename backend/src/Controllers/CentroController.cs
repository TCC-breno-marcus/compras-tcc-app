using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Services.Interfaces;

namespace ComprasTccApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CentroController : ControllerBase
    {
        private readonly ILogger<CentroController> _logger;

        private readonly ICentroService _centroService;

        public CentroController(ILogger<CentroController> logger, ICentroService centroService)
        {
            _logger = logger;
            _centroService = centroService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CentroDto>), 200)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> Get([FromQuery] string? nome, [FromQuery] string? sigla)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar centros com filtros.");

                var result = await _centroService.GetAllCentrosAsync(nome, sigla);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro não tratado no endpoint GetAllGetAllCentros."
                );
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CentroDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> GetCentroPorId([FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar um centro pelo ID.");

                var centro = await _centroService.GetCentroByIdAsync(id);

                if (centro == null)
                    return NotFound(new { message = $"Centro com ID {id} não encontrado." });

                return Ok(centro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetCentroByID.");
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }

        [HttpGet("relatorios/gastos-por-centro")]
        [ProducesResponseType(typeof(IEnumerable<RelatorioGastosCentroSaidaDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> GetRelatorioGastosPorCentro([FromQuery] RelatorioGastosCentroFiltroDto filtro)
        {
            if (filtro.DataInicio > filtro.DataFim)
                return BadRequest("Data início deve ser menor que data fim.");

            try
            {
                var resultado = await _centroService.GetRelatorioGastosPorCentroAsync(filtro);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gerar relatório de centros. Exceção: {ex.Message}");
            }
        }

        [HttpGet("relatorios/consumo-por-categoria")]
        [ProducesResponseType(typeof(IEnumerable<RelatorioCategoriaSaidaDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> GetRelatorioCategorias([FromQuery] RelatorioCategoriaFiltroDto filtro)
        {
            try
            {
                var resultado = await _centroService.GetRelatorioPorCategoriaAsync(filtro);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gerar relatório de categorias. Exceção: {ex.Message}");
            }
        }
    }
}
