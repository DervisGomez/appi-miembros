using Microsoft.AspNetCore.Authorization;
namespace ChurchApi.Controllers;

using ChurchApi.Services;
using Microsoft.AspNetCore.Mvc;
using ChurchApi.Dtos;

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
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var user = await _authService.Register(registerDto);
        return Created($"/api/auth/users/{user.Id}", user);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var authResponse = await _authService.Login(loginDto);
        return Ok(authResponse);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{userId}/promote")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PromoteToAdmin(int userId)
    {
        var user = await _authService.PromoteToAdmin(userId);
        return Ok(user);
    }
}
