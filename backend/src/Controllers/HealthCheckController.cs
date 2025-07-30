using Database;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet]
        public IActionResult GetAppHealth()
        {
            return Ok(new { status = "API is Healthy" });
        }

        [HttpGet("database")]
        public async Task<IActionResult> GetDbHealth()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    return Ok(new { status = "Database connection is Healthy" });
                }
                else
                {
                    return StatusCode(
                        503,
                        new
                        {
                            status = "Database connection is Unhealthy (CanConnectAsync returned false)",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return StatusCode(
                    503,
                    new { status = "Database connection has failed", error = ex.Message }
                );
            }
        }
    }
}
