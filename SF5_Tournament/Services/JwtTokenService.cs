using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SF5_Tournament.Models;

namespace SF5_Tournament.Services;

/// <summary>Mints short-lived JWTs carrying the admin identity.</summary>
public class JwtTokenService(IConfiguration config)
{
    public string Issue(User user)
    {
        var secret = config["Jwt:Secret"]
                     ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];
        var expireMinutes = int.Parse(config["Jwt:ExpireMinutes"] ?? "720");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int ExpireMinutes => int.Parse(config["Jwt:ExpireMinutes"] ?? "720");
}
