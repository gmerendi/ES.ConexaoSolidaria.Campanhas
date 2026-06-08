using Campanhas.Domain.Shared.Primitives;

namespace Campanhas.Domain.Shared.Interfaces;

public interface IUserContext
{
    SystemUser GetUser();
}