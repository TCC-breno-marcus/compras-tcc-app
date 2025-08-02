using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitacaoController : ControllerBase
{
  private readonly ISolicitacaoService _solicitacaoService;

  public SolicitacaoController(ISolicitacaoService solicitacaoService)
  {
    _solicitacaoService = solicitacaoService;
  }

  [HttpPost("geral")]
  [Authorize(Roles = "Solicitante,Admin")]
  public async Task<IActionResult> CreateSolicitacaoGeral(CreateSolicitacaoGeralDto dto)
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
  public async Task<IActionResult> GetSolicitacaoById(long id)
  {
    var solicitacao = await _context.Solicitacoes.Include(s => s.ItemSolicitacao).ThenInclude(si => si.Item).FirstOrDefaultAsync(s => s.Id == id);
    if (solicitacao == null) return NotFound();
    return Ok(solicitacao);
  }
}