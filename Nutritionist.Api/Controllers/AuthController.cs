using Microsoft.AspNetCore.Mvc;
using Nutritionist.Api.DTOs;
using Nutritionist.Api.Services;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(new
            {
                message = result.Message
            });

        return Ok(new
        {
            message = result.Message,
            token = result.Token
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(new
            {
                message = result.Message
            });

        return Ok(new
        {
            message = result.Message,
            token = result.Token
        });
    }
}