namespace Campanhas.Domain.Shared.Interfaces;

public interface ICryptoService
{
    bool VerifyPassword(string password, string HashPassword);
}