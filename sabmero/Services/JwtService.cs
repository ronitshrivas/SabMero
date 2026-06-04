using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using sabmero.Models;

namespace sabmero.Services;

// This class creates JWT tokens.
// A JWT token is like a digital ID card — Flutter sends it with every API request
// so the server knows who is calling and what their role is.
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    // IConfiguration lets us read values from appsettings.json
    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        // Claims = facts stored INSIDE the token (readable by server)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),   // user's ID
            new Claim(ClaimTypes.MobilePhone, user.Phone),               // user's phone
            new Claim(ClaimTypes.Role, user.Role),                       // "Customer", "Admin" etc.
            new Claim(ClaimTypes.Name, user.FullName)                    // full name
        };

        // Sign the token with our secret key from appsettings.json
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(double.Parse(_config["JwtSettings:ExpiryDays"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}