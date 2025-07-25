using ComprasTccApp.Models.Dtos;
using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> Get
        (
            [FromQuery] long? id,
            [FromQuery] string? catMat,
            [FromQuery] string? nome,
            [FromQuery] string? descricao,
            [FromQuery] string? especificacao,
            [FromQuery] bool? isActive,
            [FromQuery] string? searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? sortOrder = "asc"
        )
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar itens com filtros.");

                var paginatedResult = await _catalogoService.GetAllItensAsync(id, catMat, nome, descricao, especificacao, isActive, searchTerm, pageNumber, pageSize, sortOrder);

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
        [Authorize(Roles = "Admin")]
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

                return Created("", new { message = "Itens importados com sucesso." });
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
        [Authorize(Roles = "Admin")]
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

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ItemDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarItem(int id, [FromBody] ItemUpdateDto updateDto)
        {
            if (updateDto == null)
                return BadRequest("Corpo da requisição vazio.");

            var itemAtualizado = await _catalogoService.EditarItemAsync(id, updateDto);

            if (itemAtualizado == null)
                return NotFound(new { message = $"Item com ID {id} não encontrado." });

            return Ok(itemAtualizado);
        }

        [HttpGet("{id}", Name = "GetItemPorId")]
        [ProducesResponseType(typeof(ItemDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> GetItemPorId([FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar um item pelo ID.");

                var item = await _catalogoService.GetItemByIdAsync(id);

                if (item == null)
                    return NotFound(new { message = $"Item com ID {id} não encontrado." });

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetItem.");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde." });
            }
        }

        [HttpGet("{id}/itens-semelhantes")]
        [ProducesResponseType(typeof(IEnumerable<ItemDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetItensSemelhantes([FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar itens semelhantes ao item ID {Id}.", id);

                var itensSemelhantes = await _catalogoService.GetItensSemelhantesAsync(id);

                if (itensSemelhantes == null)
                {
                    return NotFound(new { message = $"Item com ID {id} não encontrado para basear a busca." });
                }

                return Ok(itensSemelhantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetItensSemelhantes.");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde." });
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(ItemDto), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 409)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarItem([FromBody] ItemDto newItemDto)
        {
            if (newItemDto == null)
            {
                return BadRequest("O corpo da requisição não pode ser vazio.");
            }

            try
            {
                _logger.LogInformation("Recebida requisição para criar um item.");

                var itemCriadoDto = await _catalogoService.CriarItemAsync(newItemDto);

                return CreatedAtAction("GetItemPorId", new { id = itemCriadoDto.Id }, itemCriadoDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado no endpoint CriarItem.");
                return StatusCode(500, new { message = "Erro interno ao criar o item." });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(long id)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para deletar o item com ID: {Id}", id);

                var sucesso = await _catalogoService.DeleteItemAsync(id);

                if (!sucesso)
                {
                    return NotFound(new { message = $"Item com ID {id} não encontrado." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado no endpoint DeleteItem.");
                return StatusCode(500, new { message = "Erro interno ao deletar o item." });
            }
        }
    }
}