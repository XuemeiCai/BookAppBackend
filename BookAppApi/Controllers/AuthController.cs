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

    public AuthController(IConfiguration config, TokenStoreService tokenStore)
    {
        _config = config;
        _tokenStore = tokenStore;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        if (model.Username == "user" && model.Password == "password")
        {
            // Load JWT configuration
            var jwtConfig = _config.GetSection("JwtSettings");
            var secretKey = jwtConfig["SecretKey"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Generate JWT access token
            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: new[] { new Claim(ClaimTypes.Name, model.Username) },
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["ExpiryMinutes"])),
                signingCredentials: creds
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Generate refresh token
            var refreshToken = Guid.NewGuid().ToString();
            _tokenStore.Save(refreshToken, model.Username);

            return Ok(new
            {
                success = true,
                token = accessToken,
                refreshToken,
                username = model.Username
            });
        }

        return Unauthorized(new { success = false, message = "Invalid credentials" });
    }

    // refresh token
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        if (_tokenStore.TryGetUser(request.RefreshToken, out string username))
        {
            var jwtConfig = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: new[] { new Claim(ClaimTypes.Name, username) },
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtConfig["ExpiryMinutes"])),
                signingCredentials: creds
            );

            return Ok(new
            {
                success = true,
                token = new JwtSecurityTokenHandler().WriteToken(token),
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

   
}
