using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Services.Interfaces;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost("atribuir-role")]
        public async Task<IActionResult> AtribuirRole([FromBody] AtribuirRoleDto dto)
        {
            var sucesso = await _roleService.AtribuirRoleAsync(dto.Email, dto.Role);
            if (!sucesso)
            {
                return NotFound(new { message = "Usuário não encontrado ou role inválida." });
            }
            return Ok(
                new
                {
                    message = $"Role '{dto.Role}' atribuída com sucesso para o usuário '{dto.Email}'.",
                }
            );
        }
    }
}
