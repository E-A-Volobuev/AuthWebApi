using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userName, List<string> roles, List<Claim>? extraClaims = null)
    {
        var jwt = _configuration.GetSection("Jwt");
        var symmetricKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Secret"]!));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        // Дополнительные произвольные claim'ы (например, BranchId — для проверки в кастомных политиках)
        if (extraClaims is not null)
        {
            claims.AddRange(extraClaims);
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = jwt["Issuer"],
            Audience = jwt["Audience"],
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireInMinutes"] ?? "60")),
            SigningCredentials = new SigningCredentials(
                symmetricKey,
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
