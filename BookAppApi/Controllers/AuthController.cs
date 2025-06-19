using Microsoft.AspNetCore.Mvc;
using BookAppApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using BookAppApi.Services;

namespace BookAppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly TokenStoreService _tokenStore;
    private readonly UserService _userService;



    public AuthController(IConfiguration config, TokenStoreService tokenStore, UserService userService)
    {
        _config = config;
        _tokenStore = tokenStore;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userService.GetByUsernameAsync(model.Username);
        if (user == null)
        {
            return Unauthorized(new { success = false, message = "Invalid username" });
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Unauthorized(new { success = false, message = "Invalid password" });
        }
        var jwtConfig = _config.GetSection("JwtSettings");
        var secretKey = jwtConfig["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: new[] { new Claim(ClaimTypes.Name, model.Username) },
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["ExpiryMinutes"])),
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Guid.NewGuid().ToString();
        await _tokenStore.SaveAsync(refreshToken, model.Username, DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["RefreshExpiryMinutes"])));

        return Ok(new
        {
            success = true,
            token = accessToken,
            refreshToken,
            username = model.Username
        });
    }


    // refresh token
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var (success, username) = await _tokenStore.TryGetUserAsync(request.RefreshToken);
        if (success)
        {
            var jwtConfig = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: new[] { new Claim(ClaimTypes.Name, username!) },
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["ExpiryMinutes"])),
                signingCredentials: creds
            );
            var refreshToken = Guid.NewGuid().ToString();
            await _tokenStore.UpdateTokenAsync(request.RefreshToken, refreshToken, DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["RefreshExpiryMinutes"])));

            return Ok(new
            {
                success = true,
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken,
                username
            });
        }
        return Unauthorized(new { success = false, message = "Invalid refresh token" });
    }



    [Authorize]
    [HttpGet("secret")]
    public IActionResult Secret()
    {
        return Ok("You accessed a protected route!");
    }

    // register new user
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginModel model)
    {
        var existingUser = await _userService.GetByUsernameAsync(model.Username);
        if (existingUser != null)
        {
            return BadRequest(new { success = false, message = "Username already exists" });
        }

        var user = new User
        {
            Username = model.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        await _userService.CreateAsync(user);
        return Ok(new { success = true, message = "User registered successfully" });
    }
    
    // Check if username already exists
    [HttpGet("exists/{username}")]
    public async Task<IActionResult> CheckUsernameExists(string username)
    {
        var user = await _userService.GetByUsernameAsync(username);
        return Ok(user != null);
    }

   
}
