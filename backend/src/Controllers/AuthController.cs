using ComprasTccApp.Backend.DTOs;
using ComprasTccApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComprasTccApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                var novaPessoa = await _authService.RegisterAsync(registerDto);
                return CreatedAtAction(nameof(Register), new { id = novaPessoa.Id }, new { message = "Usuário criado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var token = await _authService.LoginAsync(loginDto);
            if (token == null)
            {
                return Unauthorized(new { message = "Credenciais inválidas." });
            }
            
            return Ok(new { token, message = "Login bem-sucedido!" });
        }
    }
}