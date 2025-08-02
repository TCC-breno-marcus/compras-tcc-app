using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComprasTccApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentosController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetDepartamentos()
        {
            var departamentos = Enum.GetValues(typeof(DepartamentoEnum))
                                    .Cast<DepartamentoEnum>()
                                    .Select(d => d.ToFriendlyString())
                                    .ToList();
            
            return Ok(departamentos);
        }
    }
}