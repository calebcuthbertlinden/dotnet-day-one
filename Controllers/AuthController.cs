using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string SECRET_KEY = "gQLP8zRBwmHU/iUXeD1ujHVOP5q/dBe6t+8KBGC5Om8="; // Replace with a strong key

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthRequest request)
    {
        if (request.Username == "admin" && request.Password == "password")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(SECRET_KEY);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, request.Username) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
        return Unauthorized();
    }

    [HttpGet("protected")]
    public IActionResult ProtectedRoute()
    {
        return Ok(new { message = "You accessed a protected route!" });
    }
}
