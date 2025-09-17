using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Services.Interfaces;

namespace ComprasTccApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartamentoController : ControllerBase
    {
        private readonly ILogger<DepartamentoController> _logger;

        private readonly IDepartamentoService _departamentoService;

        public DepartamentoController(
            ILogger<DepartamentoController> logger,
            IDepartamentoService departamentoService
        )
        {
            _logger = logger;
            _departamentoService = departamentoService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartamentoDto>), 200)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> Get(
            [FromQuery] string? nome,
            [FromQuery] string? sigla,
            [FromQuery] string? siglaCentro
        )
        {
            try
            {
                _logger.LogInformation(
                    "Recebida requisição para buscar departamentos com filtros."
                );

                var result = await _departamentoService.GetAllDepartamentosAsync(
                    nome,
                    sigla,
                    siglaCentro
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro não tratado no endpoint GetAllGetAllDepartamentos."
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
        [ProducesResponseType(typeof(DepartamentoDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> GetDepartamentoPorId([FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar um departamento pelo ID.");

                var depto = await _departamentoService.GetDepartamentoByIdAsync(id);

                if (depto == null)
                    return NotFound(new { message = $"Departamento com ID {id} não encontrado." });

                return Ok(depto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro não tratado no endpoint GetDepartamentoByID."
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
    }
}
