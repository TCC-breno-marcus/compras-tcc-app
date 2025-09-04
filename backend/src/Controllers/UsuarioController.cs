using ComprasTccApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;

namespace ComprasTccApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResultDto<UserProfileDto>), 200)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] string? role = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? sortOrder = "asc"
        )
        {
            try
            {
                var users = await _usuarioService.GetAllUsersAsync(
                    role,
                    pageNumber,
                    pageSize,
                    sortOrder
                );
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de usuários.");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }
    }
}
