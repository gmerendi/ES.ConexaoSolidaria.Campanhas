using Campanhas.Domain.Entities.Usuarios.DTO;

public interface IUserContext
{
    UsuarioDTO? GetUser();
}