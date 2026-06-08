using Campanhas.Domain.Entities.Usuarios.DTO;

namespace Campanhas.Domain.Shared.Interfaces;

public interface IUserContext
{
    UsuarioDTO? GetUser();
}