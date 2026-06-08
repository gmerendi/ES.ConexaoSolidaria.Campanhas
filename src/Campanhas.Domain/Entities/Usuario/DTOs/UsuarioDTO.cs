using System.Diagnostics.CodeAnalysis;

namespace Campanhas.Domain.Entities.Usuarios.DTO
{
    public class UsuarioDTO
    {
        // ✅ IMUTÁVEL: O 'init' permite que o Serializador escreva UMA VEZ na criação,
        // mas impede qualquer código de alterar o valor depois (bloqueia injeção de dados).
        public Guid Guid { get; init; }
        public string NomeCompleto { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Cpf { get; init; } = string.Empty;
        public string Perfil { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;

        public UsuarioDTO() { }


        [SetsRequiredMembers]
        public UsuarioDTO(Guid guid, string nomeCompleto, string cpf, string email, string perfil, string status)
        {
            Guid = guid;
            NomeCompleto = nomeCompleto;
            Cpf = cpf;
            Email = email;
            Perfil = perfil;
            Status = status;
        }
    }
}