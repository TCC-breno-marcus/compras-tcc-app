using ComprasTccApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;

namespace ComprasTccApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin, Gestor")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var novaPessoa = await _authService.RegisterAsync(registerDto);
                return CreatedAtAction(
                    nameof(Register),
                    new { id = novaPessoa.Id },
                    new { message = "Usuário criado com sucesso!" }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var loginResponse = await _authService.LoginAsync(loginDto);

                if (loginResponse == null)
                {
                    return Unauthorized(new { message = "Email ou senha inválidos." });
                }

                return Ok(loginResponse);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro inesperado durante o login para o email {Email}",
                    loginDto.Email
                );
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userProfile = await _authService.GetMyProfileAsync(HttpContext.User);

            if (userProfile == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            return Ok(userProfile);
        }
    }
}
