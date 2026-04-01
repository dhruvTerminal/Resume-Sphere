using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeAPI.Data;
using ResumeAPI.DTOs;
using ResumeAPI.Models;
using ResumeAPI.Services;

namespace ResumeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { error = "User with this email already exists." });
        }

        var user = new User
        {
            FullName     = request.FullName,
            Email        = request.Email,
            PasswordHash = _authService.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = new UserDetails
            {
                Id       = user.Id,
                FullName = user.FullName,
                Email    = user.Email
            }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return BadRequest(new { error = "Invalid email or password." });
        }

        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return BadRequest(new { error = "Invalid email or password." });
        }

        var token = _authService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = new UserDetails
            {
                Id       = user.Id,
                FullName = user.FullName,
                Email    = user.Email
            }
        });
    }
}
