using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.JwtProvider;

public interface IJwtProvider
{
    string GenerateToken(User user);
}