using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/mock")]
public class MockController : ControllerBase
{

    [HttpGet("protected")]
    public IActionResult ProtectedRoute()
    {
        return Ok(new { message = "Here is another mocked route!" });
    }
}
