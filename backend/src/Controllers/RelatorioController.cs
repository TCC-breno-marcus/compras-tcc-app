using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Services.Interfaces;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Gestor")]
    public class RelatorioController : ControllerBase
    {
        private readonly IRelatorioService _relatorioService;
        private readonly ILogger<RelatorioController> _logger;

        public RelatorioController(
            IRelatorioService roleService,
            ILogger<RelatorioController> logger
        )
        {
            _relatorioService = roleService;
            _logger = logger;
        }

        [HttpGet("itens-departamento")]
        [ProducesResponseType(typeof(PaginatedResultDto<ItemPorDepartamentoDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetItensPorDepartamento(
            [FromQuery] string? searchTerm,
            [FromQuery] string? categoriaNome,
            [FromQuery] string? itemsType,
            [FromQuery] string? siglaDepartamento,
            [FromQuery] bool? somenteSolicitacoesAtivas,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50
        )
        {
            try
            {
                _logger.LogInformation(
                    "Recebida requisição para gerar relatório de itens solicitados por departamento."
                );
                var resultado = await _relatorioService.GetItensPorDepartamentoAsync(
                    searchTerm,
                    categoriaNome,
                    itemsType,
                    siglaDepartamento,
                    somenteSolicitacoesAtivas,
                    sortOrder,
                    pageNumber,
                    pageSize
                );
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro não tratado no endpoint GetItensPorDepartamento."
                );
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }

        [HttpGet("itens-departamento/{itemsType}/exportar")]
        public async Task<IActionResult> ExportarItensPorDepartamento(
            [FromRoute] string itemsType,
            [FromQuery] string formatoArquivo,
            [FromQuery] string? searchTerm,
            [FromQuery] string? categoriaNome,
            [FromQuery] string? siglaDepartamento,
            [FromQuery] bool? somenteSolicitacoesAtivas
        )
        {
            try
            {
                _logger.LogInformation(
                    "Recebida requisição para exportar relatório de itens solicitados por departamento."
                );

                bool isGeral = itemsType.Equals("geral", StringComparison.OrdinalIgnoreCase);
                if (
                    !isGeral && !itemsType.Equals("patrimonial", StringComparison.OrdinalIgnoreCase)
                )
                {
                    return BadRequest(
                        new { message = "O tipo de itens deve ser 'geral' ou 'patrimonial'." }
                    );
                }

                if (formatoArquivo.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    var csvBytes = await _relatorioService.ExportarItensPorDepartamentoAsync(
                        itemsType,
                        formatoArquivo,
                        searchTerm,
                        categoriaNome,
                        siglaDepartamento,
                        somenteSolicitacoesAtivas
                    );
                    return File(
                        csvBytes,
                        "text/csv",
                        $"relatorio-itens-por-departamento-{DateTime.Now:yyyyMMdd-HHmmss}.csv"
                    );
                }
                else if (formatoArquivo.Equals("excel", StringComparison.OrdinalIgnoreCase))
                {
                    var excelBytes = await _relatorioService.ExportarItensPorDepartamentoAsync(
                        itemsType,
                        formatoArquivo,
                        searchTerm,
                        categoriaNome,
                        siglaDepartamento,
                        somenteSolicitacoesAtivas
                    );
                    return File(
                        excelBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"relatorio-{itemsType}-{DateTime.Now:yyyyMMdd}.xlsx"
                    );
                }
                else if (formatoArquivo.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var usuarioSolicitante =
                        User.FindFirstValue(ClaimTypes.Name)
                        ?? User.Identity?.Name
                        ?? "Usuario nao identificado";
                    var dataHoraSolicitacao = DateTimeOffset.Now;

                    var pdfBytes = await _relatorioService.ExportarItensPorDepartamentoAsync(
                        itemsType,
                        formatoArquivo,
                        searchTerm,
                        categoriaNome,
                        siglaDepartamento,
                        somenteSolicitacoesAtivas,
                        usuarioSolicitante,
                        dataHoraSolicitacao
                    );

                    return File(
                        pdfBytes,
                        "application/pdf",
                        $"relatorio-itens-por-departamento-{DateTime.Now:yyyyMMdd-HHmmss}.pdf"
                    );
                }
                else
                {
                    return BadRequest(
                        new
                        {
                            message = "O formatoArquivo de arquivo deve ser 'csv', 'excel' ou 'pdf'.",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro não tratado no endpoint GetAllItensPorDepartamentoCsv."
                );
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }

        [HttpGet("relatorios/itens-por-departamento")]
        [ProducesResponseType(typeof(List<RelatorioItemSaidaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRelatorioItensPorDepartamento([FromQuery] RelatorioItensFiltroDto filtro)
        {
            if (string.IsNullOrEmpty(filtro.SiglaDepartamento))
                return BadRequest("A sigla do departamento é obrigatória para este relatório.");
            

            if (filtro.DataInicio > filtro.DataFim)
                return BadRequest("A data de início não pode ser maior que a data fim.");
          

            try
            {
                var resultado = await _relatorioService.GetRelatorioItensPorDepartamentoAsync(filtro);

                Response.Headers.Append("X-Total-Registros", resultado.Count.ToString());

                return Ok(resultado);
            }
            catch (Exception) { return StatusCode(500, "Ocorreu um erro ao gerar o relatório.");}
        }
    }
}
