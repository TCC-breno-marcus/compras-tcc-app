using System.Security.Claims;
using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitacaoController : ControllerBase
{
    private readonly ISolicitacaoService _solicitacaoService;
    private readonly AppDbContext _context;

    public SolicitacaoController(ISolicitacaoService solicitacaoService, AppDbContext context)
    {
        _solicitacaoService = solicitacaoService;
        _context = context;
    }

    [HttpPost("geral")]
    [Authorize(Roles = "Solicitante,Admin")]
    public async Task<IActionResult> CreateSolicitacaoGeral(
        [FromBody] CreateSolicitacaoGeralDto dto
    )
    {
        var pessoaId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var novaSolicitacao = await _solicitacaoService.CreateGeralAsync(dto, pessoaId);
            return CreatedAtAction(
                nameof(GetSolicitacaoById),
                new { id = novaSolicitacao.Id },
                novaSolicitacao
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("patrimonial")]
    [Authorize(Roles = "Solicitante,Admin")]
    public async Task<IActionResult> CreateSolicitacaoPatrimonial(
        CreateSolicitacaoPatrimonialDto dto
    )
    {
        var pessoaId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var novaSolicitacao = await _solicitacaoService.CreatePatrimonialAsync(dto, pessoaId);
            return CreatedAtAction(
                nameof(GetSolicitacaoById),
                new { id = novaSolicitacao.Id },
                novaSolicitacao
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}", Name = "GetSolicitacaoById")]
    [Authorize(Roles = "Solicitante,Gestor,Admin")]
    public async Task<IActionResult> GetSolicitacaoById([FromRoute] long id)
    {
        var solicitacaoDto = await _solicitacaoService.GetByIdAsync(id);

        if (solicitacaoDto == null)
            return NotFound(new { message = "Solicitação não encontrada." });

        return Ok(solicitacaoDto);
    }

    [HttpGet("minhas-solicitacoes")]
    [Authorize(Roles = "Solicitante,Admin")]
    public async Task<IActionResult> GetMinhasSolicitacoes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var solicitanteId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var resultadoPaginado = await _solicitacaoService.GetAllBySolicitanteAsync(solicitanteId, pageNumber, pageSize);
        
        return Ok(resultadoPaginado);
    }
}
