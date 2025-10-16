using System.ComponentModel.DataAnnotations;
using Database;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Microsoft.Extensions.Logging; // Adicione este using se estiver faltando

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase 
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HealthCheckController> _logger;
        private readonly IEmailService _emailService;

        public HealthCheckController(
            AppDbContext context,
            ILogger<HealthCheckController> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult GetAppHealth()
        {
            return Ok(new { status = "API is Healthy" });
        }

        [HttpGet("database")]
        public async Task<IActionResult> GetDbHealth()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (canConnect)
                {
                    return Ok(new { status = "Database connection is Healthy" });
                }

                return StatusCode(503, new { status = "Database connection is Unhealthy" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "Database connection has failed", error = ex.Message });
            }
        }

        [HttpPost("testar-email")]
        public async Task<IActionResult> TestarEnvioEmail(
            [FromQuery, Required, EmailAddress] string destinatario)
        {
            try
            {
                await _emailService.EnviarEmailHealthCheckAsync(destinatario);
                return Ok(new { message = $"E-mail de teste enviado com sucesso para {destinatario}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao tentar enviar e-mail de teste para {Destinatario}", destinatario);
                return StatusCode(500, new { message = "Ocorreu um erro ao enviar o e-mail.", error = ex.Message });
            }
        }
    }
}