using System.Security.Claims;
using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitacaoController : ControllerBase
{
  private readonly ISolicitacaoService _solicitacaoService;
  private readonly AppDbContext _context;

  public SolicitacaoController
  (
    ISolicitacaoService solicitacaoService,
    AppDbContext context
  )
  {
    _solicitacaoService = solicitacaoService;
    _context = context;
  }

  [HttpPost("geral")]
  [Authorize(Roles = "Solicitante,Admin")]
  public async Task<IActionResult> CreateSolicitacaoGeral([FromBody] CreateSolicitacaoGeralDto dto)
  {
    var solicitanteId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    try
    {
      var novaSolicitacao = await _solicitacaoService.CreateGeralAsync(dto, solicitanteId);
      return CreatedAtAction(nameof(GetSolicitacaoById), new { id = novaSolicitacao.Id }, novaSolicitacao);
    }
    catch (Exception ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetSolicitacaoById([FromRoute] long id)
  {
    var solicitacao = await _solicitacaoService.GetByIdAsync(id);
    
    if (solicitacao == null) return NotFound(new { message = "Solicitação não encontrada." });

    return Ok(solicitacao);
  }
}