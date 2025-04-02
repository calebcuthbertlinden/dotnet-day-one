# How to get going with .NET and VSCode

## Setup environment

### 1. Install .NET 9 SDK
```
brew install dotnet
dotnet --version  # Should return 9.x.x  
```

### 2. Install C# extension for VS Code

## Create .NET web api project

### 3. Setup a new .NET API
```
dotnet new webapi -o MyProject
cd MyProject
```

### 4. Run the API locally:
```
dotnet run
```

## Add Authentication (JWT)

### 5. Install authentication dependencies:
```
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.IdentityModel.Tokens
```

### 6. Open Program.cs and add JWT authentication:
```
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var key = Encoding.ASCII.GetBytes("MySuperSecretKeyForJwt"); // Replace with a strong key

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### 7. Create a DTO for login requests (Models/AuthRequest.cs):
```
public class AuthRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

### 8. Create an Authentication Controller (Controllers/AuthController.cs):
```
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string SECRET_KEY = "MySuperSecretKeyForJwt"; // Replace with a strong key

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthRequest request)
    {
        if (request.Username == "admin" && request.Password == "password")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SECRET_KEY);
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
```

### 9. Modify Program.cs to require authentication:
```
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();
```

## Test 

### 10. Start the API
```
dotnet run
```

### 11. Send a login request
```
curl -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d '{"username": "admin", "password": "password"}'
```

### 12. Use the token to access the protected route:
```
curl -X GET http://localhost:5000/api/auth/protected -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Azure setup

### 13. Install Azure CLI
```
brew install azure-cli
az login
```
### 14. Create an Azure App Service
> NB - this didn't work as planned - I had to manually create this in Azure console
```
az group create --name MyResourceGroup --location eastus
az appservice plan create --name MyAppPlan --resource-group MyResourceGroup --sku B1 --is-linux
az webapp create --resource-group MyResourceGroup --plan MyAppPlan --name MyAuthApi --runtime "DOTNET:8.0"
```

## Deploy API

### 15. Publish the project:
```
dotnet publish -c Release -o ./publish
cd publish
zip -r ../dotnet-api.zip . 
cd ..
```

### 16. Deploy it
```
az webapp deployment source config-zip --resource-group MyResourceGroup --name MyAuthApi --src out
```

### 17. Get the public URL
```
az webapp show --resource-group MyResourceGroup --name MyAuthApi --query "defaultHostName" -o tsv
```
