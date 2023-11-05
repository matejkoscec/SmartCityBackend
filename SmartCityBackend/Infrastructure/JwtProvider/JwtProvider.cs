using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.JwtProvider;

public class JwtProvider : IJwtProvider
{
    
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        Claim[] claims = new Claim[]
        {
            new Claim("Id", user.Id.ToString() ?? string.Empty),
            new Claim("Email", user.Email ?? string.Empty),
            new Claim("Role", user.Roles.First().Name ?? string.Empty),
            new Claim("PreferredUsername", user.PreferredUsername ?? string.Empty),
            new Claim("GivenName", user.GivenName ?? string.Empty),
            new Claim("FamilyName", user.FamilyName ?? string.Empty),
        };

        SigningCredentials signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"])),
            SecurityAlgorithms.HmacSha256);
        
        JwtSecurityToken token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            claims,
            null,
            DateTime.Now.AddMinutes(10),
            signingCredentials);
        
        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        
        return tokenValue;
    }
}