using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Services.Interfaces;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Gestor")]
    public class RelatorioController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;
        private readonly ILogger<RelatorioController> _logger;

        public RelatorioController(
            IRelatorioService roleService,
            ILogger<RelatorioController> logger
        )
        {
            _relatorioService = roleService;
            _logger = logger;
        }

        [HttpGet("itens-departamento")]
        [ProducesResponseType(typeof(PaginatedResultDto<ItemPorDepartamentoDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get(
            [FromQuery] string? searchTerm,
            [FromQuery] string? categoriaNome,
            [FromQuery] string? departamento,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50
        )
        {
            try
            {
                _logger.LogInformation(
                    "Recebida requisição para gerar relatório de itens solicitados por departamento."
                );
                var resultado = await _relatorioService.GetItensPorDepartamentoAsync(
                    searchTerm,
                    categoriaNome,
                    departamento,
                    sortOrder,
                    pageNumber,
                    pageSize
                );
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro não tratado no endpoint GetItensPorDepartamento."
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
