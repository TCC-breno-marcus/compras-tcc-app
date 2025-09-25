using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Services.Interfaces;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CatalogoController : ControllerBase
    {
        private readonly ILogger<CatalogoController> _logger;
        private readonly ICatalogoService _catalogoService;

        public CatalogoController(
            ILogger<CatalogoController> logger,
            ICatalogoService catalogoService
        )
        {
            _logger = logger;
            _catalogoService = catalogoService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResultDto<ItemDto>), 200)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> Get(
            [FromQuery] long? id,
            [FromQuery] string? catMat,
            [FromQuery] string? nome,
            [FromQuery] string? descricao,
            [FromQuery] List<long> categoriaId,
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

                var paginatedResult = await _catalogoService.GetAllItensAsync(
                    id,
                    catMat,
                    nome,
                    descricao,
                    categoriaId,
                    especificacao,
                    isActive,
                    searchTerm,
                    pageNumber,
                    pageSize,
                    sortOrder
                );

                return Ok(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetAllItens.");
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }

        [HttpPost("importar")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportarItens(
            [FromBody] IEnumerable<ItemImportacaoDto> itensParaImportar
        )
        {
            if (itensParaImportar == null || !itensParaImportar.Any())
            {
                return BadRequest(
                    new { message = "A lista de itens para importação não pode ser vazia." }
                );
            }

            try
            {
                _logger.LogInformation(
                    "Recebida requisição para importar {Count} itens.",
                    itensParaImportar.Count()
                );

                await _catalogoService.ImportarItensAsync(itensParaImportar);

                return Created("", new { message = "Itens importados com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint ImportarItens.");
                return StatusCode(
                    500,
                    new { message = "Ocorreu um erro interno no servidor ao importar os itens." }
                );
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

                var caminhoDasImagensNoContainer = "/app/uploads";

                var resultado = await _catalogoService.PopularImagensAsync(
                    caminhoDasImagensNoContainer
                );

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
        [Authorize(Roles = "Admin, Gestor")]
        public async Task<IActionResult> EditarItem(
            [FromRoute] int id,
            [FromBody] ItemUpdateDto updateDto
        )
        {
            if (updateDto == null)
                return BadRequest("Corpo da requisição vazio.");

            var itemAtualizado = await _catalogoService.EditarItemAsync(id, updateDto, User);

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
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }

        [HttpGet("{id}/itens-semelhantes")]
        [ProducesResponseType(typeof(IEnumerable<ItemDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> GetItensSemelhantes([FromRoute] long id)
        {
            try
            {
                _logger.LogInformation(
                    "Recebida requisição para buscar itens semelhantes ao item ID {Id}.",
                    id
                );

                var itensSemelhantes = await _catalogoService.GetItensSemelhantesAsync(id);

                if (itensSemelhantes == null)
                {
                    return NotFound(
                        new { message = $"Item com ID {id} não encontrado para basear a busca." }
                    );
                }

                return Ok(itensSemelhantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro não tratado no endpoint GetItensSemelhantes."
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

        [HttpPost]
        [ProducesResponseType(typeof(ItemDto), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 409)]
        [Authorize(Roles = "Admin, Gestor")]
        public async Task<IActionResult> CriarItem([FromBody] CreateItemDto newItemDto)
        {
            if (newItemDto == null)
            {
                return BadRequest("O corpo da requisição não pode ser vazio.");
            }

            try
            {
                _logger.LogInformation("Recebida requisição para criar um item.");

                var itemCriadoDto = await _catalogoService.CriarItemAsync(newItemDto, User);

                return CreatedAtAction(
                    "GetItemPorId",
                    new { id = itemCriadoDto.Id },
                    itemCriadoDto
                );
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
        [Authorize(Roles = "Admin, Gestor")]
        public async Task<IActionResult> DeleteItem([FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para deletar o item com ID: {Id}", id);

                var (sucesso, mensagem) = await _catalogoService.DeleteItemAsync(id, User);

                if (!sucesso)
                {
                    return NotFound(new { message = mensagem });
                }

                return Ok(new { message = mensagem });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado no endpoint DeleteItem.");
                return StatusCode(500, new { message = "Erro interno ao deletar o item." });
            }
        }

        [HttpPost("{id}/imagem")]
        [ProducesResponseType(typeof(ItemDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AtualizarImagem([FromRoute] long id, IFormFile imagem)
        {
            if (imagem == null || imagem.Length == 0)
            {
                return BadRequest(new { message = "Nenhum arquivo de imagem foi enviado." });
            }

            try
            {
                var itemAtualizado = await _catalogoService.AtualizarImagemAsync(id, imagem, User);
                if (itemAtualizado == null)
                {
                    return NotFound(new { message = $"Item com ID {id} não encontrado." });
                }
                return Ok(itemAtualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar a imagem do item {Id}.", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpDelete("{id}/imagem")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RemoverImagem([FromRoute] long id)
        {
            try
            {
                var sucesso = await _catalogoService.RemoverImagemAsync(id, User);
                if (!sucesso)
                {
                    return NotFound(new { message = $"Item com ID {id} não encontrado." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao remover a imagem do item {Id}.", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpGet("{id}/historico")]
        [ProducesResponseType(typeof(List<HistoricoItemDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> GetHistoricoItem([FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar histórico de um item.");

                var historico = await _catalogoService.GetHistoricoItemAsync(id);

                if (historico == null)
                    return NotFound(new { message = $"Item com ID {id} não encontrado." });

                return Ok(historico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetHistoricoItem.");
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }
    }
}
