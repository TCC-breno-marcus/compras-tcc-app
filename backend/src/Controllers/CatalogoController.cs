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
        [ProducesResponseType(typeof(IEnumerable<ItemDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllItens()
        {
            try
            {
                _logger.LogInformation("Recebida requisição para buscar todos os itens.");
                var itensDto = await _catalogoService.GetAllItensAsync();
                return Ok(itensDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado no endpoint GetAllItens.");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor. Por favor, tente novamente mais tarde." });
            }
        }
    }
}