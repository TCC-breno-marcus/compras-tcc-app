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
    public class CategoriaController : ControllerBase
    {
        private readonly ILogger<CategoriaController> _logger;
        private readonly AppDbContext _context;
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(
            ILogger<CategoriaController> logger,
            AppDbContext context,
            ICategoriaService categoriaService
        )
        {
            _logger = logger;
            _context = context;
            _categoriaService = categoriaService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoriaDto>), 200)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> Get(
            [FromQuery] List<long> id,
            [FromQuery] List<string> nome,
            [FromQuery] string? descricao,
            [FromQuery] bool? isActive
        )
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar categorias com filtros.");

                var result = await _categoriaService.GetAllCategoriasAsync(
                    id,
                    nome,
                    descricao,
                    isActive
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetAllCategorias.");
                return StatusCode(
                    500,
                    new
                    {
                        message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde.",
                    }
                );
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CategoriaDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin, Gestor")]
        public async Task<IActionResult> EditarCategoria(
            int id,
            [FromBody] CategoriaUpdateDto updateDto
        )
        {
            if (updateDto == null)
                return BadRequest("Corpo da requisição vazio.");

            var categoriaAtualizada = await _categoriaService.EditarCategoriaAsync(id, updateDto);

            if (categoriaAtualizada == null)
                return NotFound(new { message = $"Categoria com ID {id} não encontrada." });

            return Ok(categoriaAtualizada);
        }

        [HttpGet("{id}", Name = "GetCategoriaPorId")]
        [ProducesResponseType(typeof(CategoriaDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,Gestor,Solicitante")]
        public async Task<IActionResult> GetCategoriaPorId([FromRoute] long id)
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar uma categoria pelo ID.");

                var categoria = await _categoriaService.GetCategoriaByIdAsync(id);

                if (categoria == null)
                    return NotFound(new { message = $"Categoria com ID {id} não encontrada." });

                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetCategoria.");
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
        [ProducesResponseType(typeof(CategoriaDto), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 409)]
        [Authorize(Roles = "Admin, Gestor")]
        public async Task<IActionResult> CriarCategoria([FromBody] CategoriaDto newCategoriaDto)
        {
            if (newCategoriaDto == null)
            {
                return BadRequest("O corpo da requisição não pode ser vazio.");
            }

            try
            {
                _logger.LogInformation("Recebida requisição para criar uma categoria.");

                var categoriaCriadaDto = await _categoriaService.CriarCategoriaAsync(
                    newCategoriaDto
                );

                return CreatedAtAction(
                    "GetCategoriaPorId",
                    new { id = categoriaCriadaDto.Id },
                    categoriaCriadaDto
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado no endpoint CriarCategoria.");
                return StatusCode(500, new { message = "Erro interno ao criar a categoria." });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin, Gestor")]
        public async Task<IActionResult> DeleteCategoria(long id)
        {
            try
            {
                _logger.LogInformation(
                    "Recebida requisição para deletar a categoria com ID: {Id}",
                    id
                );

                var sucesso = await _categoriaService.DeleteCategoriaAsync(id);

                if (!sucesso)
                {
                    return NotFound(new { message = $"Categoria com ID {id} não encontrada." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado no endpoint DeleteCategoria.");
                return StatusCode(500, new { message = "Erro interno ao deletar a categoria." });
            }
        }
    }
}
