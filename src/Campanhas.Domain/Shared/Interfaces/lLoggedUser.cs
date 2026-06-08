using Campanhas.Domain.Shared.Primitives;

public interface IUserContext
{
    SystemUser? GetUser();
}