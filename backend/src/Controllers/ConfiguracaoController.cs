using System.Security.Claims;
using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracaoController : ControllerBase
{
    private readonly IConfiguracaoService _configService;
    private readonly AppDbContext _context;

    public ConfiguracaoController(IConfiguracaoService configService, AppDbContext context)
    {
        _configService = configService;
        _context = context;
    }

    [HttpGet("prazo-submissao")]
    [Authorize(Roles = "Solicitante,Gestor,Admin")]
    public async Task<IActionResult> GetPrazoSubmissao()
    {
        var data = await _configService.GetPrazoSubmissaoAsync();
        if (data == null)
            return NotFound("Prazo para submissão ainda não definido.");
        return Ok(new { prazoSubmissao = data });
    }

    [HttpPut("prazo-submissao")]
    [Authorize(Roles = "Gestor,Admin")]
    public async Task<IActionResult> SetPrazoSubmissao([FromBody] UpdatePrazoDto dto)
    {
        await _configService.SetPrazoSubmissaoAsync(dto.NovaData);
        return NoContent();
    }
}
