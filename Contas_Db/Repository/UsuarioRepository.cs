using Contas_Db.Model;
using Contas_Db.Repository.Interface;
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

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _context.Usuarios.AnyAsync(u => u.Email == email);
    }
}
