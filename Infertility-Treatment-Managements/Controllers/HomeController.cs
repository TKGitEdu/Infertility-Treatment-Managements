using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Infertility_Treatment_Managements.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public HomeController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Infertility Treatment Management API is running!",
                swagger = "/swagger/index.html",
                status = "online"
            });
        }
        [HttpGet("health/db")]
        public async Task<IActionResult> DbHealthCheck()
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return canConnect ? Ok("Database is awake") : StatusCode(503, "Database is sleeping");
        }
    }
}