using JobFlow.Domain.Entities;

namespace JobFlow.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    RefreshToken GenerateRefreshToken(string ipAddress);
}
