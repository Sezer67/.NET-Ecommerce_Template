using ECommerce.UserService.Dto;
using ECommerce.UserService.Model;
using ECommerce.UserService.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.UserService.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly AuthService _authService;
    public AuthController(UserDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpGet("allUsers")]
    public async Task<ActionResult> getAllUsers(){
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }
    [HttpPost("register")]
    public async Task<ActionResult> register(UserDto user)
    {
        try
        {
            var newUser = new User
            {
                Username = user.Username,
                PasswordHash = user.Password
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (System.Exception error)
        {
            return BadRequest(new { message = error.InnerException?.Message ?? error.Message });
        }
    }

    [HttpPost("login")]
    public IActionResult login([FromBody] UserDto body)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == body.Username);
            // if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            // {
            //     return Unauthorized();
            // }
            if (user == null || body.Password != user.PasswordHash)
            {
                return Unauthorized();
            }

            var token = _authService.GenerateToken(user);
            return Ok(new { Token = token });
        }
        catch (System.Exception error)
        {

            return BadRequest(new { message = error.InnerException?.Message ?? error.Message });
            ;
        }
    }
}