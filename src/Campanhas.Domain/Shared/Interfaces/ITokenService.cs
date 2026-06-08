namespace Campanhas.Domain.Shared.Interfaces;

public interface ITokenService
{
    TimeSpan GetTokenTimeToExpire(string token);
}   