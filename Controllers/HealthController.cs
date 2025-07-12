using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly CoachPrimeContext _context;

        public HealthController(CoachPrimeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        timestamp = DateTime.UtcNow,
                        checks = new
                        {
                            database = "unhealthy"
                        }
                    });
                }

                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    checks = new
                    {
                        database = "healthy"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message,
                    checks = new
                    {
                        database = "unhealthy"
                    }
                });
            }
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // More comprehensive health check
                var canConnect = await _context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    return StatusCode(503, new
                    {
                        status = "not ready",
                        timestamp = DateTime.UtcNow,
                        reason = "Database connection failed"
                    });
                }

                // You can add more checks here like:
                // - External service connectivity
                // - File system access
                // - Memory usage
                // - Disk space

                return Ok(new
                {
                    status = "ready",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "not ready",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }
    }
}