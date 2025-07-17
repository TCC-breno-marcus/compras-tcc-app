using ComprasTccApp.Models.Dtos;
using Database;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogoController : ControllerBase
    {
        private readonly ILogger<CatalogoController> _logger;
        private readonly AppDbContext _context;
        private readonly ICatalogoService _catalogoService;

        public CatalogoController
        (
            ILogger<CatalogoController> logger,
            AppDbContext context,
            ICatalogoService catalogoService
        )
        {
            _logger = logger;
            _context = context;
            _catalogoService = catalogoService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResultDto<ItemDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllItens
        (
            [FromQuery] long? id,
            [FromQuery] string? catMat,
            [FromQuery] string? nome,
            [FromQuery] string? descricao,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50
        )  
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar itens com filtros.");

                // Simplesmente passa todos os parâmetros recebidos para o serviço
                var paginatedResult = await _catalogoService.GetAllItensAsync(id, catMat, nome, descricao, isActive, pageNumber, pageSize);

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetAllItens.");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde." });
            }
        }

        [HttpPost("importar")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ImportarItens([FromBody] IEnumerable<ItemImportacaoDto> itensParaImportar)
        {
            if (itensParaImportar == null || !itensParaImportar.Any())
            {
                return BadRequest(new { message = "A lista de itens para importação não pode ser vazia." });
            }

            try
            {
                _logger.LogInformation("Recebida requisição para importar {Count} itens.", itensParaImportar.Count());

                await _catalogoService.ImportarItensAsync(itensParaImportar);

                return Accepted(new { message = "Itens recebidos e agendados para importação." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint ImportarItens.");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor ao importar os itens." });
            }
        }

        [HttpPost("popular-imagens")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(500)]
        // IMPORTANTE: Adicione segurança a este endpoint!
        // Exemplo: [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> PopularImagens()
        {
            try
            {
                _logger.LogInformation("Recebida requisição para popular imagens do catálogo.");

                // Este é o caminho DENTRO do container, que é mapeado pelo volume do Docker
                var caminhoDasImagensNoContainer = "/app/uploads"; // Ou o caminho que você configurou no seu Dockerfile/.NET

                var resultado = await _catalogoService.PopularImagensAsync(caminhoDasImagensNoContainer);

                return Ok(new { message = resultado });
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Diretório de imagens não encontrado.");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint PopularImagens.");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }
    }
}