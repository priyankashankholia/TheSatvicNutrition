using Microsoft.AspNetCore.Mvc;
using Nutritionist.Api.Data;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    [HttpPost("initialize")]
    public async Task<IActionResult> Initialize(
        [FromServices] IServiceProvider services)
    {
        await DatabaseInitializer.InitializeAsync(services);

        return Ok(new
        {
            message = "Database initialized successfully."
        });
    }
}