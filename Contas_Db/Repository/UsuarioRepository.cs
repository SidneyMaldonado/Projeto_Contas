using Contas_Db.Model;
using Microsoft.EntityFrameworkCore;

namespace Contas_Db.Repository;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    private readonly ContasDbContext _context;

    public UsuarioRepository(ContasDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.Ativo);
    }
}
