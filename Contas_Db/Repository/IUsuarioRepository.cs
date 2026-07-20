using Contas_Db.Model;

namespace Contas_Db.Repository;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<bool> EmailExisteAsync(string email);
}
