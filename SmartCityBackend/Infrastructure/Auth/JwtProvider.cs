using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Auth;

public interface IJwtProvider
{
    string GenerateToken(User user);
}

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration) { _configuration = configuration; }

    public string GenerateToken(User user)
    {
        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("Roles", user.Roles.Select(r => r.Name).Aggregate("", (s, r) => s + r + ",")),
        };

        var signingCredentials =
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"])),
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            claims,
            null,
            DateTime.Now.AddMinutes(10),
            signingCredentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenValue;
    }
}