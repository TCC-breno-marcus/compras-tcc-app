using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Services.Interfaces;

namespace ComprasTccApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;

        private readonly IDashboardService _dashboardService;

        public DashboardController(
            ILogger<DashboardController> logger,
            IDashboardService dashboardService
        )
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DashResultDto>), 200)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> Get([FromQuery] string? nome, [FromQuery] string? sigla)
        {
            try
            {
                _logger.LogInformation(
                    "Recebida requisição para buscar dashboards e KPIs da visão do gestor."
                );
                var anoAtual = DateTime.UtcNow.Year;
                var dashboardData = await _dashboardService.GetDashboardDataAsync(anoAtual);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetDashboard.");
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
