using Microsoft.AspNetCore.Mvc;

namespace Infertility_Treatment_Managements.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
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
    }
}