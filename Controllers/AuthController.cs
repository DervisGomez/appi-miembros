using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChurchApi.Enums;
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
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var user = await _authService.Register(registerDto);
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var authResponse = await _authService.Login(loginDto);
        return Ok(authResponse);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{userId}/promote")]
    public async Task<IActionResult> PromoteToAdmin(int userId)
    {
        var user = await _authService.PromoteToAdmin(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
    }   