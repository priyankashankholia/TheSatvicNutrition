using Microsoft.AspNetCore.Mvc;
using Nutritionist.Api.Models;
using Nutritionist.Api.Services;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NutritionController : ControllerBase
{
    private readonly NutritionAssessmentService _service;

    public NutritionController(NutritionAssessmentService service)
    {
        _service = service;
    }

    [HttpPost("assessment")]
    public ActionResult<NutritionAssessmentResponse> Assess(
        NutritionAssessmentRequest request)
    {
        try
        {
            var result = _service.Assess(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
