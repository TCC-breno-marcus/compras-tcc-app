using System.Security.Claims;
using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConfiguracaoController : ControllerBase
    {
        private readonly IConfiguracaoService _configService;
        private readonly ILogger<ConfiguracaoController> _logger;

        // Mantenha apenas este construtor
        public ConfiguracaoController(
            IConfiguracaoService configService,
            AppDbContext context,
            ILogger<ConfiguracaoController> logger
        )
        {
            _configService = configService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ConfiguracaoDto), 200)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> GetConfiguracoes()
        {
            try
            {
                var configs = await _configService.GetConfiguracoesAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado ao buscar as configurações.");
                return StatusCode(
                    500,
                    new { message = "Ocorreu um erro interno ao buscar as configurações." }
                );
            }
        }

        [HttpPatch]
        [ProducesResponseType(typeof(ConfiguracaoDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> UpdateConfiguracoes([FromBody] UpdateConfiguracaoDto dto)
        {
            try
            {
                await _configService.UpdateConfiguracoesAsync(dto);
                var configsAtualizadas = await _configService.GetConfiguracoesAsync();
                return Ok(configsAtualizadas);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado ao atualizar as configurações.");
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
